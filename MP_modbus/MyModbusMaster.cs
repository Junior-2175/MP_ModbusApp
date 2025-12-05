using MP_ModbusApp.MP_modbus;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    /// <summary>
    /// Implements the IMyModbusMaster interface, providing methods
    /// to read and write Modbus data by orchestrating the transport layer.
    /// </summary>
    public class MyModbusMaster : IMyModbusMaster
    {
        public IMyModbusTransport Transport { get; }

        public MyModbusMaster(IMyModbusTransport transport)
        {
            Transport = transport;
        }

        // --- READ FUNCTIONS (FC 01, 02, 03, 04) ---

        /// <summary>
        /// Implementation for Function Code 03 (Read Holding Registers).
        /// </summary>
        public async Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort quantity)
        {
            // 1. Build PDU data: [Start Address (2B)] + [Quantity (2B)]
            byte[] pduData = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), quantity);

            // 2. Send request via transport and get the response PDU
            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x03, pduData);

            // 3. Parse and validate the response PDU
            // Expected PDU = [FunctionCode (1B)] + [ByteCount (1B)] + [Data (N*2 B)]
            if (responsePdu.Length < 2 || responsePdu.Length != 2 + responsePdu[1])
            {
                throw new IOException("Invalid PDU response length.");
            }

            int byteCount = responsePdu[1];
            if (byteCount != quantity * 2)
            {
                throw new IOException("Received an invalid number of data bytes.");
            }

            // 4. Extract registers
            ushort[] registers = new ushort[quantity];
            for (int i = 0; i < quantity; i++)
            {
                registers[i] = BinaryPrimitives.ReadUInt16BigEndian(responsePdu.AsSpan(2 + i * 2, 2));
            }

            return registers;
        }

        /// <summary>
        /// Implementation for Function Code 04 (Read Input Registers).
        /// </summary>
        public async Task<ushort[]> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort quantity)
        {
            // Logic identical to FC 03
            byte[] pduData = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), quantity);

            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x04, pduData);

            if (responsePdu.Length < 2 || responsePdu.Length != 2 + responsePdu[1])
            {
                throw new IOException("Invalid PDU response length.");
            }

            int byteCount = responsePdu[1];
            if (byteCount != quantity * 2)
            {
                throw new IOException("Received an invalid number of data bytes.");
            }

            ushort[] registers = new ushort[quantity];
            for (int i = 0; i < quantity; i++)
            {
                registers[i] = BinaryPrimitives.ReadUInt16BigEndian(responsePdu.AsSpan(2 + i * 2, 2));
            }

            return registers;
        }

        /// <summary>
        /// Implementation for Function Code 01 (Read Coils).
        /// </summary>
        public async Task<bool[]> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort quantity)
        {
            byte[] pduData = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), quantity);

            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x01, pduData);

            if (responsePdu.Length < 2) throw new IOException("Invalid PDU response length.");
            int byteCount = responsePdu[1];
            if (responsePdu.Length != 2 + byteCount) throw new IOException("Inconsistent PDU response length.");

            // Extract boolean values (coils)
            bool[] coils = new bool[quantity];
            int coilIndex = 0;
            for (int i = 0; i < byteCount; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (coilIndex >= quantity) break;
                    coils[coilIndex] = (responsePdu[2 + i] & (1 << j)) != 0;
                    coilIndex++;
                }
            }
            return coils;
        }

        /// <summary>
        /// Implementation for Function Code 02 (Read Discrete Inputs).
        /// </summary>
        public async Task<bool[]> ReadInputsAsync(byte slaveId, ushort startAddress, ushort quantity)
        {
            // Logic identical to FC 01
            byte[] pduData = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), quantity);

            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x02, pduData);

            if (responsePdu.Length < 2) throw new IOException("Invalid PDU response length.");
            int byteCount = responsePdu[1];
            if (responsePdu.Length != 2 + byteCount) throw new IOException("Inconsistent PDU response length.");

            bool[] inputs = new bool[quantity];
            int inputIndex = 0;
            for (int i = 0; i < byteCount; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (inputIndex >= quantity) break;
                    inputs[inputIndex] = (responsePdu[2 + i] & (1 << j)) != 0;
                    inputIndex++;
                }
            }
            return inputs;
        }

        // --- WRITE FUNCTIONS (FC 05, 06, 15, 16) ---

        /// <summary>
        /// Implementation for Function Code 05 (Write Single Coil).
        /// </summary>
        public async Task WriteSingleCoilAsync(byte slaveId, ushort address, bool value)
        {
            byte[] pduData = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), address);

            // Coil value: 0xFF00 for ON, 0x0000 for OFF
            ushort coilValue = value ? (ushort)0xFF00 : (ushort)0x0000;
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), coilValue);

            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x05, pduData);

            // Response validation (FC 05 echo)
            if (responsePdu.Length != 5)
            {
                throw new IOException("Invalid PDU response length for FC 05.");
            }
            if (!responsePdu.AsSpan(1).SequenceEqual(pduData))
            {
                throw new IOException("FC 05 response echo verification failed.");
            }
        }

        /// <summary>
        /// Implementation for Function Code 06 (Write Single Holding Register).
        /// </summary>
        public async Task WriteSingleRegisterAsync(byte slaveId, ushort address, ushort value)
        {
            byte[] pduData = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), address);
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), value);

            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x06, pduData);

            // Response validation (FC 06 echo)
            if (responsePdu.Length != 5)
            {
                throw new IOException("Invalid PDU response length for FC 06.");
            }
            if (!responsePdu.AsSpan(1).SequenceEqual(pduData))
            {
                throw new IOException("FC 06 response echo verification failed.");
            }
        }

        /// <summary>
        /// Implementation for Function Code 15 (Write Multiple Coils).
        /// </summary>
        public async Task WriteMultipleCoilsAsync(byte slaveId, ushort startAddress, bool[] values)
        {
            ushort quantity = (ushort)values.Length;
            int byteCount = (quantity + 7) / 8;

            byte[] pduData = new byte[4 + 1 + byteCount];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), startAddress); // Start Address
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), quantity);    // Quantity
            pduData[4] = (byte)byteCount;                                             // Byte Count

            // Konwersja bool[] na bajty
            for (int i = 0; i < quantity; i++)
            {
                if (values[i])
                {
                    // Ustaw odpowiedni bit w bajcie (coils są pakowane od bitu 0)
                    pduData[5 + (i / 8)] |= (byte)(1 << (i % 8));
                }
            }

            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x0F, pduData);

            // Weryfikacja odpowiedzi FC 15: [FC (1B)] + [StartAddr (2B)] + [Quantity (2B)]
            if (responsePdu.Length != 5)
            {
                throw new IOException("Invalid PDU response length for FC 15.");
            }

            // Sprawdź, czy adres i ilość są poprawne (echo z zapytania)
            if (BinaryPrimitives.ReadUInt16BigEndian(responsePdu.AsSpan(1, 2)) != startAddress ||
                BinaryPrimitives.ReadUInt16BigEndian(responsePdu.AsSpan(3, 2)) != quantity)
            {
                throw new IOException("FC 15 response echo verification failed.");
            }
        }

        /// <summary>
        /// Implementation for Function Code 16 (Write Multiple Registers).
        /// </summary>
        public async Task WriteMultipleRegistersAsync(byte slaveId, ushort startAddress, ushort[] values)
        {
            ushort quantity = (ushort)values.Length;
            int byteCount = quantity * 2;

            byte[] pduData = new byte[4 + 1 + byteCount];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), startAddress); // Start Address
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), quantity);    // Quantity
            pduData[4] = (byte)byteCount;                                             // Byte Count

            // Konwersja ushort[] na bajty w formacie Big-Endian
            for (int i = 0; i < quantity; i++)
            {
                BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(5 + i * 2, 2), values[i]);
            }

            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x10, pduData);

            // Weryfikacja odpowiedzi FC 16: [FC (1B)] + [StartAddr (2B)] + [Quantity (2B)]
            if (responsePdu.Length != 5)
            {
                throw new IOException("Invalid PDU response length for FC 16.");
            }

            // Sprawdź, czy adres i ilość są poprawne (echo z zapytania)
            if (BinaryPrimitives.ReadUInt16BigEndian(responsePdu.AsSpan(1, 2)) != startAddress ||
                BinaryPrimitives.ReadUInt16BigEndian(responsePdu.AsSpan(3, 2)) != quantity)
            {
                throw new IOException("FC 16 response echo verification failed.");
            }
        }

        /// <summary>
        /// Disposes of the underlying transport.
        /// </summary>
        public void Dispose()
        {
            Transport?.Dispose();
        }
    }
}