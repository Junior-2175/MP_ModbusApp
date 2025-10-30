using MP_ModbusApp.MP_modbus;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    /// <summary>
    /// Implementacja transportu dla Modbus TCP (z nagłówkiem MBAP).
    /// </summary>
    public sealed class MyModbusTcpTransport : ModbusTransportBase
    {
        private ushort _transactionId = 0;

        public MyModbusTcpTransport(TcpClient client, MainWindow mainWindow)
            : base(client, mainWindow)
        {
        }

        public override async Task<byte[]> SendRequestAsync(byte slaveId, byte functionCode, byte[] pduData)
        {
            // 1. Zbuduj PDU (UnitID + FC + Data)
            byte[] pdu = new byte[2 + pduData.Length];
            pdu[0] = slaveId;
            pdu[1] = functionCode;
            Array.Copy(pduData, 0, pdu, 2, pduData.Length);

            // 2. Zbuduj nagłówek MBAP
            byte[] mbap = new byte[6];
            _transactionId++;
            BinaryPrimitives.WriteUInt16BigEndian(mbap.AsSpan(0, 2), _transactionId);
            // Protocol ID (00 00 dla Modbus)
            // Długość (UnitID + FC + Data)
            BinaryPrimitives.WriteUInt16BigEndian(mbap.AsSpan(4, 2), (ushort)(pdu.Length));

            // 3. Zbuduj pełną ramkę ADU (MBAP + PDU)
            byte[] adu = new byte[mbap.Length + pdu.Length];
            Array.Copy(mbap, 0, adu, 0, mbap.Length);
            Array.Copy(pdu, 0, adu, mbap.Length, pdu.Length);

            // 4. Wyślij
            LogFrame(adu, "TX", slaveId);
            await WriteAsync(adu); // Używa metody z klasy bazowej

            // 5. Odbierz odpowiedź (MBAP)
            byte[] mbapResponse = new byte[6];
            await ReadFullAsync(mbapResponse, 6);

            // 6. Sprawdź Transaction ID
            ushort responseTid = BinaryPrimitives.ReadUInt16BigEndian(mbapResponse.AsSpan(0, 2));
            if (responseTid != _transactionId)
            {
                throw new IOException("Niezgodny Transaction ID w odpowiedzi.");
            }

            // 7. Odbierz resztę (PDU)
            ushort responseLen = BinaryPrimitives.ReadUInt16BigEndian(mbapResponse.AsSpan(4, 2));
            if (responseLen == 0)
            {
                throw new IOException("Serwer zwrócił PDU o długości 0.");
            }

            byte[] pduResponse = new byte[responseLen];
            await ReadFullAsync(pduResponse, responseLen);

            LogFrame(mbapResponse.Concat(pduResponse).ToArray(), "RX", slaveId);

            // 8. Waliduj PDU i zwróć [FC] + [Data]
            return ValidateAndExtractPdu(pduResponse, slaveId, functionCode);
        }
    }
}