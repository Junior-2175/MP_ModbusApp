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

        // Configuration constants to fix the "missing registers" issue
        private const int ScanDelayMs = 50; // Increased from 5ms to 50ms to prevent flooding the device
        private const int MaxRetries = 2;   // Retry up to 2 times if a register fails to respond

        // Event to notify MainWindow about scanning state
        public event EventHandler<bool> ScanningStateChanged;

        public AddressScan(IMyModbusMaster modbusMaster)
        {
            InitializeComponent();
            _modbusMaster = modbusMaster;

            // Initial UI setup
            comboBox1.SelectedIndex = 2; // Default to Holding Registers
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
                return;
            }

            _cts = new CancellationTokenSource();

            // UI State Management
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem1.Enabled = true;
            scanResultsGrid.Rows.Clear();

            // Notify MainWindow (Pause other polling)
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
                        AddScanResult((ushort)(startAddr + i), "Stopped", "-", Color.Orange);
                        break;
                    }

                    ushort currentAddr = (ushort)(startAddr + i);

                    // Auto-scroll
                    if (scanResultsGrid.Rows.Count > 0)
                        scanResultsGrid.FirstDisplayedScrollingRowIndex = scanResultsGrid.Rows.Count - 1;

                    // === RETRY LOGIC START ===
                    bool readSuccess = false;
                    int attempts = 0;

                    while (!readSuccess && attempts <= MaxRetries)
                    {
                        if (_cts.Token.IsCancellationRequested) break;

                        try
                        {
                            await ReadAndLogRegister(slaveAddress, currentAddr, funcIndex);
                            readSuccess = true; // Success, exit retry loop
                        }
                        catch (Exception)
                        {
                            attempts++;
                            if (attempts <= MaxRetries)
                            {
                                // Small wait before retry to let the line settle
                                await Task.Delay(20);
                            }
                        }
                    }

                    if (!readSuccess)
                    {
                        // If all attempts failed, log Timeout
                        AddScanResult(currentAddr, "Timeout", "-", Color.Salmon);
                    }
                    // === RETRY LOGIC END ===

                    // Delay between valid registers to prevent device overload
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

                // Resume polling in MainWindow
                ScanningStateChanged?.Invoke(this, false);
            }
        }

        private async Task ReadAndLogRegister(byte slaveAddr, ushort regAddr, int funcIndex)
        {
            string valStr = "";
            string hexStr = "";

            // NOTE: Exceptions here are caught by the loop above to trigger Retry
            if (funcIndex == 0) // Coils (0x)
            {
                bool[] res = await _modbusMaster.ReadCoilsAsync(slaveAddr, regAddr, 1);
                bool val = res[0];
                valStr = val ? "True" : "False";
                hexStr = val ? "1" : "0";
            }
            else if (funcIndex == 1) // Discrete Inputs (1x)
            {
                bool[] res = await _modbusMaster.ReadInputsAsync(slaveAddr, regAddr, 1);
                bool val = res[0];
                valStr = val ? "True" : "False";
                hexStr = val ? "1" : "0";
            }
            else if (funcIndex == 2) // Holding Registers (4x)
            {
                ushort[] res = await _modbusMaster.ReadHoldingRegistersAsync(slaveAddr, regAddr, 1);
                ushort val = res[0];
                valStr = val.ToString();
                hexStr = $"0x{val:X4}";
            }
            else if (funcIndex == 3) // Input Registers (3x)
            {
                ushort[] res = await _modbusMaster.ReadInputRegistersAsync(slaveAddr, regAddr, 1);
                ushort val = res[0];
                valStr = val.ToString();
                hexStr = $"0x{val:X4}";
            }

            AddScanResult(regAddr, valStr, hexStr, Color.LightGreen);
        }

        private void StopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }

        private void AddScanResult(ushort addr, string value, string hex, Color backColor)
        {
            if (this.IsDisposed || scanResultsGrid.IsDisposed) return;

            if (scanResultsGrid.InvokeRequired)
            {
                scanResultsGrid.Invoke(new Action(() => AddScanResult(addr, value, hex, backColor)));
                return;
            }

            int idx = scanResultsGrid.Rows.Add();
            DataGridViewRow row = scanResultsGrid.Rows[idx];

            row.Cells[0].Value = addr.ToString();
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
    }
}