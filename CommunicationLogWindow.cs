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

        public CommunicationLogWindow()
        {
            InitializeComponent();
            dgvLog.DataSource = _frameLogs;

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

            dgvLog.Columns["TransactionID"].HeaderText = "Trans. ID";

            dgvLog.Columns["Direction"].HeaderText = "Dir";

            dgvLog.Columns["DataFrame"].HeaderText = "Data Frame";

            dgvLog.Columns["ErrorDescription"].HeaderText = "Error";
            dgvLog.Columns["ErrorDescription"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        public void LogFrame(ModbusFrameLog logEntry)
        {
            if (!_isLoggingActive) return;

            if (InvokeRequired)
            {
                Invoke(new Action(() => _frameLogs.Add(logEntry)));
            }
            else
            {
                _frameLogs.Add(logEntry);
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
        }

        private void btnExportToCsv_Click(object sender, EventArgs e)
        {
            if (dgvLog.Rows.Count == 0)
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
                        sb.AppendLine("Timestamp,TransactionID,Direction,Frame,ErrorDescription");

                        foreach (var log in _frameLogs)
                        {
                            sb.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff},{log.TransactionID},{log.Direction},\"{log.DataFrame}\",\"{log.ErrorDescription}\"");
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

        private void CommunicationLogWindow_Load(object sender, EventArgs e)
        {
            btnStartLogging.Enabled = false;
            btnStopLogging.Enabled = true;
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
    }
}
