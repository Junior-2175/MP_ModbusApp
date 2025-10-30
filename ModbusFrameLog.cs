using System;

namespace MP_ModbusApp
{
    /// <summary>
    /// Represents a single data model for an entry in the communication log.
    /// </summary>
    public class ModbusFrameLog
    {
        /// <summary>
        /// The exact time the frame was logged.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The name of the device (or "Slave X") that generated this log entry.
        /// Used for filtering in the log window.
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// The direction of the communication (e.g., "TX", "RX", or "Error").
        /// </summary>
        public string Direction { get; set; }

        /// <summary>
        /// The raw data frame, formatted as a hexadecimal string.
        /// </summary>
        public string DataFrame { get; set; }

        /// <summary>
        /// Contains the error message if the 'Direction' is "Error".
        /// </summary>
        public string ErrorDescription { get; set; } = string.Empty;
    }
}