using Microsoft.Data.Sqlite;
using NModbus;
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
        private IModbusMaster _modbusMaster;
        private System.Threading.Timer _pollingTimer;
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
        }

        private void ModbusGroup_Load(object sender, EventArgs e)
        {
            if (this.MdiParent is MainWindow mw)
            {
                _mainWindow = mw;
            }

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
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage newTab = new TabPage();
            tabNo = tabNo + 1;
            tabPanel1.SelectedTab = newTab;
            newTab.Text = "Readings " + tabNo;
            newTab.Controls.Add(new ReadingsTab() { Dock = DockStyle.Fill });
            tabPanel1.TabPages.Add(newTab);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabPanel1.SelectedTab != null)
            {
                tabPanel1.TabPages.Remove(tabPanel1.SelectedTab);
            }
        }

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
                        break;
                    }
                }
            }
        }

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

        private void newReadingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage newTab = new TabPage();
            tabNo = tabNo + 1;
            tabPanel1.SelectedTab = newTab;
            newTab.Text = "Readings " + tabNo;
            newTab.Controls.Add(new ReadingsTab() { Dock = DockStyle.Fill });
            tabPanel1.TabPages.Add(newTab);
        }

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
                    return;
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


        private void SaveDeviceConfiguration()
        {
            using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    long deviceId;

                    var checkCmd = connection.CreateCommand();
                    checkCmd.Transaction = transaction;
                    checkCmd.CommandText = "SELECT DeviceId FROM Devices WHERE DeviceName = $name;";
                    checkCmd.Parameters.AddWithValue("$name", this.Text);

                    object existingIdResult = checkCmd.ExecuteScalar();

                    if (existingIdResult != null)
                    {
                        deviceId = (long)existingIdResult;

                        var deleteGroupsCmd = connection.CreateCommand();
                        deleteGroupsCmd.Transaction = transaction;
                        deleteGroupsCmd.CommandText = "DELETE FROM ReadingGroups WHERE DeviceId = $deviceId;";
                        deleteGroupsCmd.Parameters.AddWithValue("$deviceId", deviceId);
                        deleteGroupsCmd.ExecuteNonQuery();

                        var updateDeviceCmd = connection.CreateCommand();
                        updateDeviceCmd.Transaction = transaction;
                        updateDeviceCmd.CommandText = "UPDATE Devices SET SlaveId = $slaveId WHERE DeviceId = $deviceId;";
                        updateDeviceCmd.Parameters.AddWithValue("$slaveId", (int)slaveId.Value);
                        updateDeviceCmd.Parameters.AddWithValue("$deviceId", deviceId);
                        updateDeviceCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var insertDeviceCmd = connection.CreateCommand();
                        insertDeviceCmd.Transaction = transaction;
                        insertDeviceCmd.CommandText = "INSERT INTO Devices (DeviceName, SlaveId) VALUES ($name, $slaveId) RETURNING DeviceId;";
                        insertDeviceCmd.Parameters.AddWithValue("$name", this.Text);
                        insertDeviceCmd.Parameters.AddWithValue("$slaveId", (int)slaveId.Value);

                        deviceId = (long)insertDeviceCmd.ExecuteScalar();
                    }

                    foreach (TabPage tabPage in tabPanel1.TabPages)
                    {
                        if (tabPage.Controls[0] is ReadingsTab readingsTab)
                        {
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

                            foreach (DataGridViewRow row in readingsTab.GetDataGridViewRows())
                            {
                                if (row.IsNewRow) continue;
                                var regCmd = connection.CreateCommand();
                                regCmd.Transaction = transaction;
                                regCmd.CommandText = @"
                            INSERT INTO RegisterDefinitions (GroupId, RegisterNumber, RegisterName)
                            VALUES ($groupId, $regNum, $regName);";

                                regCmd.Parameters.AddWithValue("$groupId", groupId);
                                regCmd.Parameters.AddWithValue("$regNum", row.Cells["RegisterNumber"].Value ?? 0);
                                regCmd.Parameters.AddWithValue("$regName", row.Cells["Name"].Value ?? string.Empty);
                                regCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();
                }
            }
        }
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _modbusMaster = _mainWindow?.ModbusMaster;
            if (_modbusMaster == null)
            {
                ShowDeviceError("Not connected in MainWindow!");
                return;
            }

            if (_isPolling) return;
            _isPolling = true;

            // Użyj Timera do cyklicznego wywoływania
            int pollDelay = _mainWindow.GetPollDelay(); // Potrzebujesz dodać tę metodę w MainWindow
            _pollingTimer = new System.Threading.Timer(PollDevice, null, 0, pollDelay);

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _isPolling = false;
            _pollingTimer?.Dispose();

            startToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem.Enabled = false;
        }
        private void ShowDeviceError(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowDeviceError(message)));
                return;
            }
            lblDeviceStatus.Text = $"Device Error: {message}";
            lblDeviceStatus.ForeColor = Color.Red;
        }

        private void ClearDeviceError()
        {
            if (InvokeRequired) { /* ... */ }
            lblDeviceStatus.Text = "Polling OK";
            lblDeviceStatus.ForeColor = Color.Black;
        }


        private async void PollDevice(object state)
        {
            if (!_isPolling || _modbusMaster == null) return;

            byte slaveId = (byte)this.SlaveId;

            foreach (TabPage tabPage in tabPanel1.TabPages)
            {
                if (tabPage.Controls[0] is not ReadingsTab readingsTab) continue;

                try
                {
                    int funcCode = readingsTab.GetFunctionCode();
                    ushort startAddr = (ushort)readingsTab.GetStartAddress();
                    ushort quantity = (ushort)readingsTab.GetQuantity();

                    // Tu następuje właściwy odczyt Modbus
                    ushort[] data;
                    switch (funcCode)
                    {
                        // NModbus używa indeksów 1, 2, 3, 4, a Twój ComboBox 0, 1, 2, 3
                        case 0: // 01 Coils
                            bool[] coils = await _modbusMaster.ReadCoilsAsync(slaveId, startAddr, quantity);
                            data = coils.Select(c => c ? (ushort)1 : (ushort)0).ToArray();
                            break;
                        case 1: // 02 Discrete Inputs
                            bool[] inputs = await _modbusMaster.ReadInputsAsync(slaveId, startAddr, quantity);
                            data = inputs.Select(i => i ? (ushort)1 : (ushort)0).ToArray();
                            break;
                        case 2: // 03 Holding Registers
                            data = await _modbusMaster.ReadHoldingRegistersAsync(slaveId, startAddr, quantity);
                            break;
                        case 3: // 04 Input Registers
                            data = await _modbusMaster.ReadInputRegistersAsync(slaveId, startAddr, quantity);
                            break;
                        default:
                            throw new Exception("Unknown function code");
                    }

                    // Sukces: Zaktualizuj DataGridView (musisz to zaimplementować)
                    // readingsTab.UpdateValues(data); 

                    // Wyczyść błędy (dla zakładki i urządzenia)
                    readingsTab.ClearTabError();
                    ClearDeviceError();
                }
                catch (NModbus.SlaveException modbusEx) // Błąd Modbus (np. zły adres)
                {
                    // BŁĄD NA POZIOMIE ZAKŁADKI
                    readingsTab.ShowTabError($"Modbus Error: {modbusEx.Message}");
                    LogToGlobal(tabPage.Text, modbusEx.Message);
                }
                catch (Exception ex) // Błąd Komunikacji (np. Timeout)
                {
                    // BŁĄD NA POZIOMIE URZĄDZENIA I ZAKŁADKI
                    readingsTab.ShowTabError($"Comms Error: {ex.Message}");
                    ShowDeviceError(ex.Message); // Pokaż też na oknie urządzenia
                    LogToGlobal(tabPage.Text, ex.Message);

                    // Jeśli jest błąd komunikacji, przerwij odpytywanie tego urządzenia
                    stopToolStripMenuItem_Click(null, null);
                    break;
                }
            }
        }

        private void LogToGlobal(string tabName, string errorMessage)
        {
            var logEntry = new ModbusFrameLog
            {
                Timestamp = DateTime.Now,
                Direction = "Error",
                TransactionID = 0, // NModbus może nie udostępniać tego łatwo
                DataFrame = $"Error in device '{this.Text}', tab '{tabName}'",
                ErrorDescription = errorMessage
            };
            _mainWindow?.LogCommunicationEvent(logEntry);
        }

    }
}
