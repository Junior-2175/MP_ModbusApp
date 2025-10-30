using MP_ModbusApp.MP_modbus;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MP_ModbusApp.MP_modbus
{
    /// <summary>
    /// Bazowa klasa dla transportów TCP. Obsługuje logowanie,
    /// walidację PDU i niezawodny odczyt ze strumienia.
    /// </summary>
    public abstract class ModbusTransportBase : IMyModbusTransport
    {
        protected readonly TcpClient _client;
        protected readonly NetworkStream _stream;
        protected readonly MainWindow _mainWindow; // Do logowania
        private readonly object _streamLock = new object();

        public int ReadTimeout { get; set; } = 1000;
        public int WriteTimeout { get; set; } = 1000;

        protected ModbusTransportBase(TcpClient client, MainWindow mainWindow)
        {
            _client = client;
            _stream = client.GetStream();
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// Metoda specyficzna dla implementacji (MBAP lub RTU/ASCII).
        /// </summary>
        public abstract Task<byte[]> SendRequestAsync(byte slaveId, byte functionCode, byte[] pduData);

        /// <summary>
        /// Zapewnia odczytanie żądanej liczby bajtów ze strumienia.
        /// </summary>
        protected async Task ReadFullAsync(byte[] buffer, int count)
        {
            _stream.ReadTimeout = ReadTimeout;
            int bytesRead = 0;
            while (bytesRead < count)
            {
                int read = await _stream.ReadAsync(buffer, bytesRead, count - bytesRead);
                if (read == 0)
                {
                    // Strumień został zamknięty
                    throw new IOException("Połączenie zostało nieoczekiwanie zamknięte.");
                }
                bytesRead += read;
            }
        }

        /// <summary>
        /// Waliduje PDU (z UnitID) i rzuca wyjątki lub zwraca PDU (bez UnitID).
        /// </summary>
        protected byte[] ValidateAndExtractPdu(byte[] pduWithUnitId, byte expectedSlaveId, byte expectedFunctionCode)
        {
            if (pduWithUnitId.Length < 2)
            {
                throw new IOException("Odpowiedź jest zbyt krótka.");
            }

            // 1. Sprawdź Slave ID
            if (pduWithUnitId[0] != expectedSlaveId)
            {
                throw new IOException($"Otrzymano odpowiedź od złego Slave ID. Oczekiwano: {expectedSlaveId}, Otrzymano: {pduWithUnitId[0]}");
            }

            // 2. Sprawdź kod błędu (exception)
            if (pduWithUnitId[1] == (expectedFunctionCode | 0x80))
            {
                byte exceptionCode = pduWithUnitId[2];
                // Używamy tego samego mechanizmu co w ModbusDevice do generowania wiadomości
                throw new MyModbusSlaveException($"Błąd Modbus: {exceptionCode}", pduWithUnitId[1], exceptionCode);
            }

            // 3. Sprawdź kod funkcji
            if (pduWithUnitId[1] != expectedFunctionCode)
            {
                throw new IOException($"Niezgodny Function Code. Oczekiwano: {expectedFunctionCode}, Otrzymano: {pduWithUnitId[1]}");
            }

            // 4. Zwróć PDU bez SlaveID (czyli [FC] + [Data])
            // MyModbusMaster oczekuje PDU zaczynającego się od kodu funkcji.
            return pduWithUnitId.AsSpan(1).ToArray();
        }

        /// <summary>
        /// Wysyła dane i zarządza blokadą strumienia.
        /// </summary>
        protected async Task WriteAsync(byte[] adu)
        {
            // Używamy blokady, aby zapobiec wysyłaniu wielu żądań na raz,
            // jeśli aplikacja stanie się wielowątkowa.
            lock (_streamLock)
            {
                _stream.WriteTimeout = WriteTimeout;
                _stream.Write(adu, 0, adu.Length);
            }
            await Task.CompletedTask; // Uproszczenie; Write nie jest async w SerialPort. Używamy synchronicznego zapisu w blokadzie.
                                      // Dla NetworkStream można użyć WriteAsync.
                                      // Dla spójności z SerialPort (który nie ma dobrego WriteAsync), użyjemy synchronicznego zapisu.
                                      // _stream.Write(adu, 0, adu.Length); // Zamiast WriteAsync
        }


        public virtual void Dispose()
        {
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}