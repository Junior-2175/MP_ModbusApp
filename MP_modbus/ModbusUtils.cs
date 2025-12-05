using System;
using System.Linq;
using System.Buffers.Binary;
using System.Text;
using MP_ModbusApp; // Dodano, aby uzyskać dostęp do ReadingsTab.DisplayFormat

namespace MP_ModbusApp.MP_modbus
{
    /// <summary>
    /// Provides static utility methods for Modbus RTU (CRC) and ASCII (LRC, conversions) protocols,
    /// as well as helper functions for converting complex data types (like floats/doubles) to Modbus registers.
    /// </summary>
    public static class ModbusUtils
    {
        // --- Standardowe Funkcje Modbus (CRC, LRC, Konwersje) ---

        /// <summary>
        /// Computes the CRC-16 checksum for Modbus RTU.
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
        /// Computes the LRC checksum for Modbus ASCII.
        /// </summary>
        public static byte ComputeLrc(byte[] data)
        {
            byte lrc = 0;
            foreach (byte b in data)
            {
                lrc += b;
            }
            // Returns the 2's complement
            return (byte)((-lrc) & 0xFF);
        }

        /// <summary>
        /// Converts a PDU frame to its ASCII (hex) representation (without LRC and markers).
        /// </summary>
        public static byte[] PduToAscii(byte[] pdu)
        {
            return System.Text.Encoding.ASCII.GetBytes(
                string.Concat(pdu.Select(b => b.ToString("X2")))
            );
        }

