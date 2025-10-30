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
    /// Implements the Modbus TCP transport, which uses the MBAP header.
    /// </summary>
    public sealed class MyModbusTcpTransport : ModbusTransportBase
    {
        /// <summary>
        /// Tracks the MBAP transaction ID, incrementing for each request.
        /// </summary>
        private ushort _transactionId = 0;

        public MyModbusTcpTransport(TcpClient client, MainWindow mainWindow)
            : base(client, mainWindow)
        {
        }

        /// <summary>
        /// Sends a Modbus request using the Modbus TCP (MBAP) protocol.
        /// </summary>
        public override async Task<byte[]> SendRequestAsync(byte slaveId, byte functionCode, byte[] pduData)
        {
            // 1. Build the PDU (UnitID + FC + Data)
            byte[] pdu = new byte[2 + pduData.Length];
            pdu[0] = slaveId;
            pdu[1] = functionCode;
            Array.Copy(pduData, 0, pdu, 2, pduData.Length);

            // 2. Build the MBAP header
            byte[] mbap = new byte[6];
            _transactionId++;
            BinaryPrimitives.WriteUInt16BigEndian(mbap.AsSpan(0, 2), _transactionId);
            // Protocol ID (00 00 for Modbus)
            // Length (UnitID + FC + Data)
            BinaryPrimitives.WriteUInt16BigEndian(mbap.AsSpan(4, 2), (ushort)(pdu.Length));

            // 3. Build the full ADU (MBAP + PDU)
            byte[] adu = new byte[mbap.Length + pdu.Length];
            Array.Copy(mbap, 0, adu, 0, mbap.Length);
            Array.Copy(pdu, 0, adu, mbap.Length, pdu.Length);

            // 4. Send the request
            LogFrame(adu, "TX", slaveId);
            await WriteAsync(adu); // Uses the base class method

            // 5. Receive the response MBAP header
            byte[] mbapResponse = new byte[6];
            await ReadFullAsync(mbapResponse, 6);

            // 6. Validate the Transaction ID
            ushort responseTid = BinaryPrimitives.ReadUInt16BigEndian(mbapResponse.AsSpan(0, 2));
            if (responseTid != _transactionId)
            {
                throw new IOException("Mismatched Transaction ID in response.");
            }

            // 7. Receive the rest of the response (PDU)
            ushort responseLen = BinaryPrimitives.ReadUInt16BigEndian(mbapResponse.AsSpan(4, 2));
            if (responseLen == 0)
            {
                throw new IOException("Server returned a PDU with zero length.");
            }

            byte[] pduResponse = new byte[responseLen];
            await ReadFullAsync(pduResponse, responseLen);

            LogFrame(mbapResponse.Concat(pduResponse).ToArray(), "RX", slaveId);

            // 8. Validate the PDU and return [FC] + [Data]
            return ValidateAndExtractPdu(pduResponse, slaveId, functionCode);
        }
    }
}