using MP_ModbusApp.MP_modbus;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    /// <summary>
    /// Base class for Modbus transports.
    /// Handles common logic for logging, PDU validation, and reliable stream reading.
    /// </summary>
    public abstract class ModbusTransportBase : IMyModbusTransport
    {
        protected readonly TcpClient _client;
        protected readonly NetworkStream _stream;
        protected readonly MainWindow _mainWindow;

        // Semaphore to synchronize async requests and prevent race conditions
        protected readonly SemaphoreSlim _transactionLock = new SemaphoreSlim(1, 1);

        public string LoggingDeviceName { get; set; } = null;
        public int ReadTimeout { get; set; } = 1000;
        public int WriteTimeout { get; set; } = 1000;

        protected ModbusTransportBase(TcpClient client, MainWindow mainWindow)
        {
            _client = client;
            _stream = client?.GetStream();
            _mainWindow = mainWindow;
        }

        public abstract Task<byte[]> SendRequestAsync(byte slaveId, byte functionCode, byte[] pduData);

        protected async Task ReadFullAsync(byte[] buffer, int count)
        {
            if (_stream == null) throw new InvalidOperationException("Stream is not available for this transport.");

            _stream.ReadTimeout = ReadTimeout;
            int bytesRead = 0;
            while (bytesRead < count)
            {
                int read = await _stream.ReadAsync(buffer, bytesRead, count - bytesRead);
                if (read == 0)
                {
                    throw new IOException("The connection was unexpectedly closed.");
                }
                bytesRead += read;
            }
        }

        protected byte[] ValidateAndExtractPdu(byte[] pduWithUnitId, byte expectedSlaveId, byte expectedFunctionCode)
        {
            if (pduWithUnitId.Length < 2)
            {
                throw new IOException("Response is too short.");
            }

            if (pduWithUnitId[0] != expectedSlaveId)
            {
                throw new IOException($"Received response from wrong Slave ID. Expected: {expectedSlaveId}, Received: {pduWithUnitId[0]}");
            }

            if (pduWithUnitId[1] == (expectedFunctionCode | 0x80))
            {
                byte exceptionCode = pduWithUnitId[2];
                throw new MyModbusSlaveException($"Modbus Error: {exceptionCode}", pduWithUnitId[1], exceptionCode);
            }

            if (pduWithUnitId[1] != expectedFunctionCode)
            {
                throw new IOException($"Mismatched Function Code. Expected: {expectedFunctionCode}, Received: {pduWithUnitId[1]}");
            }

            return pduWithUnitId.AsSpan(1).ToArray();
        }

        protected async Task WriteAsync(byte[] adu)
        {
            if (_stream == null) throw new InvalidOperationException("Stream is not available for this transport.");

            _stream.WriteTimeout = WriteTimeout;
            await _stream.WriteAsync(adu, 0, adu.Length);
        }

        protected void LogFrame(byte[] adu, string direction, byte slaveId)
        {
            if (_mainWindow == null) return;
            string dataFrame = BitConverter.ToString(adu).Replace("-", " ");
            string deviceNameForLog = this.LoggingDeviceName;

            if (string.IsNullOrEmpty(deviceNameForLog))
            {
                deviceNameForLog = $"Slave {slaveId}";
            }

            var logEntry = new ModbusFrameLog
            {
                Timestamp = DateTime.Now,
                DeviceName = deviceNameForLog,
                Direction = direction,
                DataFrame = dataFrame,
                ErrorDescription = string.Empty
            };

            _mainWindow.LogCommunicationEvent(logEntry);
        }

        public virtual void Dispose()
        {
            _transactionLock?.Dispose();
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}