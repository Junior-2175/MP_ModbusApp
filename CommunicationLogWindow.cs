using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MP_ModbusApp
{
    public partial class CommunicationLogWindow : Form
    {
        private bool _isLoggingActive = true;
        private readonly BindingList<ModbusFrameLog> _frameLogs = new BindingList<ModbusFrameLog>();

        // --- NOWY KOD ---
        private readonly BindingSource _bindingSource = new BindingSource();
        // --- KONIEC NOWEGO KODU ---

        public CommunicationLogWindow()
        {
            InitializeComponent();

            // --- ZMIENIONY KOD ---
            // Użyj BindingSource jako pośrednika
            _bindingSource.DataSource = _frameLogs;
            dgvLog.DataSource = _bindingSource;
            // --- KONIEC ZMIENIONEGO KODU ---


            // Konfiguracja DataGridView
            dgvLog.AllowUserToAddRows = false;
            dgvLog.AllowUserToResizeColumns = true;
            dgvLog.ReadOnly = true;
            dgvLog.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvLog.RowHeadersVisible = false;

            // Ustawienia poszczególnych kolumn
            dgvLog.Columns["Timestamp"].HeaderText = "Time";
            dgvLog.Columns["Timestamp"].DefaultCellStyle.Format = "HH:mm:ss.fff";

            // --- NOWY KOD DLA KOLUMNY DEVICENAME ---
            if (dgvLog.Columns["DeviceName"] != null)
            {
                dgvLog.Columns["DeviceName"].HeaderText = "Device";
                dgvLog.Columns["DeviceName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            // --- KONIEC NOWEGO KODU ---

            //dgvLog.Columns["TransactionID"].HeaderText = "Trans. ID";
            //dgvLog.Columns["TransactionID"].Visible = false; // Ukryjmy ją, skoro nie jest używana

            dgvLog.Columns["Direction"].HeaderText = "Dir";

            dgvLog.Columns["DataFrame"].HeaderText = "Data Frame";

            dgvLog.Columns["ErrorDescription"].HeaderText = "Error";
            dgvLog.Columns["ErrorDescription"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // --- NOWY KOD: Podłączenie handlerów ---
            this.cboDeviceFilter.SelectedIndexChanged += new System.EventHandler(this.cboDeviceFilter_SelectedIndexChanged);
            this.btnClearFilter.Click += new System.EventHandler(this.btnClearFilter_Click);
            // --- KONIEC NOWEGO KODU ---
        }

        // --- ZMIENIONA METODA ---
        // Ta metoda jest teraz wywoływana bezpiecznie w wątku UI
        private void AddLogEntry(ModbusFrameLog logEntry)
        {
            if (this.IsDisposed) return; // Zabezpieczenie

            _frameLogs.Add(logEntry); // Dodaj do głównej listy

            // Dodaj urządzenie do filtra, jeśli jest nowe
            if (!string.IsNullOrEmpty(logEntry.DeviceName) && !cboDeviceFilter.Items.Contains(logEntry.DeviceName))
            {
                cboDeviceFilter.Items.Add(logEntry.DeviceName);
            }
        }

        // --- ZMIENIONA METODA ---
        public void LogFrame(ModbusFrameLog logEntry)
        {
            if (!_isLoggingActive) return;

            if (InvokeRequired)
            {
                // Użyj Invoke, aby zachować kolejność logów i bezpiecznie zaktualizować UI
                Invoke(new Action(() => AddLogEntry(logEntry)));
            }
            else
            {
                AddLogEntry(logEntry);
            }
        }

        public void NotifyCommunicationError()
        {
            if (chkStopOnError.Checked && _isLoggingActive)
            {
                // Użyj Invoke, aby bezpiecznie zaktualizować UI z innego wątku
                if (InvokeRequired)
                {
                    Invoke(new Action(StopLoggingDueToError));
                }
                else
                {
                    StopLoggingDueToError();
                }
            }
        }

        private void StopLoggingDueToError()
        {
            _isLoggingActive = false;
            btnStartLogging.Enabled = true;
            btnStopLogging.Enabled = false;
            MessageBox.Show("Logging has been stopped due to a communication error.", "Logging Stopped", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnStartLogging_Click(object sender, EventArgs e)
        {
            _isLoggingActive = true;
            btnStartLogging.Enabled = false;
            btnStopLogging.Enabled = true;
        }

        private void btnStopLogging_Click(object sender, EventArgs e)
        {
            _isLoggingActive = false;
            btnStartLogging.Enabled = true;
            btnStopLogging.Enabled = false;
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            _frameLogs.Clear();
            cboDeviceFilter.Items.Clear();
            cboDeviceFilter.Items.Add("(Show All)");
            cboDeviceFilter.SelectedItem = "(Show All)";
        }

        // --- ZMIENIONA METODA ---
        private void btnExportToCsv_Click(object sender, EventArgs e)
        {
            // Zmieniono dgvLog.Rows.Count na _bindingSource.List.Count
            if (_bindingSource.List.Count == 0)
            {
                MessageBox.Show("No data to export.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV File (*.csv)|*.csv";
                saveFileDialog.FileName = $"ModbusLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        // --- NOWY KOD: Dodano kolumnę DeviceName ---
                        sb.AppendLine("Timestamp,DeviceName,TransactionID,Direction,Frame,ErrorDescription");
                        // --- KONIEC NOWEGO KODU ---

                        // Użyj _frameLogs, aby eksportować wszystko,
                        // a nie _bindingSource.List (które zawiera tylko przefiltrowane)
                        foreach (var log in _frameLogs)
                        {
                            sb.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff},{log.DeviceName},{log.Direction},\"{log.DataFrame}\",\"{log.ErrorDescription}\"");
                        }
                        File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                        MessageBox.Show("Export completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error during export: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // --- ZMIENIONA METODA ---
        private void CommunicationLogWindow_Load(object sender, EventArgs e)
        {
            btnStartLogging.Enabled = false;
            btnStopLogging.Enabled = true;

            // --- NOWY KOD FILTROWANIA ---
            cboDeviceFilter.Items.Add("(Show All)");
            cboDeviceFilter.SelectedItem = "(Show All)";
            // --- KONIEC NOWEGO KODU ---
        }

        private void dgvLog_MouseDown(object sender, MouseEventArgs e)
        {
            var hitTestInfo = dgvLog.HitTest(e.X, e.Y);

            if (e.Button == MouseButtons.Left && hitTestInfo.Type == DataGridViewHitTestType.None)
            {
                dgvLog.ClearSelection();
            }

            if (e.Button == MouseButtons.Right)
            {
                if (hitTestInfo.RowIndex >= 0 && hitTestInfo.ColumnIndex >= 0)
                {
                    if (!dgvLog.Rows[hitTestInfo.RowIndex].Cells[hitTestInfo.ColumnIndex].Selected)
                    {
                        dgvLog.ClearSelection();
                        dgvLog.Rows[hitTestInfo.RowIndex].Cells[hitTestInfo.ColumnIndex].Selected = true;
                    }

                    contextMenuStrip1.Show(Cursor.Position);
                }
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.dgvLog.GetClipboardContent() == null)
            {
                return;
            }
            DataObject dataObj = this.dgvLog.GetClipboardContent();
            Clipboard.SetDataObject(dataObj);
        }

        // --- NOWE METODY HANDLERÓW FILTROWANIA ---
        private void cboDeviceFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDeviceFilter.SelectedItem == null) return;

            string selectedDevice = cboDeviceFilter.SelectedItem.ToString();

            if (selectedDevice == "(Show All)")
            {
                _bindingSource.Filter = null;
            }
            else
            {
                // Budujemy filtr. Używamy string.Format, aby poprawnie obsłużyć
                // nazwy, które mogłyby zawierać apostrof.
                _bindingSource.Filter = string.Format("DeviceName = '{0}'", selectedDevice.Replace("'", "''"));
            }
        }

        private void btnClearFilter_Click(object sender, EventArgs e)
        {
            _bindingSource.Filter = null;
            cboDeviceFilter.SelectedItem = "(Show All)";
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
        // --- KONIEC NOWYCH METOD ---
    }
}