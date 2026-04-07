using MP_ModbusApp.MP_modbus;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP_ModbusApp
{
    public partial class AddressScan : Form
    {
        private readonly IMyModbusMaster _modbusMaster;
        private CancellationTokenSource _cts;
        private bool _isUpdatingValues = false;

        // Konfiguracja
        private const int ScanDelayMs = 50;
        private const int MaxRetries = 1;

        public event EventHandler<bool> ScanningStateChanged;

        public AddressScan(IMyModbusMaster modbusMaster)
        {
            InitializeComponent();
            _modbusMaster = modbusMaster;

            comboBox1.SelectedIndex = 2; // Domyślnie Holding Registers
            SetupEvents();
            UpdateMaxQuantity();
        }

        public AddressScan()
        {
            InitializeComponent();
        }

        private void SetupEvents()
        {
            startRegister.ValueChanged += StartRegister_ValueChanged;
            startRegisterHex.ValueChanged += StartRegisterHex_ValueChanged;
            startRegister.ValueChanged += (s, e) => UpdateMaxQuantity();

            startToolStripMenuItem.Click += StartToolStripMenuItem_Click;
            stopToolStripMenuItem1.Click += StopToolStripMenuItem1_Click;
            exportToolStripMenuItem.Click += ExportToolStripMenuItem_Click;

            this.FormClosing += AddressScan_FormClosing;
        }

        private void StartRegister_ValueChanged(object sender, EventArgs e)
        {
            startRegisterHex.ValueChanged -= StartRegisterHex_ValueChanged;
            startRegisterHex.Value = startRegister.Value;
            startRegisterHex.ValueChanged += StartRegisterHex_ValueChanged;
        }

        private void StartRegisterHex_ValueChanged(object sender, EventArgs e)
        {
            startRegister.ValueChanged -= StartRegister_ValueChanged;
            startRegister.Value = startRegisterHex.Value;
            startRegister.ValueChanged += StartRegister_ValueChanged;
        }

        private void UpdateMaxQuantity()
        {
            decimal maxQty = 65536 - startRegister.Value;
            if (maxQty < 1) maxQty = 1;
            if (maxQty > 65535) maxQty = 65535;

            numOfRegisters.Maximum = maxQty;
        }

        private async void StartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_modbusMaster == null)
            {
                MessageBox.Show("Modbus Master is not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            _cts = new CancellationTokenSource();

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem1.Enabled = true;
            scanResultsGrid.Rows.Clear();

            ScanningStateChanged?.Invoke(this, true);

            try
            {
                byte slaveAddress = (byte)slaveId.Value;
                ushort startAddr = (ushort)startRegister.Value;
                int quantity = (int)numOfRegisters.Value;
                int funcIndex = comboBox1.SelectedIndex;

                for (int i = 0; i < quantity; i++)
                {
                    
                    if (this.IsDisposed) break;
                    if (_cts.Token.IsCancellationRequested)
                    {
                        AddScanResult((ushort)(startAddr + i), "Stopped", "-", Color.Orange, "");
                        break;
                    }

                    ushort currentAddr = (ushort)(startAddr + i);
                    int registersRemaining = quantity - i;
                    if (_modbusMaster.Transport is MP_modbus.ModbusTransportBase transport)
                    {
                        transport.LoggingDeviceName = $"AdresScan_ ({slaveAddress})-[{currentAddr}]";
                    }
                    if (scanResultsGrid.Rows.Count > 0)
                        scanResultsGrid.FirstDisplayedScrollingRowIndex = scanResultsGrid.Rows.Count - 1;

                    // Wywołanie logiki skanowania (zwraca ile rejestrów zużyto)
                    int registersConsumed = await ReadAndLogRegisterStrategy(slaveAddress, currentAddr, funcIndex, registersRemaining);

                    if (registersConsumed == 0)
                    {
                        AddScanResult(currentAddr, "Error", "-", Color.Salmon, "");
                    }
                    else if (registersConsumed > 1)
                    {
                        // Przeskakujemy rejestry, jeśli odczytano blok (np. 32/64 bit)
                        i += (registersConsumed - 1);
                    }

                    if (!_cts.Token.IsCancellationRequested)
                        await Task.Delay(ScanDelayMs);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Scan loop error: {ex.Message}");
            }
            finally
            {
                if (!this.IsDisposed)
                {
                    startToolStripMenuItem.Enabled = true;
                    stopToolStripMenuItem1.Enabled = false;
                }

                if (_cts != null)
                {
                    _cts.Dispose();
                    _cts = null;
                }

                ScanningStateChanged?.Invoke(this, false);
            }
        }

        /// <summary>
        /// Strategia: Próbuj 16 bit -> Błąd? -> 32 bit -> Błąd? -> 48 bit -> Błąd? -> 64 bit
        /// </summary>
        private async Task<int> ReadAndLogRegisterStrategy(byte slaveAddr, ushort regAddr, int funcIndex, int registersAvailable)
        {
            // --- KROK 1: Próba 16-bit (1 rejestr) ---
            try
            {
                ushort[] raw16 = await ReadRawAsync(slaveAddr, regAddr, funcIndex, 1);

                // SUKCES 16-bit
                string valStr;
                string hexStr;

                if (funcIndex <= 1) // Coils/Inputs
                {
                    valStr = raw16[0] > 0 ? "True" : "False";
                    hexStr = raw16[0] > 0 ? "1" : "0";
                }
                else
                {
                    valStr = raw16[0].ToString();
                    hexStr = $"0x{raw16[0]:X4}";
                }

                // Brak dopisku dla 16 bit
                AddScanResult(regAddr, valStr, hexStr, Color.LightGreen, "");
                return 1;
            }
            catch
            {
                // Błąd -> idziemy dalej
            }

            // --- KROK 2: Próba 32-bit (2 rejestry) ---
            if (registersAvailable >= 2)
            {
                try
                {
                    ushort[] raw32 = await ReadRawAsync(slaveAddr, regAddr, funcIndex, 2);

                    // SUKCES 32-bit (Big Endian)
                    uint val32 = ((uint)raw32[0] << 16) | raw32[1];

                    string valStr = val32.ToString();
                    string hexStr = $"0x{raw32[0]:X4} {raw32[1]:X4}";

                    // Dodajemy znacznik (32b) do adresu
                    AddScanResult(regAddr, valStr, hexStr, Color.LightYellow, " (32b)");
                    return 2;
                }
                catch { /* Błąd -> idziemy dalej */ }
            }

            // --- KROK 3: Próba 48-bit (3 rejestry) ---
            if (registersAvailable >= 3)
            {
                try
                {
                    ushort[] raw48 = await ReadRawAsync(slaveAddr, regAddr, funcIndex, 3);

                    // SUKCES 48-bit
                    ulong val48 = ((ulong)raw48[0] << 32) | ((ulong)raw48[1] << 16) | raw48[2];

                    string valStr = val48.ToString();
                    string hexStr = $"0x{raw48[0]:X4} {raw48[1]:X4} {raw48[2]:X4}";

                    AddScanResult(regAddr, valStr, hexStr, Color.LightSkyBlue, " (48b)");
                    return 3;
                }
                catch { /* Błąd -> idziemy dalej */ }
            }

            // --- KROK 4: Próba 64-bit (4 rejestry) ---
            if (registersAvailable >= 4)
            {
                try
                {
                    ushort[] raw64 = await ReadRawAsync(slaveAddr, regAddr, funcIndex, 4);

                    // SUKCES 64-bit
                    ulong val64 = ((ulong)raw64[0] << 48) | ((ulong)raw64[1] << 32) | ((ulong)raw64[2] << 16) | raw64[3];

                    string valStr = val64.ToString();
                    string hexStr = $"0x{raw64[0]:X4} {raw64[1]:X4} {raw64[2]:X4} {raw64[3]:X4}";

                    AddScanResult(regAddr, valStr, hexStr, Color.LightCyan, " (64b)");
                    return 4;
                }
                catch { /* Błąd -> koniec prób */ }
            }

            return 0;
        }

        private async Task<ushort[]> ReadRawAsync(byte slaveAddr, ushort regAddr, int funcIndex, int count)
        {
            if (funcIndex == 0) // Coils
            {
                bool[] b = await _modbusMaster.ReadCoilsAsync(slaveAddr, regAddr, (ushort)count);
                return new ushort[] { (ushort)(b[0] ? 1 : 0) };
            }
            else if (funcIndex == 1) // Discrete Inputs
            {
                bool[] b = await _modbusMaster.ReadInputsAsync(slaveAddr, regAddr, (ushort)count);
                return new ushort[] { (ushort)(b[0] ? 1 : 0) };
            }
            else if (funcIndex == 2) // Holding Registers
            {
                return await _modbusMaster.ReadHoldingRegistersAsync(slaveAddr, regAddr, (ushort)count);
            }
            else // Input Registers
            {
                return await _modbusMaster.ReadInputRegistersAsync(slaveAddr, regAddr, (ushort)count);
            }
        }

        private void StopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }

        // Zaktualizowana metoda przyjmująca sufiks adresu
        private void AddScanResult(ushort addr, string value, string hex, Color backColor, string addressSuffix)
        {
            if (this.IsDisposed || scanResultsGrid.IsDisposed) return;

            if (scanResultsGrid.InvokeRequired)
            {
                scanResultsGrid.Invoke(new Action(() => AddScanResult(addr, value, hex, backColor, addressSuffix)));
                return;
            }

            int idx = scanResultsGrid.Rows.Add();
            DataGridViewRow row = scanResultsGrid.Rows[idx];

            // Tutaj łączymy adres z sufiksem, np. "100 (32b)"
            row.Cells[0].Value = addr.ToString() + addressSuffix;
            row.Cells[1].Value = value;
            row.Cells[2].Value = hex;

            row.DefaultCellStyle.BackColor = backColor;
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (scanResultsGrid.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV File (*.csv)|*.csv";
                sfd.FileName = $"ScanAddress_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Register Address;Response;Response HEX");

                        foreach (DataGridViewRow row in scanResultsGrid.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                string addr = row.Cells[0].Value?.ToString() ?? "";
                                string val = row.Cells[1].Value?.ToString() ?? "";
                                string hex = row.Cells[2].Value?.ToString() ?? "";

                                val = val.Replace(";", ",");
                                sb.AppendLine($"{addr};{val};{hex}");
                            }
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"File save error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void AddressScan_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
            ScanningStateChanged?.Invoke(this, false);
        }

        private void startRegister_1_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingValues) return;
            _isUpdatingValues = true;

            try
            {
                string originalString = (startRegister_1.Value - 1).ToString();
                if (originalString.Length == 0) { _isUpdatingValues = false; return; }

                string functionString = originalString.Substring(0, 1);
                string registerString = originalString.Substring(1);
                decimal registerStringDecimal = Convert.ToDecimal(registerString);
                string requestedFunctionPrefix = "";

                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        if (startRegister_1.Value > 65536 || startRegister_1.Value < 1)
                        {
                            MessageBox.Show("Invalid Register Number. Must be 1-65536.");
                            _isUpdatingValues = false;
                            return;
                        }
                        registerStringDecimal = startRegister_1.Value - 1;
                        break;
                    case 1: requestedFunctionPrefix = "1"; break;
                    case 2: requestedFunctionPrefix = "4"; break;
                    case 3: requestedFunctionPrefix = "3"; break;
                }

                if (functionString != requestedFunctionPrefix && comboBox1.SelectedIndex != 0 || (registerStringDecimal < 0 || registerStringDecimal > 65535))
                {
                    MessageBox.Show("Invalid register range for selected function code. Correcting value.");
                }

                startRegister.Value = registerStringDecimal;
                startRegisterHex.Value = registerStringDecimal;
            }
            finally
            {
                _isUpdatingValues = false;
            }
        }

        private void startRegister_ValueChanged_1(object sender, EventArgs e)
        {
            if (_isUpdatingValues) return;
            _isUpdatingValues = true;

            try
            {
                startRegisterHex.Value = startRegister.Value;
                updateStartAddressDisplay();
            }
            finally
            {
                _isUpdatingValues = false;
            }
        }

        private void startRegisterHex_ValueChanged_1(object sender, EventArgs e)
        {
            if (_isUpdatingValues) return;
            _isUpdatingValues = true;

            try
            {
                startRegister.Value = startRegisterHex.Value;
                updateStartAddressDisplay();
            }
            finally
            {
                _isUpdatingValues = false;
            }
        }

        private void updateStartAddressDisplay()
        {
            decimal requestedFuncionNo = 0;
            switch (comboBox1.SelectedIndex)
            {
                case 0: requestedFuncionNo = 0; break;
                case 1: requestedFuncionNo = 1; break;
                case 2: requestedFuncionNo = 4; break;
                case 3: requestedFuncionNo = 3; break;
            }

            if (startRegister.Value > 0 && startRegister.Value < 10000)
            {
                startRegister_1.Value = requestedFuncionNo * 10000 + startRegister.Value + 1;
            }
            else
            {
                startRegister_1.Value = requestedFuncionNo * 100000 + startRegister.Value + 1;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateStartAddressDisplay();
        }

        private void AddressScan_Load(object sender, EventArgs e)
        {
            MessageBox.Show("Warning: Modbus network scanning generates significant traffic and sends requests to unknown addresses. This may cause instability in legacy devices. Only use this feature during machine downtime or in a test environment. The software author is not liable for any network disruptions.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }
    }
}