        /// <summary>
        /// Converts an ASCII (hex) frame back to a PDU (without LRC and markers).
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
        /// Translates a Modbus exception code into its name (for UI display).
        /// </summary>
        public static string GetExceptionName(byte exceptionCode)
        {
            switch (exceptionCode)
            {
                case 1: return "Illegal Function";
                case 2: return "Illegal Data Address";
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
        /// Translates a Modbus exception code into a full string (for logging).
        /// </summary>
        public static string GetFullExceptionMessage(byte functionCode, byte exceptionCode)
        {
            string errorName = GetExceptionName(exceptionCode);
            byte originalFunctionCode = (byte)(functionCode - 128);
            return $"Modbus Error (FC:{originalFunctionCode}, Code:{exceptionCode}) - {errorName}";
        }

        // --- Funkcje Konwersji Wartości Wielobajtowych do Rejestrów (dla ZAPISU) ---

        /// <summary>
        /// Zwraca całkowitą liczbę bajtów dla danego formatu.
        /// </summary>
        public static int GetTotalBytesForFormat(ReadingsTab.DisplayFormat format)
        {
            string fmtStr = format.ToString();

            if (fmtStr.Contains("64"))
                return 8; // 4 rejestry * 2 bajty

            if (fmtStr.Contains("32"))
                return 4; // 2 rejestry * 2 bajty

            return 2; // 1 rejestr * 2 bajty
        }

        /// <summary>
        /// Konwertuje surowe bajty wartości numerycznej (np. float/double) na tablicę ushort[]
        /// zgodnie z zadanym formatem.
        /// </summary>
        /// <typeparam name="T">Typ wartości (float, double, int, long, uint, ulong).</typeparam>
        /// <param name="value">Wartość do konwersji.</param>
        /// <param name="format">Format docelowy (np. Float32_BE, Signed64_LE_BS).</param>
        /// <returns>Tablica rejestrów ushort w kolejności Modbus (Rejestr 1, Rejestr 2, ...).</returns>
        public static ushort[] ConvertValueToRegisters<T>(T value, ReadingsTab.DisplayFormat format) where T : struct
        {
            byte[] valueBytes;

            // Konwersja generyczna wartości na bajty w kolejności systemowej (zwykle Little Endian)
            if (typeof(T) == typeof(float)) valueBytes = BitConverter.GetBytes((float)(object)value);
            else if (typeof(T) == typeof(double)) valueBytes = BitConverter.GetBytes((double)(object)value);
            else if (typeof(T) == typeof(int)) valueBytes = BitConverter.GetBytes((int)(object)value);
            else if (typeof(T) == typeof(uint)) valueBytes = BitConverter.GetBytes((uint)(object)value);
            else if (typeof(T) == typeof(long)) valueBytes = BitConverter.GetBytes((long)(object)value);
            else if (typeof(T) == typeof(ulong)) valueBytes = BitConverter.GetBytes((ulong)(object)value);
            else throw new ArgumentException("Nieobsługiwany typ generyczny dla konwersji.");

            int totalBytes = valueBytes.Length;
            int numRegisters = totalBytes / 2;
            ushort[] registers = new ushort[numRegisters];

            string fmtStr = format.ToString();
            bool isLE_Format = fmtStr.Contains("_LE");
            bool isBS_Format = fmtStr.Contains("_BS");

            byte[] orderedBytes = new byte[totalBytes];
            int[] byteOrder;

            if (totalBytes == 4) // 32-bit (float, int, uint)
            {
                // B0 B1 B2 B3 (kolejność PC LE)
                if (isLE_Format)
                    // LE Normal (1-0-3-2): R1(B1 B0), R2(B3 B2)
                    byteOrder = isBS_Format ? new[] { 2, 3, 0, 1 } : new[] { 1, 0, 3, 2 };
                else // Big Endian
                    // BE Normal (3-2-1-0): R1(B3 B2), R2(B1 B0)
                    byteOrder = isBS_Format ? new[] { 0, 1, 2, 3 } : new[] { 3, 2, 1, 0 };
            }
            else if (totalBytes == 8) // 64-bit (double, long, ulong)
            {
                // B0 B1 B2 B3 B4 B5 B6 B7 (kolejność PC LE)
                if (isLE_Format)
                    // LE Normal: R1(B1 B0), R2(B3 B2), R3(B5 B4), R4(B7 B6)
                    byteOrder = isBS_Format ? new[] { 6, 7, 4, 5, 2, 3, 0, 1 } : new[] { 1, 0, 3, 2, 5, 4, 7, 6 };
                else // Big Endian
                    // BE Normal: R1(B7 B6), R2(B5 B4), R3(B3 B2), R4(B1 B0)
                    byteOrder = isBS_Format ? new[] { 0, 1, 2, 3, 4, 5, 6, 7 } : new[] { 7, 6, 5, 4, 3, 2, 1, 0 };
            }
            else
            {
                throw new NotSupportedException($"Nieobsługiwana liczba bajtów: {totalBytes}");
            }

            // Mapowanie bajtów z kolejności systemowej na kolejność Modbus
            for (int i = 0; i < totalBytes; i++)
            {
                orderedBytes[i] = valueBytes[byteOrder[i]];
            }

            // Konwersja kolejnych par bajtów na rejestry ushort (Big Endian)
            for (int i = 0; i < numRegisters; i++)
            {
                registers[i] = BinaryPrimitives.ReadUInt16BigEndian(orderedBytes.AsSpan(i * 2, 2));
            }

            return registers;
        }

        /// <summary>
        /// Konwertuje ciąg znaków ASCII na tablicę rejestrów ushort[].
        /// </summary>
        public static ushort[] ConvertAsciiToRegisters(string value, ReadingsTab.DisplayFormat format)
        {
            string fmtStr = format.ToString();
            int bytesNeeded = GetTotalBytesForFormat(format);

            string cleanValue = value.TrimEnd('\0');
            while (cleanValue.Length < bytesNeeded)
            {
                cleanValue += '\0';
            }
            if (cleanValue.Length > bytesNeeded)
            {
                cleanValue = cleanValue.Substring(0, bytesNeeded);
            }

            byte[] asciiBytes = Encoding.ASCII.GetBytes(cleanValue);
            int numRegisters = bytesNeeded / 2;
            ushort[] registers = new ushort[numRegisters];

            bool isBS_Format = fmtStr.Contains("_BS");

            for (int i = 0; i < numRegisters; i++)
            {
                byte byte1 = asciiBytes[i * 2];
                byte byte2 = asciiBytes[i * 2 + 1];

                // Wymiana kolejności bajtów wewnątrz rejestru (byte swap)
                if (isBS_Format)
                {
                    // Lo-Byte, Hi-Byte
                    registers[i] = (ushort)((byte2 << 8) | byte1);
                }
                else
                {
                    // Standardowy Modbus (Big Endian) wewnątrz rejestru: Hi-Byte, Lo-Byte
                    registers[i] = (ushort)((byte1 << 8) | byte2);
                }
            }

            return registers;
        }
    }
}