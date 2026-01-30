using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP_ModbusApp
{
    public partial class ModbusDevice : Form
    {
        private MainWindow _mainWindow;
        private MP_modbus.IMyModbusMaster _modbusMaster;
        private bool _isPolling = false;
        private bool _wasPolling = false;
        private int _consecutiveErrorCount = 0;
        private int _rxCounter = 0;
        private int _txCounter = 0;
        private int _errorCounter = 0;
        public event EventHandler DeviceSaved;

        private TabPage chartTabPage = null;
        private ChartTab chartControl = null;

        // Właściwość publiczna, aby sprawdzić, czy urządzenie jest zajęte
        public bool IsPolling => _isPolling;

        public void ReadingsTab_ChartDataUpdated(object sender, ChartDataUpdateEventArgs e)
        {
            UpdateChartDataFromAllTabs();
        }

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

            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);

            // Podpięcie zdarzenia zmiany ID (z poprzedniego rozwiązania)
            this.slaveId.ValueChanged += new EventHandler(this.slaveId_ValueChanged);
        }

        private void slaveId_ValueChanged(object sender, EventArgs e)
        {
            if (_modbusMaster?.Transport is MP_modbus.MyModbusTcpTransport tcpTransport)
            {
                tcpTransport.ResetTransactionId();
            }
        }

        private async void ModbusGroup_Load(object sender, EventArgs e)
        {
            if (this.MdiParent is MainWindow mw)
            {
                _mainWindow = mw;
            }

            lblDeviceStatus.Visible = false;
            tabNo = tabPanel1.TabPages.Count;

            // ZMIANA: Zawsze upewnij się, że zakładka wykresu istnieje i jest na końcu
            EnsureChartTabExists();

            // Upewnij się, że istnieje przynajmniej jedna zakładka z odczytami
            // Sprawdzamy czy mamy tylko wykres (count == 1 i to jest wykres), czy pusto
            bool hasReadingsTab = false;
            foreach (TabPage page in tabPanel1.TabPages)
            {
                if (page != chartTabPage) { hasReadingsTab = true; break; }
            }

            if (!hasReadingsTab)
            {
                AddNewReadingsTab();
            }

            // Ponowne podpięcie zdarzeń dla załadowanych zakładek
            foreach (TabPage page in tabPanel1.TabPages)
            {
                if (page.Controls.Count > 0 && page.Controls[0] is ReadingsTab tab)
                {
                    tab.ChartDataUpdated += ReadingsTab_ChartDataUpdated;
                    tab.WriteValueRequested += ReadingsTab_WriteValueRequested;
                }
            }

            await Task.Delay(100);
            startToolStripMenuItem_Click(sender, e);
        }

        // ZMIANA: Nowa metoda pomocnicza do tworzenia zakładki wykresu
        private void EnsureChartTabExists()
        {
            if (chartTabPage == null || chartTabPage.IsDisposed)
            {
                chartControl = new ChartTab() { Dock = DockStyle.Fill };
                chartTabPage = new TabPage("Chart"); // Lub "Wykres"
                chartTabPage.Controls.Add(chartControl);
            }

            // Jeśli zakładki nie ma w kolekcji, dodaj ją na sam koniec
            if (!tabPanel1.TabPages.Contains(chartTabPage))
            {
                tabPanel1.TabPages.Add(chartTabPage);
            }
            else
            {
                // Jeśli jest, ale nie na końcu, przesuń ją na koniec
                if (tabPanel1.TabPages.IndexOf(chartTabPage) != tabPanel1.TabPages.Count - 1)
                {
                    tabPanel1.TabPages.Remove(chartTabPage);
                    tabPanel1.TabPages.Add(chartTabPage);
                }
            }
        }

        public async void ReadingsTab_WriteValueRequested(object sender, WriteRequestedEventArgs e)
        {
            if (_modbusMaster == null)
            {
                ShowDeviceError("Port not connected! Cannot perform write operation.");
                e.ReadingsTab.ShowTabError("No connection.");
                return;
            }

            bool wasPolling = _isPolling;
            StopPolling(1);

            if (_modbusMaster.Transport is MP_modbus.ModbusTransportBase transport)
            {
                transport.LoggingDeviceName = $"{this.DeviceName} ({this.slaveId.Value})";
            }

            try
            {
                byte slaveId = (byte)this.SlaveId;
                ushort address = e.StartAddress;
                string rawValue = e.ValueString;
                int regsNeeded = e.ReadingsTab.GetRegistersForFormat(e.Format);

                if (e.FunctionCode == 0) // Coils
                {
                    bool valueToWrite = false;
                    if (e.Format == ReadingsTab.DisplayFormat.Bool16) { bool.TryParse(rawValue, out valueToWrite); }
                    else { if (rawValue.Trim() == "1") valueToWrite = true; else if (rawValue.Trim() == "0") valueToWrite = false; else bool.TryParse(rawValue, out valueToWrite); }
                    await _modbusMaster.WriteSingleCoilAsync(slaveId, address, valueToWrite);
                }
                else if (e.FunctionCode == 2) // Registers
                {
                    if (ushort.TryParse(rawValue, out ushort val)) await _modbusMaster.WriteSingleRegisterAsync(slaveId, address, val);
                    else await _modbusMaster.WriteMultipleRegistersAsync(slaveId, address, new ushort[] { 0 });
                }

                e.ReadingsTab.ClearTabError();
                await PollDeviceOnce();
            }
            catch (Exception ex)
            {
                e.ReadingsTab.ShowTabError($"Write error: {ex.Message}");
                MessageBox.Show($"Write failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (wasPolling) startToolStripMenuItem_Click(this, EventArgs.Empty);
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e) => AddNewReadingsTab();

        // ZMIANA: Logika wstawiania nowej zakładki PRZED wykresem
        private void AddNewReadingsTab()
        {
            TabPage newTab = new TabPage();
            tabNo++;
            newTab.Text = "Readings " + tabNo;

            ReadingsTab readingsTab = new ReadingsTab() { Dock = DockStyle.Fill };
            readingsTab.ChartDataUpdated += ReadingsTab_ChartDataUpdated;
            readingsTab.WriteValueRequested += ReadingsTab_WriteValueRequested;

            newTab.Controls.Add(readingsTab);

            // Sprawdź, czy mamy zakładkę wykresu
            if (chartTabPage != null && tabPanel1.TabPages.Contains(chartTabPage))
            {
                // Wstaw nową zakładkę przed zakładką wykresu
                int chartIndex = tabPanel1.TabPages.IndexOf(chartTabPage);
                tabPanel1.TabPages.Insert(chartIndex, newTab);
            }
            else
            {
                // Jeśli wykresu nie ma (co nie powinno się zdarzyć przy nowej logice), dodaj na koniec
                tabPanel1.TabPages.Add(newTab);
            }

            tabPanel1.SelectedTab = newTab;
        }

        private void chartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Ponieważ zakładka jest teraz zawsze widoczna, ta metoda tylko ją aktywuje
            EnsureChartTabExists();
            tabPanel1.SelectedTab = chartTabPage;
            UpdateChartDataFromAllTabs();
        }

        private void UpdateChartDataFromAllTabs()
        {
            if (InvokeRequired) { Invoke(new Action(UpdateChartDataFromAllTabs)); return; }

            // Jeśli kontrolka wykresu nie istnieje, nie ma co aktualizować
            if (chartControl == null || chartControl.IsDisposed) return;

            var allChartData = new List<ChartDataPoint>();
            foreach (TabPage page in tabPanel1.TabPages)
            {
                // Pomijamy samą zakładkę wykresu przy zbieraniu danych
                if (page == chartTabPage) continue;

                if (page.Controls.Count > 0 && page.Controls[0] is ReadingsTab readingsTab)
                {
                    allChartData.AddRange(readingsTab.GetChartData());
                }
            }
            chartControl.UpdateChart(allChartData);
        }

        // ZMIANA: Blokada usuwania zakładki wykresu
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabPanel1.SelectedTab != null)
            {
                if (tabPanel1.SelectedTab == chartTabPage)
                {
                    MessageBox.Show("Cannot remove the Chart tab.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                tabPanel1.TabPages.Remove(tabPanel1.SelectedTab);
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e) => PerformTabRename();

        private void tabPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tabPanel1.TabCount; i++)
                {
                    if (tabPanel1.GetTabRect(i).Contains(e.Location) && i < tabPanel1.TabCount-1)
                    {
                        tabPanel1.SelectedIndex = i;
                        contextMenuStrip1.Show(tabPanel1, e.Location);
                        break;
                    }
                }
            }
        }

        private void PerformTabRename()
        {
            tabToRename = tabPanel1.SelectedTab;
            if (tabToRename == null) return;
            // Opcjonalnie: blokada zmiany nazwy zakładki Chart
            if (tabToRename == chartTabPage) return;

            using (RenameForm renameDialog = new RenameForm() { Text = "Rename Group", newName = tabToRename.Text })
            {
                if (renameDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(renameDialog.newName))
                {
                    tabToRename.Text = renameDialog.newName;
                }
            }
            tabToRename = null;
        }

        private void ModbusDevice_Resize(object sender, EventArgs e) { }

        private void newReadingsToolStripMenuItem_Click(object sender, EventArgs e) => AddNewReadingsTab();

        private void tabPanel1_DoubleClick(object sender, EventArgs e) => PerformTabRename();

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (RenameForm renameDialog = new RenameForm() { Text = "Save Device", newName = this.Text })
            {
                if (renameDialog.ShowDialog() == DialogResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(renameDialog.newName))
                    {
                        MessageBox.Show("Please provide a device name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    this.Text = renameDialog.newName;
                    try
                    {
                        SaveDeviceConfiguration();
                        MessageBox.Show("Configuration saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DeviceSaved?.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving device: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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

                    object result = checkCmd.ExecuteScalar();
                    if (result != null)
                    {
                        deviceId = (long)result;
                        var delCmd = connection.CreateCommand();
                        delCmd.Transaction = transaction;
                        delCmd.CommandText = "DELETE FROM ReadingGroups WHERE DeviceId = $deviceId;";
                        delCmd.Parameters.AddWithValue("$deviceId", deviceId);
                        delCmd.ExecuteNonQuery();

                        var updCmd = connection.CreateCommand();
                        updCmd.Transaction = transaction;
                        updCmd.CommandText = "UPDATE Devices SET SlaveId = $slaveId WHERE DeviceId = $deviceId;";
                        updCmd.Parameters.AddWithValue("$slaveId", (int)slaveId.Value);
                        updCmd.Parameters.AddWithValue("$deviceId", deviceId);
                        updCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var insCmd = connection.CreateCommand();
                        insCmd.Transaction = transaction;
                        insCmd.CommandText = "INSERT INTO Devices (DeviceName, SlaveId) VALUES ($name, $slaveId) RETURNING DeviceId;";
                        insCmd.Parameters.AddWithValue("$name", this.Text);
                        insCmd.Parameters.AddWithValue("$slaveId", (int)slaveId.Value);
                        deviceId = (long)insCmd.ExecuteScalar();
                    }

                    foreach (TabPage tabPage in tabPanel1.TabPages)
                    {
                        // ZMIANA: Nie zapisuj zakładki wykresu do bazy danych (ona jest tworzona dynamicznie)
                        if (tabPage == chartTabPage) continue;

                        if (tabPage.Controls.Count > 0 && tabPage.Controls[0] is ReadingsTab readingsTab)
                        {
                            var groupCmd = connection.CreateCommand();
                            groupCmd.Transaction = transaction;
                            groupCmd.CommandText = @"INSERT INTO ReadingGroups (DeviceId, GroupName, FunctionCode, StartAddress, Quantity) 
                                                     VALUES ($deviceId, $groupName, $funcCode, $startAddr, $quantity) RETURNING GroupId;";

                            groupCmd.Parameters.AddWithValue("$deviceId", deviceId);
                            groupCmd.Parameters.AddWithValue("$groupName", tabPage.Text);
                            groupCmd.Parameters.AddWithValue("$funcCode", readingsTab.GetFunctionCode());
                            groupCmd.Parameters.AddWithValue("$startAddr", readingsTab.GetStartAddress());
                            groupCmd.Parameters.AddWithValue("$quantity", readingsTab.GetQuantity());
                            long groupId = (long)groupCmd.ExecuteScalar();

                            int baseAddr = readingsTab.GetStartAddress();
                            foreach (DataGridViewRow row in readingsTab.GetDataGridViewRows())
                            {
                                if (row.IsNewRow) continue;
                                var regCmd = connection.CreateCommand();
                                regCmd.Transaction = transaction;
                                regCmd.CommandText = "INSERT INTO RegisterDefinitions (GroupId, RegisterNumber, RegisterName,RegisterDescription, DisplayFormatColumn) VALUES ($gId, $rNum, $rName, $rDescription, $fmt);";
                                regCmd.Parameters.AddWithValue("$gId", groupId);
                                regCmd.Parameters.AddWithValue("$rNum", baseAddr + row.Index);
                                regCmd.Parameters.AddWithValue("$rName", row.Cells["Name"].Value ?? string.Empty);
                                regCmd.Parameters.AddWithValue("$rDescription", row.Cells["Description"].Value ?? string.Empty);
                                regCmd.Parameters.AddWithValue("$fmt", row.Cells["DisplayFormatColumn"].Value?.ToString() ?? "Unsigned16");
                                regCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    transaction.Commit();
                }
            }
        }

        private async void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _modbusMaster = _mainWindow?.ModbusMaster;
            if (_modbusMaster == null)
            {
                ShowDeviceError("Port not connected! Configure connection in Setup.");
                return;
            }

            if (IsAnotherDevicePollingSameId())
            {
                ShowDeviceError($"Slave ID {SlaveId} is busy in another window.");
                return;
            }

            if (_modbusMaster.Transport is MP_modbus.MyModbusTcpTransport tcpTransport)
            {
                tcpTransport.ResetTransactionId();
            }

            if (_isPolling) return;
            _isPolling = true;

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;

            await PollDeviceLoop();
        }

        private bool IsAnotherDevicePollingSameId()
        {
            if (_mainWindow == null) return false;
            foreach (var form in _mainWindow.MdiChildren)
            {
                if (form is ModbusDevice otherDev && form != this)
                {
                    if (otherDev.SlaveId == this.SlaveId && otherDev.IsPolling) return true;
                }
            }
            return false;
        }

        private async Task PollDeviceLoop()
        {
            while (_isPolling)
            {
                await PollDeviceOnce();
                if (!_isPolling) break;
                await Task.Delay(_mainWindow.GetPollDelay());
            }
        }

        public void StopPolling(int form)
        {
            if (_isPolling) _wasPolling = true; else _wasPolling = false;
            _isPolling = false;
            Action updateUi = () => { startToolStripMenuItem.Enabled = true; stopToolStripMenuItem.Enabled = false; };
            if (InvokeRequired) Invoke(updateUi); else updateUi();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e) => StopPolling(1);

        private void ShowDeviceError(string message)
        {
            if (InvokeRequired) { Invoke(new Action(() => ShowDeviceError(message))); return; }
            lblDeviceStatus.Visible = true;
            lblDeviceStatus.Text = message;
            lblDeviceStatus.ForeColor = Color.Red;
        }

        private void ClearDeviceError()
        {
            if (InvokeRequired) { Invoke(new Action(ClearDeviceError)); return; }
            lblDeviceStatus.Visible = true;
            lblDeviceStatus.Text = _isPolling ? " " : "Polling Paused";
            lblDeviceStatus.ForeColor = _isPolling ? Color.Green : Color.Black;
        }

        private async Task PollDeviceOnce()
        {
            if (!_isPolling || _modbusMaster == null || this.WindowState == FormWindowState.Minimized) return;

            if (_modbusMaster.Transport is MP_modbus.ModbusTransportBase transport)
            {
                transport.LoggingDeviceName = $"{this.DeviceName} ({this.slaveId.Value})";
            }

            byte sId = (byte)this.SlaveId;
            int maxRetries = _mainWindow?.GetMaxRetries() ?? 0;

            foreach (TabPage tabPage in tabPanel1.TabPages)
            {
                if (!_isPolling) break;
                // ZMIANA: Pomiń zakładkę wykresu przy odpytywaniu (ona nie ma rejestrów)
                if (tabPage == chartTabPage) continue;
                if (tabPage.Controls.Count == 0 || tabPage.Controls[0] is not ReadingsTab readingsTab) continue;

                try
                {
                    int func = readingsTab.GetFunctionCode();
                    ushort addr = (ushort)readingsTab.GetStartAddress();
                    ushort qty = (ushort)readingsTab.GetQuantity();
                    _txCounter++;
                    ushort[] data;

                    switch (func)
                    {
                        case 0: data = (await _modbusMaster.ReadCoilsAsync(sId, addr, qty)).Select(c => c ? (ushort)1 : (ushort)0).ToArray(); break;
                        case 1: data = (await _modbusMaster.ReadInputsAsync(sId, addr, qty)).Select(i => i ? (ushort)1 : (ushort)0).ToArray(); break;
                        case 2: data = await _modbusMaster.ReadHoldingRegistersAsync(sId, addr, qty); break;
                        case 3: data = await _modbusMaster.ReadInputRegistersAsync(sId, addr, qty); break;
                        default: continue;
                    }

                    _rxCounter++;
                    readingsTab.UpdateValues(data);
                    readingsTab.ClearTabError();
                    _consecutiveErrorCount = 0;
                }
                catch (Exception ex)
                {
                    _errorCounter++;
                    _consecutiveErrorCount++;
                    LogFrame("Error", "", $"Comms Error: {ex.Message}");
                    readingsTab.ShowTabError($"Comms Error: {ex.Message}");

                    if (maxRetries > 0 && _consecutiveErrorCount >= maxRetries)
                    {
                        ShowDeviceError("Stopped due to excessive errors.");
                        StopPolling(1);
                    }
                    else ShowDeviceError($"Error (Attempt {_consecutiveErrorCount})");
                    break;
                }
                label1.Text = $"TX: {_txCounter}  RX: {_rxCounter}  ERR: {_errorCounter}";
            }
            if (_isPolling) ClearDeviceError();
        }

        private void LogFrame(string direction, string dataFrame, string error = "")
        {
            _mainWindow?.LogCommunicationEvent(new ModbusFrameLog
            {
                Timestamp = DateTime.Now,
                DeviceName = $"{this.DeviceName} ({slaveId.Value})",
                Direction = direction,
                DataFrame = dataFrame,
                ErrorDescription = error
            });
        }

        public void StartPolling()
        {
            if (_wasPolling)
            {
                _wasPolling = false;
                startToolStripMenuItem.Enabled = true;
                startToolStripMenuItem.PerformClick();
            }
            else startToolStripMenuItem.Enabled = true;
        }

        private void ModbusDevice_FormClosed(object sender, FormClosedEventArgs e) => StopPolling(1);
        private void ModbusDevice_FormClosing(object sender, FormClosingEventArgs e) => StopPolling(1);
    }
}