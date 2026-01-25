using System;
using System.Linq;
using System.Buffers.Binary;
using System.Text;
using MP_ModbusApp;

namespace MP_ModbusApp.MP_modbus
{
    /// <summary>
    /// Provides static utility methods for Modbus RTU (CRC) and ASCII (LRC, conversions) protocols,
    /// as well as helper functions for converting complex data types to Modbus registers.
    /// </summary>
    public static class ModbusUtils
    {
        // --- Standard Modbus Functions (CRC, LRC, Conversions) ---

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
        /// Converts a PDU frame to its ASCII (hex) representation.
        /// </summary>
        public static byte[] PduToAscii(byte[] pdu)
        {
            return System.Text.Encoding.ASCII.GetBytes(
                string.Concat(pdu.Select(b => b.ToString("X2")))
            );
        }

        /// <summary>
        /// Converts an ASCII (hex) frame back to a PDU.
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
        /// Translates a Modbus exception code into its name.
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
        /// Translates a Modbus exception code into a full log message.
        /// </summary>
        public static string GetFullExceptionMessage(byte functionCode, byte exceptionCode)
        {
            string errorName = GetExceptionName(exceptionCode);
            byte originalFunctionCode = (byte)(functionCode - 128);
            return $"Modbus Error (FC:{originalFunctionCode}, Code:{exceptionCode}) - {errorName}";
        }

        // --- Multi-byte Value Conversion Functions (for Write operations) ---

        /// <summary>
        /// Gets the total byte count required for a given display format.
        /// </summary>
        public static int GetTotalBytesForFormat(ReadingsTab.DisplayFormat format)
        {
            string fmtStr = format.ToString();

            if (fmtStr.Contains("64"))
                return 8; // 4 registers * 2 bytes

            if (fmtStr.Contains("32"))
                return 4; // 2 registers * 2 bytes

            return 2; // 1 register * 2 bytes
        }

        /// <summary>
        /// Converts raw bytes of a numeric value to a ushort register array according to the specified format.
        /// </summary>
        /// <typeparam name="T">Value type (float, double, int, etc.).</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="format">Target Modbus display format.</param>
        /// <returns>An array of ushort registers in Modbus order.</returns>
        public static ushort[] ConvertValueToRegisters<T>(T value, ReadingsTab.DisplayFormat format) where T : struct
        {
            byte[] valueBytes;

            // System-dependent conversion (usually Little Endian)
            if (typeof(T) == typeof(float)) valueBytes = BitConverter.GetBytes((float)(object)value);
            else if (typeof(T) == typeof(double)) valueBytes = BitConverter.GetBytes((double)(object)value);
            else if (typeof(T) == typeof(int)) valueBytes = BitConverter.GetBytes((int)(object)value);
            else if (typeof(T) == typeof(uint)) valueBytes = BitConverter.GetBytes((uint)(object)value);
            else if (typeof(T) == typeof(long)) valueBytes = BitConverter.GetBytes((long)(object)value);
            else if (typeof(T) == typeof(ulong)) valueBytes = BitConverter.GetBytes((ulong)(object)value);
            else throw new ArgumentException("Unsupported generic type for conversion.");

            int totalBytes = valueBytes.Length;
            int numRegisters = totalBytes / 2;
            ushort[] registers = new ushort[numRegisters];

            string fmtStr = format.ToString();
            bool isLE_Format = fmtStr.Contains("_LE");
            bool isBS_Format = fmtStr.Contains("_BS");

            byte[] orderedBytes = new byte[totalBytes];
            int[] byteOrder;

            if (totalBytes == 4) // 32-bit values
            {
                if (isLE_Format)
                    byteOrder = isBS_Format ? new[] { 2, 3, 0, 1 } : new[] { 1, 0, 3, 2 };
                else // Big Endian
                    byteOrder = isBS_Format ? new[] { 0, 1, 2, 3 } : new[] { 3, 2, 1, 0 };
            }
            else if (totalBytes == 8) // 64-bit values
            {
                if (isLE_Format)
                    byteOrder = isBS_Format ? new[] { 6, 7, 4, 5, 2, 3, 0, 1 } : new[] { 1, 0, 3, 2, 5, 4, 7, 6 };
                else // Big Endian
                    byteOrder = isBS_Format ? new[] { 0, 1, 2, 3, 4, 5, 6, 7 } : new[] { 7, 6, 5, 4, 3, 2, 1, 0 };
            }
            else
            {
                throw new NotSupportedException($"Unsupported byte count: {totalBytes}");
            }

            // Map bytes from system order to Modbus order
            for (int i = 0; i < totalBytes; i++)
            {
                orderedBytes[i] = valueBytes[byteOrder[i]];
            }

            // Convert consecutive byte pairs to Big Endian ushort registers
            for (int i = 0; i < numRegisters; i++)
            {
                registers[i] = BinaryPrimitives.ReadUInt16BigEndian(orderedBytes.AsSpan(i * 2, 2));
            }

            return registers;
        }

        /// <summary>
        /// Converts an ASCII string to a ushort register array.
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

                if (isBS_Format)
                {
                    // Lo-Byte, Hi-Byte
                    registers[i] = (ushort)((byte2 << 8) | byte1);
                }
                else
                {
                    // Standard Modbus (Big Endian) within register: Hi-Byte, Lo-Byte
                    registers[i] = (ushort)((byte1 << 8) | byte2);
                }
            }

            return registers;
        }
    }
}