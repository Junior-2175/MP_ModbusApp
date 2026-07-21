using MP_ModbusApp.MP_modbus;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MP_ModbusApp
{
    public partial class DeviceScan : Form
    {
        private readonly IMyModbusMaster _modbusMaster;
        private CancellationTokenSource _cts;

        // Event to notify about the current scanning status
        public event EventHandler<bool> ScanningStateChanged;

        public DeviceScan(IMyModbusMaster modbusMaster)
        {
            InitializeComponent();
            _modbusMaster = modbusMaster;
            SetupDataGridView();
        }

        public DeviceScan()
        {
            InitializeComponent();
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            if (scanResultsGrid == null) return;

            scanResultsGrid.Columns.Clear();
            scanResultsGrid.Columns.Add("colSlaveId", "Slave ID");
            scanResultsGrid.Columns.Add("colStatus", "Status");
            scanResultsGrid.Columns.Add("colRetries", "Retries");

            scanResultsGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            scanResultsGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            scanResultsGrid.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            scanResultsGrid.AllowUserToAddRows = false;
            scanResultsGrid.ReadOnly = true;
            scanResultsGrid.RowHeadersVisible = false;
        }

        private void slaveId_ValueChanged(object sender, EventArgs e) { ValidateRange(); }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e) { ValidateRange(); }

        private void ValidateRange()
        {
            if (startId.Value > endId.Value) endId.Value = startId.Value;
        }

        private async void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_modbusMaster == null)
            {
                MessageBox.Show("Modbus Master is not initialized. Configure connection in Setup and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            


            _cts = new CancellationTokenSource();
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem1.Enabled = true;

            scanResultsGrid.Rows.Clear();

            // Notify MainWindow that scanning has started
            ScanningStateChanged?.Invoke(this, true);

            byte start = (byte)startId.Value;
            byte end = (byte)endId.Value;
            ushort registerAddress = 10;
            ushort quantity = 1;

            try
            {
                for (int i = start; i <= end; i++)
                {
                    for (int attempt = 1; attempt < timeoutRetries.Value+1; attempt++)
                    {
                        // Check if form is still active
                        if (this.IsDisposed) break;

                        // Check for cancellation request
                        if (_cts.Token.IsCancellationRequested)
                        {
                            AddScanResult((byte)i, "Stopped","0", Color.Orange);
                            break;
                        }

                        byte currentSlaveId = (byte)i;
                        if (_modbusMaster.Transport is MP_modbus.ModbusTransportBase transport)
                        {
                            transport.LoggingDeviceName = $"DeviceScan_ ({currentSlaveId})";
                        }
                        // Auto-scroll the grid
                        if (!this.IsDisposed && scanResultsGrid.Rows.Count > 0)
                            scanResultsGrid.FirstDisplayedScrollingRowIndex = scanResultsGrid.Rows.Count - 1;

                        try
                        {
                            await _modbusMaster.ReadHoldingRegistersAsync(currentSlaveId, registerAddress, quantity);
                            AddScanResult(currentSlaveId, "Response OK", attempt.ToString(), Color.LightGreen);
                            attempt = (int)timeoutRetries.Value+1;
                        }
                        catch (TimeoutException)
                        {
                            AddScanResult(currentSlaveId, "Timeout", attempt.ToString(), Color.Salmon);
                        }
                        catch (MyModbusSlaveException)
                        {
                            //AddScanResult(currentSlaveId, "Response OK (Exception)", Color.LightGreen);
                            AddScanResult(currentSlaveId, "Response OK", attempt.ToString(), Color.LightGreen);
                            attempt = (int)timeoutRetries.Value+1;
                        }
                        catch (IOException)
                        {
                            //AddScanResult(currentSlaveId, "Response OK (Frame Error)", Color.LightGreen);
                            AddScanResult(currentSlaveId, "Response OK", attempt.ToString(), Color.LightGreen);
                            attempt = (int)timeoutRetries.Value+1;
                        }
                        catch (Exception)
                        {
                            AddScanResult(currentSlaveId, "Error", attempt.ToString(), Color.LightYellow);
                            attempt = (int)timeoutRetries.Value+1;
                        }
                        


                        if (!_cts.Token.IsCancellationRequested)
                            await Task.Delay(20);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Scan loop error: " + ex.Message);
            }
            finally
            {
                // Restore UI state if form is not disposed
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

                // Notify MainWindow to resume normal communication
                ScanningStateChanged?.Invoke(this, false);

                if (!this.IsDisposed)
                {
                    //MessageBox.Show("Scanning finished.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void stopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }

        private void exportToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (scanResultsGrid.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV File (*.csv)|*.csv";
                sfd.FileName = $"ScanResults_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Slave ID;Status;Retries");

                        foreach (DataGridViewRow row in scanResultsGrid.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                string id = row.Cells[0].Value?.ToString() ?? "";
                                string status = row.Cells[1].Value?.ToString() ?? "";
                                status = status.Replace(";", ",");
                                string retries = row.Cells[2].Value?.ToString() ?? "";
                                sb.AppendLine($"{id};{status};{retries}");
                            }
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        //MessageBox.Show("Data exported successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"File save error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Thread-safe method to add scan results to the grid
        private void AddScanResult(byte slaveId, string status, string retries, Color backColor)
        {
            /// - OLD ///  if (this.IsDisposed || scanResultsGrid.IsDisposed) return;
            /// - OLD /// 
            /// - OLD /// if (scanResultsGrid.InvokeRequired)
            /// - OLD /// {
            /// - OLD ///     scanResultsGrid.Invoke(new Action(() => AddScanResult(slaveId, status, retries, backColor)));
            /// - OLD ///     return;
            /// - OLD /// }
            /// - OLD /// 
            /// - OLD /// int rowIndex = scanResultsGrid.Rows.Add();
            /// - OLD /// DataGridViewRow row = scanResultsGrid.Rows[rowIndex];
            /// - OLD /// row.Cells[0].Value = slaveId > 0 ? slaveId.ToString() : "-";
            /// - OLD /// row.Cells[1].Value = status;
            /// - OLD /// row.Cells[2].Value = retries;
            /// - OLD /// row.DefaultCellStyle.BackColor = backColor;


            if (this.IsDisposed || scanResultsGrid.IsDisposed) return;

            if (scanResultsGrid.InvokeRequired)
            {
                scanResultsGrid.Invoke(new Action(() => AddScanResult(slaveId, status, retries, backColor)));
                return;
            }

            DataGridViewRow targetRow = null;

            // Szukaj istniejącego wiersza dla tego Slave ID
            foreach (DataGridViewRow row in scanResultsGrid.Rows)
            {
                if (row.Cells[0].Value?.ToString() == slaveId.ToString())
                {
                    targetRow = row;
                    break;
                }
            }

            // Jeśli nie znaleziono, dodaj nowy
            if (targetRow == null)
            {
                int rowIndex = scanResultsGrid.Rows.Add();
                targetRow = scanResultsGrid.Rows[rowIndex];
                targetRow.Cells[0].Value = slaveId > 0 ? slaveId.ToString() : "-";
            }

            // Aktualizuj dane
            targetRow.Cells[1].Value = status;
            targetRow.Cells[2].Value = retries;
            targetRow.DefaultCellStyle.BackColor = backColor;
        }

        private void DeviceScan_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }

            // Ensure MainWindow resumes communication when scanning window is closed
            ScanningStateChanged?.Invoke(this, false);
        }

        private void DeviceScan_Load(object sender, EventArgs e)
        {
            MessageBox.Show("Warning: Modbus network scanning generates significant traffic and sends requests to unknown addresses. This may cause instability in legacy devices. Only use this feature during machine downtime or in a test environment. The software author is not liable for any network disruptions.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}