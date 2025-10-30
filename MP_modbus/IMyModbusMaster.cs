using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    // Ten interfejs zastąpi NModbus.IModbusMaster
    public interface IMyModbusMaster : System.IDisposable
    {
        IMyModbusTransport Transport { get; }

        Task<bool[]> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort quantity);
        Task<bool[]> ReadInputsAsync(byte slaveId, ushort startAddress, ushort quantity);
        Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort quantity);
        Task<ushort[]> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort quantity);

        // Tutaj można dodać metody zapisu (WriteCoilsAsync, WriteRegistersAsync itd.)
    }
}