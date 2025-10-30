using System;
using System.Buffers.Binary; // dla CRC
using System.IO.Ports;
using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    public class MyModbusSerialTransport : IMyModbusTransport
    {
        private readonly SerialPort _port;
        private readonly bool _isRtu;
        // TODO: Dodaj referencję do MainWindow, aby móc logować ramki

        public int ReadTimeout { get; set; } = 1000;
        public int WriteTimeout { get; set; } = 1000;

        public MyModbusSerialTransport(SerialPort port, bool isRtu/*, MainWindow mainWindow*/)
        {
            _port = port;
            _isRtu = isRtu;
            // _mainWindow = mainWindow;
            _port.ReadTimeout = ReadTimeout;
            _port.WriteTimeout = WriteTimeout;
        }

        public async Task<byte[]> SendRequestAsync(byte slaveId, byte functionCode, byte[] pduData)
        {
            // 1. Zbuduj PDU (SlaveID + FC + Data)
            byte[] pdu = new byte[2 + pduData.Length];
            pdu[0] = slaveId;
            pdu[1] = functionCode;
            Array.Copy(pduData, 0, pdu, 2, pduData.Length);

            byte[] adu;

            if (_isRtu)
            {
                // 2a. Zbuduj ramkę RTU (PDU + CRC)
                ushort crc = ModbusUtils.ComputeCrc(pdu);
                adu = new byte[pdu.Length + 2];
                Array.Copy(pdu, 0, adu, 0, pdu.Length);
                BinaryPrimitives.WriteUInt16LittleEndian(adu.AsSpan(pdu.Length, 2), crc); // RTU CRC jest Little Endian
            }
            else
            {
                // 2b. Zbuduj ramkę ASCII (Start + PDU_ASCII + LRC_ASCII + End)
                byte[] pduAscii = ModbusUtils.PduToAscii(pdu);
                byte lrc = ModbusUtils.ComputeLrc(pdu);
                byte[] lrcAscii = ModbusUtils.PduToAscii(new[] { lrc });

                adu = new byte[1 + pduAscii.Length + lrcAscii.Length + 2]; // : + PDU + LRC + CR + LF
                adu[0] = (byte)':';
                Array.Copy(pduAscii, 0, adu, 1, pduAscii.Length);
                Array.Copy(lrcAscii, 0, adu, 1 + pduAscii.Length, lrcAscii.Length);
                adu[adu.Length - 2] = (byte)'\r';
                adu[adu.Length - 1] = (byte)'\n';
            }

            // TODO: Logowanie ramki TX

            // 3. Wyczyść bufory i wyślij
            _port.DiscardInBuffer();
            _port.DiscardOutBuffer();
            await _port.BaseStream.WriteAsync(adu, 0, adu.Length);

            // 4. Odbierz odpowiedź (TO JEST NAJBARDZIEJ SKOMPLIKOWANA CZĘŚĆ)
            // Wymaga pętli odczytu z obsługą timeoutów między bajtami (dla RTU)
            // lub odczytu linii (dla ASCII).
            // Poniżej BARDZO UPROSZCZONY odczyt dla RTU, który zawiedzie w praktyce.
            // Musisz zaimplementować solidny mechanizm odczytu ramki.

            byte[] buffer = new byte[256];
            int bytesRead = await _port.BaseStream.ReadAsync(buffer, 0, buffer.Length);
            byte[] responseAdu = buffer.AsSpan(0, bytesRead).ToArray();

            // TODO: Logowanie ramki RX

            // 5. Zwaliduj ramkę (CRC/LRC) i wyodrębnij PDU
            byte[] responsePdu;
            if (_isRtu)
            {
                if (responseAdu.Length < 4) throw new Exception("Ramka RTU zbyt krótka.");
                ushort receivedCrc = BinaryPrimitives.ReadUInt16LittleEndian(responseAdu.AsSpan(responseAdu.Length - 2, 2));
                ushort computedCrc = ModbusUtils.ComputeCrc(responseAdu.AsSpan(0, responseAdu.Length - 2).ToArray());
                if (receivedCrc != computedCrc) throw new Exception("Błąd CRC.");

                responsePdu = responseAdu.AsSpan(0, responseAdu.Length - 2).ToArray();
            }
            else
            {
                // TODO: Walidacja ramki ASCII (znaczniki, LRC, konwersja)
                responsePdu = new byte[0]; // Placeholder
            }

            // 6. Walidacja PDU (tak jak w TCP)
            if (responsePdu[0] != slaveId) throw new Exception("Niezgodny Slave ID w odpowiedzi.");
            if (responsePdu[1] == (functionCode | 0x80))
            {
                byte exceptionCode = responsePdu[2];
                throw new MyModbusSlaveException($"Błąd Modbus: {exceptionCode}", responsePdu[1], exceptionCode);
            }
            if (responsePdu[1] != functionCode) throw new Exception("Niezgodny Function Code w odpowiedzi.");

            // 7. Zwróć dane PDU (bez SlaveID)
            byte[] pduDataResponse = new byte[responsePdu.Length - 1];
            Array.Copy(responsePdu, 1, pduDataResponse, 0, pduDataResponse.Length);

            return pduDataResponse;
        }

        public void Dispose()
        {
            _port?.Dispose();
        }
    }
}