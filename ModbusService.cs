using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MP_ModbusApp
{
    public class ModbusService
    {
        private readonly NetworkStream _stream;
        private ushort _transactionIdCounter = 0;

        public event Action<ModbusFrameLog> FrameDataAvailable;
        public ushort LastTransactionId { get; private set; }

        public ModbusService(NetworkStream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public async Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort quantity, CancellationToken cancellationToken)
        {
            _transactionIdCounter++;
            this.LastTransactionId = _transactionIdCounter;
            byte[] requestFrame = BuildReadRequest(this.LastTransactionId, slaveId, startAddress, quantity);

            FrameDataAvailable?.Invoke(new ModbusFrameLog
            {
                Timestamp = DateTime.Now,
                TransactionID = this.LastTransactionId,
                Direction = "TX",
                DataFrame = ByteArrayToString(requestFrame, requestFrame.Length)
            });

            await _stream.WriteAsync(requestFrame, 0, requestFrame.Length, cancellationToken);

            byte[] responseBuffer = new byte[256];
            int bytesRead = 0;

            try
            {
                // ZMIANA: Użycie CancellationToken dla niezawodnego timeoutu
                bytesRead = await _stream.ReadAsync(responseBuffer, 0, responseBuffer.Length, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Ten wyjątek jest oczekiwany w przypadku timeoutu.
                // Rzucamy go dalej, aby MainWindow mogło go obsłużyć i zalogować.
                throw new Exception("No response from slave device (Timeout).");
            }


            string errorDescription = string.Empty;
            if (bytesRead > 8 && (responseBuffer[7] & 0x80) > 0)
            {
                errorDescription = GetModbusExceptionMessage(responseBuffer[8]);
            }

            if (bytesRead > 0)
            {
                FrameDataAvailable?.Invoke(new ModbusFrameLog
                {
                    Timestamp = DateTime.Now,
                    TransactionID = this.LastTransactionId,
                    Direction = "RX",
                    DataFrame = ByteArrayToString(responseBuffer, bytesRead),
                    ErrorDescription = errorDescription
                });
            }
            else
            {
                // Jeśli z jakiegoś powodu odczytano 0 bajtów, ale nie było timeoutu (np. zamknięcie gniazda)
                throw new Exception("Connection closed by remote host.");
            }

            return ParseReadResponse(responseBuffer, bytesRead, quantity);
        }

        private byte[] BuildReadRequest(ushort transactionId, byte slaveId, ushort startAddress, ushort quantity)
        {
            byte[] frame = new byte[12];
            frame[0] = (byte)(transactionId >> 8);
            frame[1] = (byte)(transactionId & 0xFF);
            frame[2] = 0x00; frame[3] = 0x00;
            frame[4] = 0x00; frame[5] = 0x06;
            frame[6] = slaveId;
            frame[7] = 0x03;
            frame[8] = (byte)(startAddress >> 8);
            frame[9] = (byte)(startAddress & 0xFF);
            frame[10] = (byte)(quantity >> 8);
            frame[11] = (byte)(quantity & 0xFF);
            return frame;
        }

        private ushort[] ParseReadResponse(byte[] response, int length, ushort quantity)
        {
            if (length < 9)
                throw new Exception("Response from server is too short.");

            if ((response[7] & 0x80) > 0)
            {
                string errorMessage = GetModbusExceptionMessage(response[8]);
                throw new Exception($"Modbus device returned an exception: {errorMessage}");
            }

            if (length < 9 + 2 * quantity)
                throw new Exception("Response is too short for the requested number of registers.");

            ushort[] values = new ushort[quantity];
            for (int i = 0; i < quantity; i++)
            {
                values[i] = (ushort)((response[9 + i * 2] << 8) | response[10 + i * 2]);
            }
            return values;
        }

        private string GetModbusExceptionMessage(byte exceptionCode)
        {
            return exceptionCode switch
            {
                0x01 => "Illegal Function",
                0x02 => "Illegal Data Address",
                0x03 => "Illegal Data Value",
                0x04 => "Slave Device Failure",
                0x05 => "Acknowledge",
                0x06 => "Slave Device Busy",
                0x08 => "Memory Parity Error",
                0x0A => "Gateway Path Unavailable",
                0x0B => "Gateway Target Device Failed to Respond",
                _ => $"Unknown Exception Code (0x{exceptionCode:X2})"
            };
        }

        private string ByteArrayToString(byte[] data, int length)
        {
            return BitConverter.ToString(data, 0, length).Replace("-", " ");
        }
    }
}