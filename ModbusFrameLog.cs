using System;

namespace MP_ModbusApp
{
    /// <summary>
    /// Reprezentuje pojedynczy wpis w logu komunikacyjnym Modbus.
    /// </summary>
    public class ModbusFrameLog
    {
        public DateTime Timestamp { get; set; }
        public ushort TransactionID { get; set; }
        public string Direction { get; set; } // "TX" lub "RX"
        public string DataFrame { get; set; }
        public string ErrorDescription { get; set; } = string.Empty;
    }
}