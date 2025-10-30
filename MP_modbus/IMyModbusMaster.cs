using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    /// <summary>
    /// Defines the contract for a Modbus Master.
    /// </summary>
    public interface IMyModbusMaster : System.IDisposable
    {
        /// <summary>
        /// Gets the transport layer (e.g., TCP, Serial) used by this master.
        /// </summary>
        IMyModbusTransport Transport { get; }

        /// <summary>
        /// Reads one or more coils (Function Code 01).
        /// </summary>
        /// <param name="slaveId">The slave device address.</param>
        /// <param name="startAddress">The address of the first coil to read.</param>
        /// <param name="quantity">The number of coils to read.</param>
        /// <returns>An array of boolean values representing the coil states.</returns>
        Task<bool[]> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort quantity);

        /// <summary>
        /// Reads one or more discrete inputs (Function Code 02).
        /// </summary>
        /// <param name="slaveId">The slave device address.</param>
        /// <param name="startAddress">The address of the first input to read.</param>
        /// <param name="quantity">The number of inputs to read.</param>
        /// <returns>An array of boolean values representing the input states.</returns>
        Task<bool[]> ReadInputsAsync(byte slaveId, ushort startAddress, ushort quantity);

        /// <summary>
        /// Reads one or more holding registers (Function Code 03).
        /// </summary>
        /// <param name="slaveId">The slave device address.</param>
        /// <param name="startAddress">The address of the first register to read.</param>
        /// <param name="quantity">The number of registers to read.</param>
        /// <returns>An array of 16-bit unsigned integers representing the register values.</returns>
        Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort quantity);

        /// <summary>
        /// Reads one or more input registers (Function Code 04).
        /// </summary>
        /// <param name="slaveId">The slave device address.</param>
        /// <param name="startAddress">The address of the first register to read.</param>
        /// <param name="quantity">The number of registers to read.</param>
        /// <returns>An array of 16-bit unsigned integers representing the register values.</returns>
        Task<ushort[]> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort quantity);

        // Write methods (WriteCoilsAsync, WriteRegistersAsync, etc.) can be added here
    }
}