using MP_ModbusApp.MP_modbus;
using System;
using System.Buffers.Binary;
using System.IO; // Added for IOException
using System.Linq;
using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    /// <summary>
    /// Implements the IMyModbusMaster interface, providing methods
    /// to read Modbus data by orchestrating the transport layer.
    /// </summary>
    public class MyModbusMaster : IMyModbusMaster
    {
        public IMyModbusTransport Transport { get; }

        public MyModbusMaster(IMyModbusTransport transport)
        {
            Transport = transport;
        }

        /// <summary>
        /// Implementation for Function Code 03 (Read Holding Registers).
        /// </summary>
        public async Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort quantity)
        {
            // 1. Build PDU data (excluding SlaveID and Function Code)
            byte[] pduData = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), quantity);

            // 2. Send request via transport and get the response PDU
            // The transport handles the ADU framing (MBAP or CRC/LRC)
            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x03, pduData);

            // 3. Parse the response PDU
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
            // Logic is identical to ReadHoldingRegisters, just a different function code
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

            // Expected PDU = [FunctionCode (1B)] + [ByteCount (1B)] + [Data (N B)]
            if (responsePdu.Length < 2) throw new IOException("Invalid PDU response length.");

            int byteCount = responsePdu[1];
            if (responsePdu.Length != 2 + byteCount) throw new IOException("Inconsistent PDU response length.");

            // 4. Extract boolean values (coils)
            bool[] coils = new bool[quantity];
            int coilIndex = 0;
            for (int i = 0; i < byteCount; i++)
            {
                for (int j = 0; j < 8; j++) // Iterate through each bit of the byte
                {
                    if (coilIndex >= quantity) break; // Stop if we've read all requested coils
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
            byte[] pduData = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), quantity);

            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x02, pduData);

            // Parsing is identical to ReadCoils
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

        /// <summary>
        /// Disposes of the underlying transport.
        /// </summary>
        public void Dispose()
        {
            Transport?.Dispose();
        }
    }
}