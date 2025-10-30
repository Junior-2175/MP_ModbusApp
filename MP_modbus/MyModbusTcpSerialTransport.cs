using MP_ModbusApp.MP_modbus;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    /// <summary>
    /// Implementation of the transport for RTU/ASCII over TCP (without MBAP header).
    /// This transport wraps raw RTU/ASCII frames in a TCP stream.
    /// </summary>
    public sealed class MyModbusTcpSerialTransport : ModbusTransportBase
    {
        private readonly bool _isRtu;
        private readonly StreamReader _asciiReader;

        public MyModbusTcpSerialTransport(TcpClient client, bool isRtu, MainWindow mainWindow)
            : base(client, mainWindow)
        {
            _isRtu = isRtu;
            if (!_isRtu)
            {
                // We use a StreamReader for easy reading of ASCII lines
                _asciiReader = new StreamReader(_stream, Encoding.ASCII);
            }
        }

        /// <summary>
        /// Sends a Modbus request over the TCP stream, handling RTU/ASCII framing.
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
                await WriteAsync(adu);

                // 4a. Receive RTU frame
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
                await WriteAsync(adu);

                // 4b. Receive ASCII line
                string responseLine = await ReadAsciiLineAsync();
                LogFrame(Encoding.ASCII.GetBytes(responseLine), "RX", slaveId);

                // 5b. Validate ASCII frame
                if (!responseLine.StartsWith(":") || !responseLine.EndsWith("\r\n"))
                {
                    throw new IOException("Invalid ASCII frame format.");
                }

                // Remove markers : and \r\n
                string hexData = responseLine.Substring(1, responseLine.Length - 3);

                // Convert from hex string to bytes
                byte[] responseAduRaw = ModbusUtils.AsciiToPdu(Encoding.ASCII.GetBytes(hexData));

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
        /// Reads an RTU frame from the TCP stream, detecting the end of the frame using an inter-byte timeout.
        /// </summary>
        private async Task<byte[]> ReadRtuFrameAsync()
        {
            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    try
                    {
                        // Set the main timeout for reading the first byte
                        _stream.ReadTimeout = ReadTimeout;
                        byte[] buffer = new byte[1];
                        int bytesRead = await _stream.ReadAsync(buffer, 0, 1);

                        if (bytesRead == 0) break; // End of stream
                        ms.WriteByte(buffer[0]);

                        // After the first byte, set a short inter-byte timeout (T3.5)
                        // 50ms is a safe, though generous, value.
                        _stream.ReadTimeout = 50;
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException sockEx && sockEx.SocketErrorCode == SocketError.TimedOut)
                    {
                        // Inter-byte timeout = end of RTU frame
                        break;
                    }
                    catch (Exception)
                    {
                        throw; // Other error
                    }
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Reads an ASCII line (terminating with \n) from the TCP stream.
        /// </summary>
        private async Task<string> ReadAsciiLineAsync()
        {
            _stream.ReadTimeout = ReadTimeout;
            string line = await _asciiReader.ReadLineAsync();
            if (line == null)
            {
                throw new IOException("The connection was closed while reading the ASCII frame.");
            }
            // ReadLineAsync() removes \n, we add it back for consistency
            return line + "\n";
        }

        /// <summary>
        /// Disposes of the StreamReader and calls the base Dispose.
        /// </summary>
        public override void Dispose()
        {
            _asciiReader?.Dispose();
            base.Dispose();
        }
    }
}