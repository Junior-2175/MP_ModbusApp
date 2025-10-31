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

        /// <summary>
        /// The master list of all log entries.
        /// </summary>
        private readonly BindingList<ModbusFrameLog> _frameLogs = new BindingList<ModbusFrameLog>();
       
        /// <summary>
        /// The filtered list of log entries, bound to the DataGridView.
        /// </summary>
        private readonly BindingList<ModbusFrameLog> _filteredLogs = new BindingList<ModbusFrameLog>();

        /// <summary>
        /// The BindingSource acts as a proxy between the DataGridView and the master list (_frameLogs).
        /// This is crucial for allowing filtering (via the 'Filter' property) without
        /// modifying the underlying complete list of logs.
        /// </summary>
        //private readonly BindingSource _bindingSource = new BindingSource();

        public CommunicationLogWindow()
        {
            InitializeComponent();

            // Set the BindingSource as the intermediary
            _bindingSource.DataSource = _filteredLogs;
            dgvLog.DataSource = _bindingSource;

            // DataGridView Configuration
            dgvLog.AllowUserToAddRows = false;
            dgvLog.AllowUserToResizeColumns = true;
            dgvLog.ReadOnly = true;
            dgvLog.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvLog.RowHeadersVisible = false;

            // Individual column settings
            dgvLog.Columns["Timestamp"].HeaderText = "Time";
            dgvLog.Columns["Timestamp"].DefaultCellStyle.Format = "HH:mm:ss.fff";

            if (dgvLog.Columns["DeviceName"] != null)
            {
                dgvLog.Columns["DeviceName"].HeaderText = "Device";
                dgvLog.Columns["DeviceName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            dgvLog.Columns["Direction"].HeaderText = "Dir";
            dgvLog.Columns["DataFrame"].HeaderText = "Data Frame";

            dgvLog.Columns["ErrorDescription"].HeaderText = "Error";
            dgvLog.Columns["ErrorDescription"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        /// <summary>
        /// Adds a log entry to the list. 
        /// This method MUST be called on the UI thread.
        /// </summary>
        private void AddLogEntry(ModbusFrameLog logEntry)
        {
            if (this.IsDisposed) return; // Safeguard

            _frameLogs.Add(logEntry); // Add to the master list

            // Add the device to the filter dropdown if it's new
            if (!string.IsNullOrEmpty(logEntry.DeviceName) && !cboDeviceFilter.Items.Contains(logEntry.DeviceName))
            {
                cboDeviceFilter.Items.Add(logEntry.DeviceName);
            }

            // Check if the new log entry matches the current filter
            string currentFilter = cboDeviceFilter.SelectedItem?.ToString() ?? "(Show All)";

            if (currentFilter == "(Show All)" || logEntry.DeviceName == currentFilter)
            {
                // If it matches, add it to the filtered list
                _filteredLogs.Add(logEntry);
            }
        }

        /// <summary>
        /// Public, thread-safe method to add a log entry to the window.
        /// </summary>
        public void LogFrame(ModbusFrameLog logEntry)
        {
            if (!_isLoggingActive) return;

            if (InvokeRequired)
            {
                // Use Invoke to maintain log order and safely update the UI collection
                Invoke(new Action(() => AddLogEntry(logEntry)));
            }
            else
            {
                AddLogEntry(logEntry);
            }
        }

        /// <summary>
        /// Rebuilds the filtered list (_filteredLogs) based on
        /// the main list (_frameLogs) and the current ComboBox selection.
        /// </summary>
        private void ApplyFilter()
        {
            string selectedDevice = cboDeviceFilter.SelectedItem?.ToString() ?? "(Show All)";

            // Disable change notifications while rebuilding the list (for performance)
            _filteredLogs.RaiseListChangedEvents = false;
            _filteredLogs.Clear();

            try
            {
                // Use LINQ to quickly find matching entries
                IEnumerable<ModbusFrameLog> itemsToAdd;
                if (selectedDevice == "(Show All)")
                {
                    itemsToAdd = _frameLogs;
                }
                else
                {
                    itemsToAdd = _frameLogs.Where(log => log.DeviceName == selectedDevice);
                }

                // Add the found entries to the filtered list
                foreach (var log in itemsToAdd)
                {
                    _filteredLogs.Add(log);
                }
            }
            finally
            {
                // Enable notifications and notify the DataGridView that the data has changed
                _filteredLogs.RaiseListChangedEvents = true;
                _filteredLogs.ResetBindings();
            }
        }

        /// <summary>
        /// Public, thread-safe method to notify the window of an error.
        /// If "Stop on Error" is checked, logging will be halted.
        /// </summary>
        public void NotifyCommunicationError()
        {
            if (chkStopOnError.Checked && _isLoggingActive)
            {
                // Use Invoke to safely update UI from another thread
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

        /// <summary>
        /// Stops logging and shows a notification. Must be called on the UI thread.
        /// </summary>
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
            _filteredLogs.Clear();
            cboDeviceFilter.Items.Clear();
            cboDeviceFilter.Items.Add("(Show All)");
            cboDeviceFilter.SelectedItem = "(Show All)";
        }

        private void btnExportToCsv_Click(object sender, EventArgs e)
        {
            // Check the underlying list
            //if (_frameLogs.Count == 0)
            if (_filteredLogs.Count == 0)
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
                        // CSV Header
                        sb.AppendLine("Timestamp,DeviceName,Direction,DataFrame,ErrorDescription");

                        // Iterate over the master list (_filteredLogs) to ensure all data is exported,
                        // just the filtered data currently in _bindingSource.List.
                        //foreach (var log in _frameLogs)
                        foreach (var log in _filteredLogs)
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

        private void CommunicationLogWindow_Load(object sender, EventArgs e)
        {
            btnStartLogging.Enabled = false;
            btnStopLogging.Enabled = true;

            // Initialize the filter ComboBox
            cboDeviceFilter.Items.Add("(Show All)");
            cboDeviceFilter.SelectedItem = "(Show All)";
        }

        /// <summary>
        /// Handles mouse clicks on the DataGridView, primarily for the context menu.
        /// </summary>
        private void dgvLog_MouseDown(object sender, MouseEventArgs e)
        {
            var hitTestInfo = dgvLog.HitTest(e.X, e.Y);

            // If clicking outside the cells, clear selection
            if (e.Button == MouseButtons.Left && hitTestInfo.Type == DataGridViewHitTestType.None)
            {
                dgvLog.ClearSelection();
            }

            if (e.Button == MouseButtons.Right)
            {
                if (hitTestInfo.RowIndex >= 0 && hitTestInfo.ColumnIndex >= 0)
                {
                    // Select the cell that was right-clicked
                    if (!dgvLog.Rows[hitTestInfo.RowIndex].Cells[hitTestInfo.ColumnIndex].Selected)
                    {
                        dgvLog.ClearSelection();
                        dgvLog.Rows[hitTestInfo.RowIndex].Cells[hitTestInfo.ColumnIndex].Selected = true;
                    }

                    contextMenuStrip1.Show(Cursor.Position);
                }
            }
        }

        /// <summary>
        /// Copies the selected DataGridView content to the clipboard.
        /// </summary>
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.dgvLog.GetClipboardContent() == null)
            {
                return;
            }
            DataObject dataObj = this.dgvLog.GetClipboardContent();
            Clipboard.SetDataObject(dataObj);
        }

        // --- Filter Handler Methods ---

        /// <summary>
        /// Applies a filter to the BindingSource when the ComboBox selection changes.
        /// </summary>
        private void cboDeviceFilter_SelectedIndexChanged(object sender, EventArgs e)
        {

            ApplyFilter();
        }

        /// <summary>
        /// Clears the filter on the BindingSource.
        /// </summary>
        private void btnClearFilter_Click(object sender, EventArgs e)
        {
            cboDeviceFilter.SelectedItem = "(Show All)";
        }
    }
}