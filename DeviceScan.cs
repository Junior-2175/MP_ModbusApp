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

        // Główny konstruktor - przyjmuje instancję Mastera Modbus
        public DeviceScan(IMyModbusMaster modbusMaster)
        {
            InitializeComponent();
            _modbusMaster = modbusMaster;
            SetupDataGridView();
        }

        // Konstruktor bezparametrowy (dla Designera Visual Studio)
        public DeviceScan()
        {
            InitializeComponent();
            SetupDataGridView();
        }

        // Konfiguracja kolumn w tabeli wyników
        private void SetupDataGridView()
        {
            // Upewniamy się, że kontrolka istnieje (zabezpieczenie)
            if (scanResultsGrid == null) return;

            scanResultsGrid.Columns.Clear();

            // Dodajemy kolumny
            scanResultsGrid.Columns.Add("colSlaveId", "Adres ID");
            scanResultsGrid.Columns.Add("colStatus", "Status");

            // Ustawienia wyglądu
            scanResultsGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            scanResultsGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            scanResultsGrid.AllowUserToAddRows = false;
            scanResultsGrid.ReadOnly = true;
            scanResultsGrid.RowHeadersVisible = false;
        }

        private void slaveId_ValueChanged(object sender, EventArgs e)
        {
            ValidateRange();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ValidateRange();
        }

        private void ValidateRange()
        {
            int startValue = (int)startId.Value;
            int endValue = (int)endId.Value;
            if (startValue > endValue)
            {
                endId.Value = startValue;
            }
        }

        // --- ROZPOCZĘCIE SKANOWANIA ---
        private async void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem1.Enabled = true;
            if (_modbusMaster == null)
            {
                MessageBox.Show("Modbus Master nie jest zainicjalizowany.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _cts = new CancellationTokenSource();
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem1.Enabled = true;

            // Czyścimy poprzednie wyniki
            scanResultsGrid.Rows.Clear();

            // Pobieramy zakres z kontrolek
            byte start = (byte)startId.Value;
            byte end = (byte)endId.Value;

            // Parametry testowego odczytu (np. rejestr 10, ilość 1)
            ushort registerAddress = 10;
            ushort quantity = 1;

            try
            {
                // Pętla po wszystkich adresach w zakresie
                for (int i = start; i <= end; i++)
                {
                    // Sprawdzenie czy użytkownik wcisnął STOP
                    if (_cts.Token.IsCancellationRequested)
                    {
                        AddScanResult(0, "Stoped", Color.Orange);
                        break;
                    }

                    byte currentSlaveId = (byte)i;

                    // Automatyczne przewijanie tabeli do dołu
                    if (scanResultsGrid.Rows.Count > 0)
                        scanResultsGrid.FirstDisplayedScrollingRowIndex = scanResultsGrid.Rows.Count - 1;

                    try
                    {
                        // Wykonanie zapytania Modbus (FC 03)
                        await _modbusMaster.ReadHoldingRegistersAsync(currentSlaveId, registerAddress, quantity);

                        // Jeśli nie ma wyjątku -> PEŁNY SUKCES (ZIELONY)
                        AddScanResult(currentSlaveId, "Response OK", Color.LightGreen);
                    }
                    catch (TimeoutException)
                    {
                        // Timeout -> BRAK URZĄDZENIA (CZERWONY)
                        AddScanResult(currentSlaveId, "Timeout", Color.Salmon);
                    }
                    catch (MyModbusSlaveException ex)
                    {
                        // Urządzenie odpowiedziało błędem logicznym (np. zły adres) -> URZĄDZENIE JEST (ZIELONY)
                        AddScanResult(currentSlaveId, "Response OK", Color.LightGreen);
                    }
                    catch (IOException ex)
                    {
                        // Urządzenie odpowiedziało, ale ramka jest uszkodzona -> URZĄDZENIE JEST (ZIELONY)
                        AddScanResult(currentSlaveId, "Response OK", Color.LightGreen);
                    }
                    catch (Exception ex)
                    {
                        // Inny błąd
                        AddScanResult(currentSlaveId, "Error", Color.LightYellow);
                    }

                    // Krótkie opóźnienie dla stabilności
                    await Task.Delay(20);
                }
            }
            finally
            {
                startToolStripMenuItem.Enabled = true;
                stopToolStripMenuItem1.Enabled = false;
                if (_cts != null)
                {
                    _cts.Dispose();
                    _cts = null;
                }
                MessageBox.Show("Skanowanie zakończone.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // --- ZATRZYMANIE SKANOWANIA ---
        private void stopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem1.Enabled = false;
            _cts?.Cancel();
        }

        // --- EKSPORT DO CSV ---
        // Pamiętaj, aby podpiąć tę metodę pod przycisk lub menu w Designerze!
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

                        // Nagłówki kolumn
                        sb.AppendLine("Slave ID;Status;Szczegoly");

                        // Dane wierszy
                        foreach (DataGridViewRow row in scanResultsGrid.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                string id = row.Cells[0].Value?.ToString() ?? "";
                                string status = row.Cells[1].Value?.ToString() ?? "";

                                // Zastąpienie ewentualnych średników w tekście, aby nie psuły formatu CSV
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

        // Metoda pomocnicza do dodawania wierszy
        private void AddScanResult(byte slaveId, string status, Color backColor)
        {
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

        private void DeviceScan_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel();
        }
    }
}