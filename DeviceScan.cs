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
    public partial class DeviceScan : Form
    {
        private readonly IMyModbusMaster _modbusMaster;
        private CancellationTokenSource _cts;

        // Zdarzenie informujące o stanie skanowania
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
            scanResultsGrid.Columns.Add("colSlaveId", "Adres ID");
            scanResultsGrid.Columns.Add("colStatus", "Status");

            scanResultsGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            scanResultsGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

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

        // --- ROZPOCZĘCIE SKANOWANIA ---
        private async void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_modbusMaster == null)
            {
                MessageBox.Show("Modbus Master nie jest zainicjalizowany.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _cts = new CancellationTokenSource();
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem1.Enabled = true;

            scanResultsGrid.Rows.Clear();

            // Powiadom MainWindow o starcie
            ScanningStateChanged?.Invoke(this, true);

            byte start = (byte)startId.Value;
            byte end = (byte)endId.Value;
            ushort registerAddress = 10;
            ushort quantity = 1;

            try
            {
                for (int i = start; i <= end; i++)
                {
                    // 1. Sprawdź czy okno nie zostało zamknięte
                    if (this.IsDisposed) break;

                    // 2. Sprawdź czy użytkownik nie anulował
                    if (_cts.Token.IsCancellationRequested)
                    {
                        AddScanResult(0, "Stopped", Color.Orange);
                        break;
                    }

                    byte currentSlaveId = (byte)i;

                    // Przewijanie tabeli (tylko jeśli okno istnieje)
                    if (!this.IsDisposed && scanResultsGrid.Rows.Count > 0)
                        scanResultsGrid.FirstDisplayedScrollingRowIndex = scanResultsGrid.Rows.Count - 1;

                    try
                    {
                        await _modbusMaster.ReadHoldingRegistersAsync(currentSlaveId, registerAddress, quantity);
                        AddScanResult(currentSlaveId, "Response OK", Color.LightGreen);
                    }
                    catch (TimeoutException)
                    {
                        AddScanResult(currentSlaveId, "Timeout", Color.Salmon);
                    }
                    catch (MyModbusSlaveException)
                    {
                        AddScanResult(currentSlaveId, "Response OK (Exception)", Color.LightGreen);
                    }
                    catch (IOException)
                    {
                        AddScanResult(currentSlaveId, "Response OK (Frame Error)", Color.LightGreen);
                    }
                    catch (Exception)
                    {
                        AddScanResult(currentSlaveId, "Error", Color.LightYellow);
                    }

                    // Opóźnienie (chyba że anulowano)
                    if (!_cts.Token.IsCancellationRequested)
                        await Task.Delay(20);
                }
            }
            catch (Exception ex)
            {
                // Logowanie błędów krytycznych pętli (opcjonalne)
                System.Diagnostics.Debug.WriteLine("Błąd pętli skanowania: " + ex.Message);
            }
            finally
            {
                // --- KLUCZOWA ZMIANA: Sprawdzamy czy okno nadal istnieje ---
                // Nie możemy odwoływać się do kontrolek UI (ToolStripMenuItem), jeśli forma jest Disposed

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

                // Powiadom MainWindow, że można wznowić komunikację (to jest bezpieczne)
                ScanningStateChanged?.Invoke(this, false);

                // Wyświetl komunikat o sukcesie TYLKO jeśli okno nadal jest otwarte
                if (!this.IsDisposed)
                {
                    MessageBox.Show("Skanowanie zakończone.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("Brak danych do wyeksportowania.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Plik CSV (*.csv)|*.csv";
                sfd.FileName = "ScanResults.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Slave ID;Status");

                        foreach (DataGridViewRow row in scanResultsGrid.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                string id = row.Cells[0].Value?.ToString() ?? "";
                                string status = row.Cells[1].Value?.ToString() ?? "";
                                status = status.Replace(";", ",");
                                sb.AppendLine($"{id};{status}");
                            }
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Dane wyeksportowane pomyślnie!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Błąd podczas zapisu pliku: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Metoda bezpiecznie dodająca wiersz (nawet z innego wątku)
        private void AddScanResult(byte slaveId, string status, Color backColor)
        {
            // ZABEZPIECZENIE PRZED BŁĘDEM: Jeśli okno lub grid nie istnieje, wyjdź
            if (this.IsDisposed || scanResultsGrid.IsDisposed) return;

            if (scanResultsGrid.InvokeRequired)
            {
                scanResultsGrid.Invoke(new Action(() => AddScanResult(slaveId, status, backColor)));
                return;
            }

            int rowIndex = scanResultsGrid.Rows.Add();
            DataGridViewRow row = scanResultsGrid.Rows[rowIndex];
            row.Cells[0].Value = slaveId > 0 ? slaveId.ToString() : "-";
            row.Cells[1].Value = status;
            row.DefaultCellStyle.BackColor = backColor;
        }

        // Obsługa zamykania okna "krzyżykiem"
        private void DeviceScan_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Jeśli skanowanie trwa, anuluj je
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }

            // Upewnij się, że MainWindow wie, że ma wznowić komunikację
            // Wywołujemy to tutaj, bo blok 'finally' może się nie wykonać w odpowiednim momencie
            // lub zostać przerwany, gdy okno jest niszczone.
            ScanningStateChanged?.Invoke(this, false);
        }
    }
}