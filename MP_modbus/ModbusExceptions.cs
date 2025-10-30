using System;

namespace MP_ModbusApp.MP_modbus
{
    // Ten wyjątek zastąpi NModbus.SlaveException
    public class MyModbusSlaveException : Exception
    {
        public byte FunctionCode { get; }
        public byte SlaveExceptionCode { get; }

        public MyModbusSlaveException(string message, byte functionCode, byte slaveExceptionCode)
            : base(message)
        {
            FunctionCode = functionCode;
            SlaveExceptionCode = slaveExceptionCode;
        }
    }
}