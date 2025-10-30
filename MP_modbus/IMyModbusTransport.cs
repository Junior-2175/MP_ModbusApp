using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    /// <summary>
    /// Defines the contract for a Modbus transport layer (e.g., TCP, Serial).
    /// </summary>
    public interface IMyModbusTransport : System.IDisposable
    {
        /// <summary>
        /// Gets or sets the read timeout in milliseconds.
        /// </summary>
        int ReadTimeout { get; set; }

        /// <summary>
        /// Gets or sets the write timeout in milliseconds.
        /// </summary>
        int WriteTimeout { get; set; }

        /// <summary>
        /// Asynchronously sends a Modbus request (PDU) to a specific slave.
        /// The transport layer is responsible for building the full ADU (e.g., adding MBAP header or CRC/LRC),
        /// sending the request, parsing the response, and handling transport-specific errors.
        /// </summary>
        /// <param name="slaveId">The slave device address.</param>
        /// <param name="functionCode">The Modbus function code (e.g., 0x03).</param>
        /// <param name="pduData">The PDU data (payload) specific to the function code, *excluding* slave ID and function code.</param>
        /// <returns>A task that represents the asynchronous operation. The result is the PDU part of the response (starting with the function code) or an exception.</returns>
        Task<byte[]> SendRequestAsync(byte slaveId, byte functionCode, byte[] pduData);
    }
}