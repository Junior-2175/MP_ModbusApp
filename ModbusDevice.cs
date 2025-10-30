using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP_ModbusApp
{
    public partial class ModbusDevice : Form
    {

        private MainWindow _mainWindow;
        private MP_modbus.IMyModbusMaster _modbusMaster;
        private bool _isPolling = false;


        public event EventHandler DeviceSaved;

        public string DeviceName
        {
            get => this.Text;
            set => this.Text = value;
        }

        public int SlaveId
        {
            get => (int)slaveId.Value;
            set => slaveId.Value = value;
        }

        public TabControl DeviceTabControl => tabPanel1;

        private TabPage tabToRename = null;
        private int tabNo = 0;
        public ModbusDevice()
        {
            InitializeComponent();
            addToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+ + ";
            removeToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+ - ";
            newReadingsToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+ + ";

            // Connect the "Pause" button event
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
        }

        /// <summary>
        /// Handles the form load event. Initializes tabs and starts polling.
        /// </summary>
        private async void ModbusGroup_Load(object sender, EventArgs e)
        {
            if (this.MdiParent is MainWindow mw)
            {
                _mainWindow = mw;
            }

            lblDeviceStatus.Visible = false;
            tabNo = tabPanel1.TabPages.Count;
            if (tabPanel1.TabPages.Count == 0)
            {
                TabPage newTab = new TabPage();
                tabNo = tabNo + 1;
                tabPanel1.SelectedTab = newTab;
                newTab.Text = "Readings " + tabNo;
                newTab.Controls.Add(new ReadingsTab() { Dock = DockStyle.Fill });
                tabPanel1.TabPages.Add(newTab);
            }
            txtRenameTab.Visible = false;

            // Automatically start polling after a short delay
            await Task.Delay(100);
            startToolStripMenuItem_Click(sender, e);
        }

        /// <summary>
        /// Adds a new readings tab.
        /// </summary>
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage newTab = new TabPage();
            tabNo = tabNo + 1;
            tabPanel1.SelectedTab = newTab;
            newTab.Text = "Readings " + tabNo;
            newTab.Controls.Add(new ReadingsTab() { Dock = DockStyle.Fill });
            tabPanel1.TabPages.Add(newTab);
        }

        /// <summary>
        /// Removes the currently selected tab.
        /// </summary>
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabPanel1.SelectedTab != null)
            {
                tabPanel1.TabPages.Remove(tabPanel1.SelectedTab);
            }
        }

        /// <summary>
        /// Initiates the tab rename process by showing a TextBox over the tab.
        /// </summary>
        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabToRename = tabPanel1.SelectedTab;
            if (tabToRename == null) return;

            Rectangle tabRect = tabPanel1.GetTabRect(tabPanel1.SelectedIndex);
            Point textCords = new Point(
                tabPanel1.Left + tabRect.Left,
                tabPanel1.Top + tabRect.Top);
            txtRenameTab.Location = textCords;
            txtRenameTab.Size = tabRect.Size;
            txtRenameTab.Text = tabToRename.Text;

            txtRenameTab.Visible = true;
            txtRenameTab.BringToFront();
            txtRenameTab.Focus();
            txtRenameTab.SelectAll();
        }

        /// <summary>
        /// Handles right-clicking on the tab control to show the context menu.
        /// </summary>
        private void tabPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tabPanel1.TabCount; i++)
                {
                    Rectangle tabRect = tabPanel1.GetTabRect(i);
                    if (tabRect.Contains(e.Location))
                    {
                        tabPanel1.SelectedIndex = i;
                        contextMenuStrip1.Show(tabPanel1, e.Location);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Handles key presses in the rename TextBox (Enter to confirm, Escape to cancel).
        /// </summary>
        private void txtRenameTab_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AllowRename();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                CancelRename();
            }
        }

        /// <summary>
        /// Applies the new name to the tab and hides the rename TextBox.
        /// </summary>
        private void AllowRename()
        {
            if (tabToRename == null) return;
            if (!string.IsNullOrWhiteSpace(txtRenameTab.Text))
            {
                tabToRename.Text = txtRenameTab.Text;
            }

            txtRenameTab.Visible = false;
            tabToRename = null;
            txtRenameTab.Parent = this;
        }

        /// <summary>
        /// Cancels the tab rename process.
        /// </summary>
        private void CancelRename()
        {
            txtRenameTab.Visible = false;
            tabToRename = null;
            txtRenameTab.Parent = this;
        }

        private void ModbusDevice_Resize(object sender, EventArgs e)
        {
            CancelRename();
        }

        /// <summary>
        /// Handles the "New readings" menu item click (same as addToolStripMenuItem_Click).
        /// </summary>
        private void newReadingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage newTab = new TabPage();
            tabNo = tabNo + 1;
            tabPanel1.SelectedTab = newTab;
            newTab.Text = "Readings " + tabNo;
            newTab.Controls.Add(new ReadingsTab() { Dock = DockStyle.Fill });
            tabPanel1.TabPages.Add(newTab);
        }

        /// <summary>
        /// Handles double-clicking on a tab to initiate rename.
        /// </summary>
        private void tabPanel1_DoubleClick(object sender, EventArgs e)
        {
            tabToRename = tabPanel1.SelectedTab;
            if (tabToRename == null) return;

            Rectangle tabRect = tabPanel1.GetTabRect(tabPanel1.SelectedIndex);
            Point textCords = new Point(
                tabPanel1.Left + tabRect.Left,
                tabPanel1.Top + tabRect.Top);
            txtRenameTab.Location = textCords;
            txtRenameTab.Size = tabRect.Size;
            txtRenameTab.Text = tabToRename.Text;

            txtRenameTab.Visible = true;
            txtRenameTab.BringToFront();
            txtRenameTab.Focus();
            txtRenameTab.SelectAll();

        }

        /// <summary>
        /// Handles the "Save" menu item click, prompting for a device name.
        /// </summary>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string oldName = this.Text;
            string newName;

            using (RenameForm renameDialog = new RenameForm())
            {
                renameDialog.newName = oldName;
                if (renameDialog.ShowDialog() == DialogResult.OK)
                {
                    newName = renameDialog.newName;
                }
                else
                {
                    return; // User cancelled
                }
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Please provide a device name to save.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Focus();
                return;
            }

            this.Text = newName;

            try
            {
                SaveDeviceConfiguration();
                MessageBox.Show("Device saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DeviceSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the device: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        /// <summary>
        /// Saves the entire device configuration (device, groups, registers) to the database.
        /// This performs an "upsert" logic: updates if the name exists, inserts if it's new.
        /// </summary>
        private void SaveDeviceConfiguration()
        {
            using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    long deviceId;

                    // Check if a device with this name already exists
                    var checkCmd = connection.CreateCommand();
                    checkCmd.Transaction = transaction;
                    checkCmd.CommandText = "SELECT DeviceId FROM Devices WHERE DeviceName = $name;";
                    checkCmd.Parameters.AddWithValue("$name", this.Text);

                    object existingIdResult = checkCmd.ExecuteScalar();

                    if (existingIdResult != null)
                    {
                        // --- UPDATE path ---
                        deviceId = (long)existingIdResult;

                        // Delete old groups and registers (they will be re-added)
                        var deleteGroupsCmd = connection.CreateCommand();
                        deleteGroupsCmd.Transaction = transaction;
                        deleteGroupsCmd.CommandText = "DELETE FROM ReadingGroups WHERE DeviceId = $deviceId;";
                        deleteGroupsCmd.Parameters.AddWithValue("$deviceId", deviceId);
                        deleteGroupsCmd.ExecuteNonQuery();

                        // Update the device's SlaveId
                        var updateDeviceCmd = connection.CreateCommand();
                        updateDeviceCmd.Transaction = transaction;
                        updateDeviceCmd.CommandText = "UPDATE Devices SET SlaveId = $slaveId WHERE DeviceId = $deviceId;";
                        updateDeviceCmd.Parameters.AddWithValue("$slaveId", (int)slaveId.Value);
                        updateDeviceCmd.Parameters.AddWithValue("$deviceId", deviceId);
                        updateDeviceCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // --- INSERT path ---
                        var insertDeviceCmd = connection.CreateCommand();
                        insertDeviceCmd.Transaction = transaction;
                        insertDeviceCmd.CommandText = "INSERT INTO Devices (DeviceName, SlaveId) VALUES ($name, $slaveId) RETURNING DeviceId;";
                        insertDeviceCmd.Parameters.AddWithValue("$name", this.Text);
                        insertDeviceCmd.Parameters.AddWithValue("$slaveId", (int)slaveId.Value);

                        deviceId = (long)insertDeviceCmd.ExecuteScalar();
                    }

                    // --- Save all tabs (ReadingGroups) and their registers ---
                    foreach (TabPage tabPage in tabPanel1.TabPages)
                    {
                        if (tabPage.Controls[0] is ReadingsTab readingsTab)
                        {
                            // Insert the ReadingGroup
                            var groupCmd = connection.CreateCommand();
                            groupCmd.Transaction = transaction;
                            groupCmd.CommandText = @"
                        INSERT INTO ReadingGroups (DeviceId, GroupName, FunctionCode, StartAddress, Quantity)
                        VALUES ($deviceId, $groupName, $funcCode, $startAddr, $quantity)
                        RETURNING GroupId;";

                            groupCmd.Parameters.AddWithValue("$deviceId", deviceId);
                            groupCmd.Parameters.AddWithValue("$groupName", tabPage.Text);
                            groupCmd.Parameters.AddWithValue("$funcCode", readingsTab.GetFunctionCode());
                            groupCmd.Parameters.AddWithValue("$startAddr", readingsTab.GetStartAddress());
                            groupCmd.Parameters.AddWithValue("$quantity", readingsTab.GetQuantity());

                            long groupId = (long)groupCmd.ExecuteScalar();

                            int startAddrForThisTab = readingsTab.GetStartAddress();

                            // Insert all RegisterDefinitions for this group
                            foreach (DataGridViewRow row in readingsTab.GetDataGridViewRows())
                            {
                                if (row.IsNewRow) continue;
                                int registerNumber = startAddrForThisTab + row.Index;

                                var regCmd = connection.CreateCommand();
                                regCmd.Transaction = transaction;
                                regCmd.CommandText = @"
                            INSERT INTO RegisterDefinitions (GroupId, RegisterNumber, RegisterName, DisplayFormatColumn)
                            VALUES ($groupId, $regNum, $regName, $displayFormat);";

                                regCmd.Parameters.AddWithValue("$groupId", groupId);
                                regCmd.Parameters.AddWithValue("$regNum", registerNumber);
                                regCmd.Parameters.AddWithValue("$regName", row.Cells["Name"].Value ?? string.Empty);

                                var displayFormat = row.Cells["DisplayFormatColumn"].Value?.ToString() ?? "Unsigned16";
                                regCmd.Parameters.AddWithValue("$displayFormat", displayFormat);
                                regCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Handles the "Start" polling menu item click.
        /// </summary>
        private async void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _modbusMaster = _mainWindow?.ModbusMaster;
            if (_modbusMaster == null)
            {
                ShowDeviceError("Not connected in Setup tab!");
                return;
            }

            if (_isPolling) return;
            _isPolling = true;

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;

            await PollDeviceLoop();
        }

        /// <summary>
        /// The main polling loop. Continues as long as _isPolling is true.
        /// </summary>
        private async Task PollDeviceLoop()
        {
            while (_isPolling)
            {
                await PollDeviceOnce();

                if (!_isPolling) break;

                int pollDelay = _mainWindow.GetPollDelay();

                await Task.Delay(pollDelay);
            }
        }

        /// <summary>
        /// Public, thread-safe method to stop polling.
        /// Can be called from other threads (e.g., MainWindow).
        /// </summary>
        public void StopPolling()
        {
            // Set the flag to stop the loop
            _isPolling = false;

            // Update UI controls, ensuring it's on the UI thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    startToolStripMenuItem.Enabled = true;
                    stopToolStripMenuItem.Enabled = false;
                }));
            }
            else
            {
                startToolStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = false;
            }
        }

        /// <summary>
        /// Handles the "Pause" button click.
        /// </summary>
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // It just calls the new, public, thread-safe method
            this.StopPolling();
        }

        /// <summary>
        /// Displays a device-level error message. Thread-safe.
        /// </summary>
        private void ShowDeviceError(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowDeviceError(message)));
                return;
            }
            lblDeviceStatus.Visible = true;
            lblDeviceStatus.Text = $"Device Error: {message}";
            lblDeviceStatus.ForeColor = Color.Red;
        }

        /// <summary>
        /// Clears the device-level error message. Thread-safe.
        /// </summary>
        private void ClearDeviceError()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearDeviceError));
                return;
            }
            lblDeviceStatus.Visible = true;
            lblDeviceStatus.Text = _isPolling ? " " : "Polling Paused"; // Show "Paused" instead of empty
            lblDeviceStatus.ForeColor = _isPolling ? Color.Green : Color.Black; // Green for OK, Black for Paused
        }

        /// <summary>
        /// Performs a single polling cycle, iterating through all tabs.
        /// </summary>
        private async Task PollDeviceOnce()
        {
            if (!_isPolling || _modbusMaster == null) return;

            // Set the custom device name in the transport layer *before* polling.
            // This ensures all TX/RX logs generated during this poll cycle
            // use this device's friendly name (this.DeviceName).
            if (_modbusMaster.Transport is MP_modbus.ModbusTransportBase transport)
            {
                transport.LoggingDeviceName = this.DeviceName + " (" + this.slaveId.Value.ToString() + ")";
            }

            byte slaveId = (byte)this.SlaveId;

            foreach (TabPage tabPage in tabPanel1.TabPages)
            {
                // If the user clicked "Pause" while processing the previous tab,
                // we don't want to continue with the next ones.
                if (!_isPolling) break;

                if (tabPage.Controls[0] is not ReadingsTab readingsTab) continue;

                int funcCode = 0;
                ushort startAddr = 0;
                ushort quantity = 0;
                try
                {
                    funcCode = readingsTab.GetFunctionCode();
                    startAddr = (ushort)readingsTab.GetStartAddress();
                    quantity = (ushort)readingsTab.GetQuantity();

                    ushort[] data;
                    switch (funcCode)
                    {
                        case 0: // 01 Coils
                            bool[] coils = await _modbusMaster.ReadCoilsAsync(slaveId, startAddr, quantity);
                            data = coils.Select(c => c ? (ushort)1 : (ushort)0).ToArray();
                            break;
                        case 1: // 02 Discrete Inputs
                            bool[] inputs = await _modbusMaster.ReadInputsAsync(slaveId, startAddr, quantity);
                            data = inputs.Select(i => i ? (ushort)1 : (ushort)0).ToArray();
                            break;
                        // 03 Holding Registers (4x) is ComboBox index 2
                        case 2: // 03 Holding Registers
                            data = await _modbusMaster.ReadHoldingRegistersAsync(slaveId, startAddr, quantity);
                            break;
                        // 04 Input Registers (3x) is ComboBox index 3
                        case 3: // 04 Input Registers
                            data = await _modbusMaster.ReadInputRegistersAsync(slaveId, startAddr, quantity);
                            break;
                        default:
                            // This shouldn't happen with a ComboBox, but just in case
                            readingsTab.ShowTabError($"Unsupported Function Code Index: {funcCode}");
                            continue; // Continue to the next tab
                    }

                    readingsTab.UpdateValues(data);
                    readingsTab.ClearTabError(); // Clear the error if the read was successful
                }
                catch (MP_modbus.MyModbusSlaveException modbusEx) // Modbus Error (e.g., bad address)
                {
                    // Use ModbusUtils to get the required error format
                    string fullError = MP_modbus.ModbusUtils.GetFullExceptionMessage(modbusEx.FunctionCode, modbusEx.SlaveExceptionCode);
                    string simpleError = MP_modbus.ModbusUtils.GetExceptionName(modbusEx.SlaveExceptionCode);

                    // Display the simple error in the device tab
                    readingsTab.ShowTabError(simpleError);

                    // Log the full error in the communication window
                    LogFrame("Error", "", fullError);

                    // We don't show this error at the device level (ShowDeviceError)
                    // nor do we stop polling, as it might only affect one tab.
                }
                catch (Exception ex) // Communication Error (e.g., Timeout, disconnection)
                {
                    string commsError = $"Comms Error: {ex.Message}";

                    // Log the communication error
                    LogFrame("Error", "", commsError); // Clean error in the ErrorDescription column

                    // Show the error in the tab and at the device level
                    readingsTab.ShowTabError(commsError);
                    ShowDeviceError(commsError); // Show device-level error

                    // Safely stops the loop and updates the UI
                    this.StopPolling();

                    break; // Break the tab loop because the error affects the entire connection
                }
            } // End of foreach loop for tabs

            // If the loop finished and polling is still active, clear the device error.
            // This handles the case where the last tab had a SlaveException,
            // but previous reads were OK - we don't want to see a device-level error then.
            if (_isPolling)
            {
                ClearDeviceError();
            }
        }

        /// <summary>
        /// Logs a frame to the main communication window.
        /// </summary>
        private void LogFrame(string direction, string dataFrame, string error = "")
        {
            var logEntry = new ModbusFrameLog
            {
                Timestamp = DateTime.Now,
                DeviceName = this.DeviceName + " (" + slaveId.Value.ToString() + ")", // Add device name to the log
                Direction = direction,
                DataFrame = dataFrame,
                ErrorDescription = error
            };
            _mainWindow?.LogCommunicationEvent(logEntry);
        }

        /// <summary>
        /// Ensures polling is stopped when the form is closed.
        /// </summary>
        private void ModbusDevice_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.StopPolling();
        }
    }
}