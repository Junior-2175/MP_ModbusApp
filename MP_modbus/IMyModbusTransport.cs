using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    // Ten interfejs zastąpi NModbus.Transport.IModbusTransport
    public interface IMyModbusTransport : System.IDisposable
    {
        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }

        // Ta metoda będzie budować pełną ramkę ADU, wysyłać ją i parsować odpowiedź,
        // zwracając tylko PDU z danymi lub rzucając wyjątek.
        Task<byte[]> SendRequestAsync(byte slaveId, byte functionCode, byte[] pduData);
    }
}