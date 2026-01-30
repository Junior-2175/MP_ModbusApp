using MP_ModbusApp.MP_modbus;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    public sealed class MyModbusTcpTransport : ModbusTransportBase
    {
        private ushort _transactionId = 0;

        public MyModbusTcpTransport(TcpClient client, MainWindow mainWindow)
            : base(client, mainWindow)
        {
        }

        // Method to reset the transaction counter (used when restarting polling)
        public void ResetTransactionId()
        {
            _transactionId = 0;
        }

        public override async Task<byte[]> SendRequestAsync(byte slaveId, byte functionCode, byte[] pduData)
        {
            // Lock the transport to prevent other devices from interfering
            await _transactionLock.WaitAsync();
            try
            {
                // CRITICAL FIX: Flush any garbage data from the stream before sending a new request.
                // This handles cases where a previous timeout (e.g. during scan) left data in the buffer.
                if (_stream != null && _stream.DataAvailable)
                {
                    await FlushInputBuffer();
                }

                // 1. Build the PDU
                byte[] pdu = new byte[2 + pduData.Length];
                pdu[0] = slaveId;
                pdu[1] = functionCode;
                Array.Copy(pduData, 0, pdu, 2, pduData.Length);

                // 2. Build the MBAP header
                byte[] mbap = new byte[6];
                _transactionId++;
                BinaryPrimitives.WriteUInt16BigEndian(mbap.AsSpan(0, 2), _transactionId);
                // Protocol ID (00 00) + Length
                BinaryPrimitives.WriteUInt16BigEndian(mbap.AsSpan(4, 2), (ushort)(pdu.Length));

                // 3. Build ADU
                byte[] adu = new byte[mbap.Length + pdu.Length];
                Array.Copy(mbap, 0, adu, 0, mbap.Length);
                Array.Copy(pdu, 0, adu, mbap.Length, pdu.Length);

                // 4. Send request
                LogFrame(adu, "TX", slaveId);
                await WriteAsync(adu);

                // 5. Read MBAP response
                byte[] mbapResponse = new byte[6];
                await ReadFullAsync(mbapResponse, 6);

                // 6. Validate Transaction ID
                ushort responseTid = BinaryPrimitives.ReadUInt16BigEndian(mbapResponse.AsSpan(0, 2));
                if (responseTid != _transactionId)
                {
                    throw new IOException($"Mismatched Transaction ID. Sent: {_transactionId}, Recv: {responseTid}");
                }

                // 7. Read PDU
                ushort responseLen = BinaryPrimitives.ReadUInt16BigEndian(mbapResponse.AsSpan(4, 2));
                if (responseLen == 0) throw new IOException("Zero length PDU.");

                byte[] pduResponse = new byte[responseLen];
                await ReadFullAsync(pduResponse, responseLen);

                LogFrame(mbapResponse.Concat(pduResponse).ToArray(), "RX", slaveId);

                return ValidateAndExtractPdu(pduResponse, slaveId, functionCode);
            }
            finally
            {
                _transactionLock.Release();
            }
        }

        private async Task FlushInputBuffer()
        {
            try
            {
                byte[] discardBuffer = new byte[1024];
                while (_stream != null && _stream.DataAvailable)
                {
                    int read = await _stream.ReadAsync(discardBuffer, 0, discardBuffer.Length);
                    if (read == 0) break;
                }
            }
            catch { /* Ignore errors during flush */ }
        }
    }
}