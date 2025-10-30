using System;

namespace MP_ModbusApp.MP_modbus
{
    /// <summary>
    /// Represents a Modbus slave exception (e.g., Illegal Function, Illegal Data Address).
    /// </summary>
    public class MyModbusSlaveException : Exception
    {
        /// <summary>
        /// Gets the function code that caused the exception (e.g., 0x83 for an error on FC 0x03).
        /// </summary>
        public byte FunctionCode { get; }

        /// <summary>
        /// Gets the Modbus exception code (e.g., 0x01, 0x02, 0x03).
        /// </summary>
        public byte SlaveExceptionCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyModbusSlaveException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="functionCode">The function code that caused the exception.</param>
        /// <param name="slaveExceptionCode">The Modbus exception code.</param>
        public MyModbusSlaveException(string message, byte functionCode, byte slaveExceptionCode)
            : base(message)
        {
            FunctionCode = functionCode;
            SlaveExceptionCode = slaveExceptionCode;
        }
    }
}