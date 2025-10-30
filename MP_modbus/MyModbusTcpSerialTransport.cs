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
    /// Implementacja transportu dla RTU/ASCII over TCP (bez nagłówka MBAP).
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
                // Używamy StreamReader do łatwego odczytu linii ASCII
                _asciiReader = new StreamReader(_stream, Encoding.ASCII);
            }
        }

        public override async Task<byte[]> SendRequestAsync(byte slaveId, byte functionCode, byte[] pduData)
        {
            // 1. Zbuduj PDU (SlaveID + FC + Data)
            byte[] pdu = new byte[2 + pduData.Length];
            pdu[0] = slaveId;
            pdu[1] = functionCode;
            Array.Copy(pduData, 0, pdu, 2, pduData.Length);

            byte[] adu;
            byte[] pduResponse;

            if (_isRtu)
            {
                // ---- Logika RTU ----

                // 2a. Zbuduj ramkę RTU (PDU + CRC)
                ushort crc = ModbusUtils.ComputeCrc(pdu);
                adu = new byte[pdu.Length + 2];
                Array.Copy(pdu, 0, adu, 0, pdu.Length);
                BinaryPrimitives.WriteUInt16LittleEndian(adu.AsSpan(pdu.Length, 2), crc); // RTU CRC jest Little Endian

                // 3a. Wyślij
                LogFrame(adu, "TX", slaveId);
                await WriteAsync(adu);

                // 4a. Odbierz ramkę RTU
                byte[] responseAdu = await ReadRtuFrameAsync();
                LogFrame(responseAdu, "RX", slaveId);

                // 5a. Waliduj CRC
                if (responseAdu.Length < 4) throw new IOException("Ramka RTU zbyt krótka.");
                ushort receivedCrc = BinaryPrimitives.ReadUInt16LittleEndian(responseAdu.AsSpan(responseAdu.Length - 2, 2));
                ushort computedCrc = ModbusUtils.ComputeCrc(responseAdu.AsSpan(0, responseAdu.Length - 2).ToArray());

                if (receivedCrc != computedCrc) throw new IOException("Błąd sumy kontrolnej CRC.");

                pduResponse = responseAdu.AsSpan(0, responseAdu.Length - 2).ToArray();
            }
            else
            {
                // ---- Logika ASCII ----

                // 2b. Zbuduj ramkę ASCII (Start + PDU_ASCII + LRC_ASCII + End)
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

                // 3b. Wyślij
                LogFrame(adu, "TX", slaveId);
                await WriteAsync(adu);

                // 4b. Odbierz linię ASCII
                string responseLine = await ReadAsciiLineAsync();
                LogFrame(Encoding.ASCII.GetBytes(responseLine), "RX", slaveId);

                // 5b. Waliduj ramkę ASCII
                if (!responseLine.StartsWith(":") || !responseLine.EndsWith("\r\n"))
                {
                    throw new IOException("Nieprawidłowy format ramki ASCII.");
                }

                // Usuń znaczniki : i \r\n
                string hexData = responseLine.Substring(1, responseLine.Length - 3);

                // Konwertuj z hex na bajty
                byte[] responseAduRaw = ModbusUtils.AsciiToPdu(Encoding.ASCII.GetBytes(hexData));

                // Podziel na PDU i LRC
                byte[] pduPlusLrc = responseAduRaw;
                if (pduPlusLrc.Length < 2) throw new IOException("Ramka ASCII zbyt krótka.");

                pduResponse = pduPlusLrc.AsSpan(0, pduPlusLrc.Length - 1).ToArray();
                byte receivedLrc = pduPlusLrc[pduPlusLrc.Length - 1];
                byte computedLrc = ModbusUtils.ComputeLrc(pduResponse);

                if (receivedLrc != computedLrc) throw new IOException("Błąd sumy kontrolnej LRC.");
            }

            // 6. Wspólna walidacja PDU (z UnitID) i zwrot [FC] + [Data]
            return ValidateAndExtractPdu(pduResponse, slaveId, functionCode);
        }

        /// <summary>
        /// Odczytuje ramkę RTU, wykrywając koniec za pomocą timeoutu.
        /// </summary>
        private async Task<byte[]> ReadRtuFrameAsync()
        {
            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    try
                    {
                        // Ustawiamy główny timeout na odczyt pierwszego bajtu
                        _stream.ReadTimeout = ReadTimeout;
                        byte[] buffer = new byte[1];
                        int bytesRead = await _stream.ReadAsync(buffer, 0, 1);

                        if (bytesRead == 0) break; // Koniec strumienia
                        ms.WriteByte(buffer[0]);

                        // Po pierwszym bajcie, ustawiamy krótki timeout międzybajtowy (T3.5)
                        // 50ms to bezpieczna, choć hojna wartość.
                        _stream.ReadTimeout = 50;
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException sockEx && sockEx.SocketErrorCode == SocketError.TimedOut)
                    {
                        // Timeout między bajtami = koniec ramki RTU
                        break;
                    }
                    catch (Exception)
                    {
                        throw; // Inny błąd
                    }
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Odczytuje linię ASCII (kończącą się na \n).
        /// </summary>
        private async Task<string> ReadAsciiLineAsync()
        {
            _stream.ReadTimeout = ReadTimeout;
            string line = await _asciiReader.ReadLineAsync();
            if (line == null)
            {
                throw new IOException("Połączenie zostało zamknięte podczas odczytu ramki ASCII.");
            }
            return line + "\n"; // ReadLineAsync() usuwa \n, my go dodajemy dla spójności
        }

        public override void Dispose()
        {
            _asciiReader?.Dispose();
            base.Dispose();
        }
    }
}