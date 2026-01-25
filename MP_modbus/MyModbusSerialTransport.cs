using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets; // Required for IOException (base compatibility)
using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    /// <summary>
    /// Implements the Modbus transport for Serial Port (COM) communication (RTU and ASCII).
    /// Inherits from ModbusTransportBase but uses SerialPort instead of TcpClient.
    /// </summary>
    public class MyModbusSerialTransport : ModbusTransportBase
    {
        private readonly SerialPort _port;
        private readonly bool _isRtu;

        public MyModbusSerialTransport(SerialPort port, bool isRtu, MainWindow mainWindow)
            // Pass null for the TcpClient, as this transport uses a SerialPort
            : base(null, mainWindow)
        {
            _port = port;
            _isRtu = isRtu;
            _port.ReadTimeout = ReadTimeout;
            _port.WriteTimeout = WriteTimeout;
        }

        /// <summary>
        /// Sends a Modbus request over the serial port, handling RTU/ASCII framing.
        /// </summary>
        public override async Task<byte[]> SendRequestAsync(byte slaveId, byte functionCode, byte[] pduData)
        {
            // 1. Build PDU (SlaveID + FC + Data)
            byte[] pdu = new byte[2 + pduData.Length];
            pdu[0] = slaveId;
            pdu[1] = functionCode;
            Array.Copy(pduData, 0, pdu, 2, pduData.Length);

            byte[] adu;
            byte[] pduResponse;

            if (_isRtu)
            {
                // ---- RTU Logic ----

                // 2a. Build RTU frame (PDU + CRC)
                ushort crc = ModbusUtils.ComputeCrc(pdu);
                adu = new byte[pdu.Length + 2];
                Array.Copy(pdu, 0, adu, 0, pdu.Length);
                BinaryPrimitives.WriteUInt16LittleEndian(adu.AsSpan(pdu.Length, 2), crc); // RTU CRC is Little Endian

                // 3a. Send
                LogFrame(adu, "TX", slaveId);

                _port.DiscardInBuffer();
                _port.DiscardOutBuffer();
                // Use WriteAsync on the SerialPort's BaseStream
                await _port.BaseStream.WriteAsync(adu, 0, adu.Length);

                // 4a. Receive RTU frame
                // Use the robust RTU read method
                byte[] responseAdu = await ReadRtuFrameAsync();
                LogFrame(responseAdu, "RX", slaveId);

                // 5a. Validate CRC
                if (responseAdu.Length < 4) throw new IOException("RTU frame is too short.");
                ushort receivedCrc = BinaryPrimitives.ReadUInt16LittleEndian(responseAdu.AsSpan(responseAdu.Length - 2, 2));
                ushort computedCrc = ModbusUtils.ComputeCrc(responseAdu.AsSpan(0, responseAdu.Length - 2).ToArray());

                if (receivedCrc != computedCrc) throw new IOException("CRC checksum error.");

                pduResponse = responseAdu.AsSpan(0, responseAdu.Length - 2).ToArray();
            }
            else
            {
                // ---- ASCII Logic ----

                // 2b. Build ASCII frame (Start + PDU_ASCII + LRC_ASCII + End)
                byte[] pduAscii = ModbusUtils.PduToAscii(pdu);
                byte lrc = ModbusUtils.ComputeLrc(pdu);
                byte[] lrcAscii = ModbusUtils.PduToAscii(new[] { lrc });

                // : + PDU(hex) + LRC(hex) + CR + LF
                adu = new byte[1 + pduAscii.Length + lrcAscii.Length + 2];
                adu[0] = (byte)':';
                Array.Copy(pduAscii, 0, adu, 1, pduAscii.Length);
                Array.Copy(lrcAscii, 0, adu, 1 + pduAscii.Length, lrcAscii.Length);
                adu[adu.Length - 2] = (byte)'\r';
                adu[adu.Length - 1] = (byte)'\n';

                // 3b. Send
                LogFrame(adu, "TX", slaveId);

                _port.DiscardInBuffer();
                _port.DiscardOutBuffer();
                await _port.BaseStream.WriteAsync(adu, 0, adu.Length);

                // 4b. Receive ASCII line
                string responseLine = await ReadAsciiLineAsync();
                LogFrame(System.Text.Encoding.ASCII.GetBytes(responseLine), "RX", slaveId);

                // 5b. Validate ASCII frame
                if (!responseLine.StartsWith(":") || !responseLine.EndsWith("\r\n"))
                {
                    throw new IOException("Invalid ASCII frame format.");
                }

                // Remove markers : and \r\n
                string hexData = responseLine.Substring(1, responseLine.Length - 3);

                // Convert from hex string to bytes
                byte[] responseAduRaw = ModbusUtils.AsciiToPdu(System.Text.Encoding.ASCII.GetBytes(hexData));

                // Split into PDU and LRC
                byte[] pduPlusLrc = responseAduRaw;
                if (pduPlusLrc.Length < 2) throw new IOException("ASCII frame is too short.");

                pduResponse = pduPlusLrc.AsSpan(0, pduPlusLrc.Length - 1).ToArray();
                byte receivedLrc = pduPlusLrc[pduPlusLrc.Length - 1];
                byte computedLrc = ModbusUtils.ComputeLrc(pduResponse);

                if (receivedLrc != computedLrc) throw new IOException("LRC checksum error.");
            }

            // 6. Common PDU validation (with UnitID) and return [FC] + [Data]
            return ValidateAndExtractPdu(pduResponse, slaveId, functionCode);
        }

        /// <summary>
        /// Reads an RTU frame, detecting the end of the frame using an inter-byte timeout.
        /// RTU frames are delimited by silence (a T3.5 char timeout).
        /// </summary>
        private async Task<byte[]> ReadRtuFrameAsync()
        {
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[1]; // Alokacja bufora poza pętlą dla wydajności
                bool isFirstByte = true;

                while (true)
                {
                    try
                    {
                        // Ustawienie odpowiedniego czasu oczekiwania
                        int currentTimeout = isFirstByte ? ReadTimeout : 50;

                        // Tworzymy zadanie odczytu
                        var readTask = _port.BaseStream.ReadAsync(buffer, 0, 1);

                        // Tworzymy zadanie timeoutu (ponieważ ReadAsync w SerialPort często ignoruje ReadTimeout)
                        var timeoutTask = Task.Delay(currentTimeout);

                        // Czekamy na to, co wydarzy się pierwsze: odczyt czy timeout
                        var completedTask = await Task.WhenAny(readTask, timeoutTask);

                        if (completedTask == timeoutTask)
                        {
                            // Wystąpił timeout
                            if (isFirstByte)
                            {
                                // Timeout na pierwszym bajcie to błąd komunikacji (brak odpowiedzi)
                                throw new TimeoutException("Timeout waiting for response.");
                            }
                            else
                            {
                                // Timeout na kolejnych bajtach oznacza KONIEC RAMKI (RTU Silence)
                                break;
                            }
                        }

                        // Jeśli odczyt się powiódł, pobieramy wynik
                        int bytesRead = await readTask;

                        if (bytesRead == 0) break; // Zamknięcie strumienia

                        ms.WriteByte(buffer[0]);
                        isFirstByte = false;
                    }
                    catch (Exception)
                    {
                        throw; // Przekaż inne błędy wyżej
                    }
                }
                return ms.ToArray();
            }
        }
        /// <summary>
        /// Reads an ASCII line (terminating with \n).
        /// </summary>
        private async Task<string> ReadAsciiLineAsync()
        {
            _port.ReadTimeout = ReadTimeout;
            byte[] buffer = new byte[1];
            var lineBuilder = new System.Text.StringBuilder();
            while (true)
            {
                try
                {
                    int bytesRead = await _port.BaseStream.ReadAsync(buffer, 0, 1);
                    if (bytesRead == 0) throw new IOException("Stream closed.");

                    char c = (char)buffer[0];
                    lineBuilder.Append(c);

                    if (c == '\n') // End of line
                    {
                        return lineBuilder.ToString();
                    }
                }
                catch (TimeoutException)
                {
                    // Main read timeout
                    throw new IOException("Timeout while reading ASCII frame.");
                }
            }
        }

        /// <summary>
        /// Disposes of the SerialPort and calls the base Dispose.
        /// </summary>
        public override void Dispose()
        {
            _port?.Dispose();
            // Call the base class's Dispose() method
            base.Dispose();
        }
    }
}