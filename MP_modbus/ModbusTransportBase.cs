using MP_ModbusApp.MP_modbus;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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
        protected readonly MainWindow _mainWindow; // For logging
        private readonly object _streamLock = new object();

        public int ReadTimeout { get; set; } = 1000;
        public int WriteTimeout { get; set; } = 1000;

        protected ModbusTransportBase(TcpClient client, MainWindow mainWindow)
        {
            _client = client;
            // Get the stream only if the client is not null
            // (Serial transport will pass null and use its own stream)
            _stream = client?.GetStream();
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// Sends a request using the specific transport implementation (e.g., MBAP or RTU/ASCII framing).
        /// </summary>
        public abstract Task<byte[]> SendRequestAsync(byte slaveId, byte functionCode, byte[] pduData);

        /// <summary>
        /// Ensures that the requested number of bytes is read from the stream.
        /// This is primarily used by TCP transports.
        /// </summary>
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
                    // Stream was closed
                    throw new IOException("The connection was unexpectedly closed.");
                }
                bytesRead += read;
            }
        }

        /// <summary>
        /// Validates the received PDU (which includes the UnitID) against expectations.
        /// Throws exceptions for mismatches or slave errors.
        /// </summary>
        /// <returns>The PDU payload (Function Code + Data) if validation succeeds.</returns>
        protected byte[] ValidateAndExtractPdu(byte[] pduWithUnitId, byte expectedSlaveId, byte expectedFunctionCode)
        {
            if (pduWithUnitId.Length < 2)
            {
                throw new IOException("Response is too short.");
            }

            // 1. Check Slave ID
            if (pduWithUnitId[0] != expectedSlaveId)
            {
                throw new IOException($"Received response from wrong Slave ID. Expected: {expectedSlaveId}, Received: {pduWithUnitId[0]}");
            }

            // 2. Check for Modbus exception (e.g., 0x83 for FC 0x03)
            if (pduWithUnitId[1] == (expectedFunctionCode | 0x80))
            {
                byte exceptionCode = pduWithUnitId[2];
                // Use the same mechanism as ModbusDevice to generate the message
                throw new MyModbusSlaveException($"Modbus Error: {exceptionCode}", pduWithUnitId[1], exceptionCode);
            }

            // 3. Check Function Code
            if (pduWithUnitId[1] != expectedFunctionCode)
            {
                throw new IOException($"Mismatched Function Code. Expected: {expectedFunctionCode}, Received: {pduWithUnitId[1]}");
            }

            // 4. Return the PDU without the SlaveID (i.e., [FC] + [Data])
            // MyModbusMaster expects a PDU starting with the function code.
            return pduWithUnitId.AsSpan(1).ToArray();
        }

        /// <summary>
        /// Writes data to the stream using a lock.
        /// This method is used by TCP-based transports.
        /// </summary>
        protected async Task WriteAsync(byte[] adu)
        {
            if (_stream == null) throw new InvalidOperationException("Stream is not available for this transport.");

            // Use a lock to prevent multiple requests from interleaving
            // if the application becomes multi-threaded.
            lock (_streamLock)
            {
                _stream.WriteTimeout = WriteTimeout;
                _stream.Write(adu, 0, adu.Length);
            }
            await Task.CompletedTask; // Keep async signature, even though write is sync
        }


        /// <summary>
        /// Logs the ADU frame to the CommunicationLogWindow.
        /// </summary>
        protected void LogFrame(byte[] adu, string direction, byte slaveId)
        {
            // Check if _mainWindow (and the log window) exist
            if (_mainWindow == null) return;

            // Convert byte[] to a readable HEX string
            string dataFrame = BitConverter.ToString(adu).Replace("-", " ");

            var logEntry = new ModbusFrameLog
            {
                Timestamp = DateTime.Now,
                // At this level, we don't know the device name, but we know the SlaveID.
                // We use this so that filtering in the log window works.
                // LogFrame in ModbusDevice.cs (for errors) will use the full name.
                DeviceName = $"Slave {slaveId}",
                Direction = direction,
                DataFrame = dataFrame,
                ErrorDescription = string.Empty
            };

            // Call the method in MainWindow, which passes the log to the log window
            _mainWindow.LogCommunicationEvent(logEntry);
        }

        /// <summary>
        /// Disposes of the underlying transport resources (Stream and Client).
        /// </summary>
        public virtual void Dispose()
        {
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}