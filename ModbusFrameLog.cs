using System;

namespace MP_ModbusApp
{
    /// <summary>
    /// Reprezentuje pojedynczy wpis w logu komunikacyjnym Modbus.
    /// </summary>
    public class ModbusFrameLog
    {
        public DateTime Timestamp { get; set; }
        // --- NOWY KOD ---
        public string DeviceName { get; set; } = string.Empty; // Nazwa urządzenia generującego log
        // --- KONIEC NOWEGO KODU ---
        public ushort TransactionID { get; set; } // Zmieniono na ushort zgodnie z poprzednim kodem, chociaż nie jest używane
        public string Direction { get; set; } // "TX" lub "RX" lub "Error"
        public string DataFrame { get; set; }
        public string ErrorDescription { get; set; } = string.Empty;
    }
}