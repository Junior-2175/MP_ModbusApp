using MP_ModbusApp.MP_modbus;
using System;
using System.Buffers.Binary;
using System.Linq;
using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    public class MyModbusMaster : IMyModbusMaster
    {
        public IMyModbusTransport Transport { get; }

        public MyModbusMaster(IMyModbusTransport transport)
        {
            Transport = transport;
        }

        // Implementacja dla Function Code 03 (Read Holding Registers)
        public async Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort quantity)
        {
            // 1. Zbuduj PDU (bez SlaveID i Function Code)
            byte[] pduData = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), quantity);

            // 2. Wyślij żądanie przez transport i pobierz PDU odpowiedzi
            // Transport zajmie się opakowaniem (MBAP lub CRC/LRC)
            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x03, pduData);

            // 3. Sparsuj PDU odpowiedzi
            // PDU = [FunctionCode (1B)] + [ByteCount (1B)] + [Data (N*2 B)]
            if (responsePdu.Length < 2 || responsePdu.Length != 2 + responsePdu[1])
            {
                throw new Exception("Nieprawidłowa długość odpowiedzi PDU.");
            }

            int byteCount = responsePdu[1];
            if (byteCount != quantity * 2)
            {
                throw new Exception("Otrzymano nieprawidłową liczbę bajtów danych.");
            }

            ushort[] registers = new ushort[quantity];
            for (int i = 0; i < quantity; i++)
            {
                registers[i] = BinaryPrimitives.ReadUInt16BigEndian(responsePdu.AsSpan(2 + i * 2, 2));
            }

            return registers;
        }

        // Implementacja dla Function Code 04 (Read Input Registers)
        public async Task<ushort[]> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort quantity)
        {
            // Identycznie jak dla 03, ale inny kod funkcji
            byte[] pduData = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), quantity);

            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x04, pduData);

            if (responsePdu.Length < 2 || responsePdu.Length != 2 + responsePdu[1])
            {
                throw new Exception("Nieprawidłowa długość odpowiedzi PDU.");
            }

            int byteCount = responsePdu[1];
            if (byteCount != quantity * 2)
            {
                throw new Exception("Otrzymano nieprawidłową liczbę bajtów danych.");
            }

            ushort[] registers = new ushort[quantity];
            for (int i = 0; i < quantity; i++)
            {
                registers[i] = BinaryPrimitives.ReadUInt16BigEndian(responsePdu.AsSpan(2 + i * 2, 2));
            }

            return registers;
        }

        // Implementacja dla Function Code 01 (Read Coils)
        public async Task<bool[]> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort quantity)
        {
            byte[] pduData = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), quantity);

            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x01, pduData);

            if (responsePdu.Length < 2) throw new Exception("Nieprawidłowa długość odpowiedzi PDU.");

            int byteCount = responsePdu[1];
            if (responsePdu.Length != 2 + byteCount) throw new Exception("Niespójna długość odpowiedzi PDU.");

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

        // Implementacja dla Function Code 02 (Read Discrete Inputs)
        public async Task<bool[]> ReadInputsAsync(byte slaveId, ushort startAddress, ushort quantity)
        {
            byte[] pduData = new byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(0, 2), startAddress);
            BinaryPrimitives.WriteUInt16BigEndian(pduData.AsSpan(2, 2), quantity);

            byte[] responsePdu = await Transport.SendRequestAsync(slaveId, 0x02, pduData);

            // Parsowanie identyczne jak dla ReadCoils
            if (responsePdu.Length < 2) throw new Exception("Nieprawidłowa długość odpowiedzi PDU.");
            int byteCount = responsePdu[1];
            if (responsePdu.Length != 2 + byteCount) throw new Exception("Niespójna długość odpowiedzi PDU.");

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

        public void Dispose()
        {
            Transport?.Dispose();
        }
    }
}