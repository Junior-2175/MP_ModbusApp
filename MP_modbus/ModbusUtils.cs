using System.Linq;

namespace MP_ModbusApp.MP_modbus
{
    public static class ModbusUtils
    {
        /// <summary>
        /// Oblicza sumę kontrolną CRC-16 dla Modbus RTU.
        /// </summary>
        public static ushort ComputeCrc(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte b in data)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    }
                    else
                    {
                        crc = (ushort)(crc >> 1);
                    }
                }
            }
            return crc;
        }

        /// <summary>
        /// Oblicza sumę kontrolną LRC dla Modbus ASCII.
        /// </summary>
        public static byte ComputeLrc(byte[] data)
        {
            byte lrc = 0;
            foreach (byte b in data)
            {
                lrc += b;
            }
            // Zwraca 2's complement
            return (byte)((-lrc) & 0xFF);
        }

        /// <summary>
        /// Konwertuje ramkę PDU na reprezentację ASCII (bez sumy LRC i znaczników).
        /// </summary>
        public static byte[] PduToAscii(byte[] pdu)
        {
            return System.Text.Encoding.ASCII.GetBytes(
                string.Concat(pdu.Select(b => b.ToString("X2")))
            );
        }

        /// <summary>
        /// Konwertuje ramkę ASCII na PDU (bez sumy LRC i znaczników).
        /// </summary>
        public static byte[] AsciiToPdu(byte[] asciiFrame)
        {
            byte[] pdu = new byte[asciiFrame.Length / 2];
            for (int i = 0; i < pdu.Length; i++)
            {
                string hexPair = System.Text.Encoding.ASCII.GetString(asciiFrame, i * 2, 2);
                pdu[i] = System.Convert.ToByte(hexPair, 16);
            }
            return pdu;
        }

        /// <summary>
        /// Tłumaczy kod błędu Modbus na samą nazwę (dla UI).
        /// </summary>
        public static string GetExceptionName(byte exceptionCode)
        {
            switch (exceptionCode)
            {
                case 1: return "Illegal Function";
                case 2: return "Illegal Data Address"; // Błąd z Twojego zrzutu ekranu
                case 3: return "Illegal Data Value";
                case 4: return "Slave Device Failure";
                case 5: return "Acknowledge";
                case 6: return "Slave Device Busy";
                case 7: return "Negative Acknowledge";
                case 8: return "Memory Parity Error";
                case 10: return "Gateway Path Unavailable";
                case 11: return "Gateway Target Device Failed to Respond";
                default: return $"Unknown Exception ({exceptionCode})";
            }
        }

        /// <summary>
        /// Tłumaczy kod błędu Modbus na pełny string (dla logów).
        /// </summary>
        public static string GetFullExceptionMessage(byte functionCode, byte exceptionCode)
        {
            string errorName = GetExceptionName(exceptionCode);
            // Kod funkcji w ramce błędu to już 0x80 + FC
            byte originalFunctionCode = (byte)(functionCode - 128);
            return $"Modbus Error (FC:{originalFunctionCode}, Code:{exceptionCode}) - {errorName}";
        }
    }
}