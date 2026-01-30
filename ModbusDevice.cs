using Microsoft.Data.Sqlite;
using System.Data;

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

        // Updates chart data by gathering information from all available reading tabs
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
            this.chartToolStripMenuItem.Click += new System.EventHandler(this.chartToolStripMenuItem_Click);
        }

        private async void ModbusGroup_Load(object sender, EventArgs e)
        {
            if (this.MdiParent is MainWindow mw)
            {
                _mainWindow = mw;
            }

            lblDeviceStatus.Visible = false;
            tabNo = tabPanel1.TabPages.Count;

            // Ensure at least one tab exists on load
            if (tabPanel1.TabPages.Count == 0)
            {
                AddNewReadingsTab();
            }

            // Re-subscribe events for pre-loaded tabs from database
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

        /// <summary>
        /// Handles Modbus write requests (FC 05, 06, 16) from a readings tab.
        /// </summary>
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

                if (e.FunctionCode == 0) // Coils (FC 05)
                {
                    bool valueToWrite = false;
                    if (e.Format == ReadingsTab.DisplayFormat.Bool16)
                    {
                        if (!bool.TryParse(rawValue, out valueToWrite))
                            throw new ArgumentException($"Invalid boolean value for Bool16: {rawValue}");
                    }
                    else
                    {
                        if (rawValue.Trim() == "0") valueToWrite = false;
                        else if (rawValue.Trim() == "1") valueToWrite = true;
                        else if (int.TryParse(rawValue, out int intVal)) valueToWrite = (intVal != 0);
                        else throw new ArgumentException($"Invalid Coil value (expected True/False/0/1): {rawValue}");
                    }

                    await _modbusMaster.WriteSingleCoilAsync(slaveId, address, valueToWrite);
                    LogFrame("TX_Write", $"Write Coil {address} = {valueToWrite} (FC 05)", "Write OK");
                }
                else if (e.FunctionCode == 2) // Holding Registers (FC 06/16)
                {
                    ushort[] valuesToWrite;

                    if (regsNeeded == 1) // FC 06
                    {
                        ushort valueToWrite;
                        switch (e.Format)
                        {
                            case ReadingsTab.DisplayFormat.Unsigned16:
                                if (!ushort.TryParse(rawValue, out valueToWrite)) throw new ArgumentException("Invalid Unsigned16 value.");
                                break;
                            case ReadingsTab.DisplayFormat.Signed16:
                                if (!short.TryParse(rawValue, out short sVal)) throw new ArgumentException("Invalid Signed16 value.");
                                valueToWrite = (ushort)sVal;
                                break;
                            case ReadingsTab.DisplayFormat.Hex16:
                                string cleanHex = rawValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? rawValue.Substring(2) : rawValue;
                                if (!ushort.TryParse(cleanHex, System.Globalization.NumberStyles.HexNumber, null, out valueToWrite)) throw new ArgumentException("Invalid Hex value.");
                                break;
                            case ReadingsTab.DisplayFormat.ASCII:
                                if (rawValue.Length != 2) throw new ArgumentException("ASCII 16-bit requires exactly 2 characters.");
                                valueToWrite = (ushort)((rawValue[0] << 8) | rawValue[1]);
                                break;
                            default:
                                throw new NotSupportedException("Format not supported for 16-bit write.");
                        }

                        await _modbusMaster.WriteSingleRegisterAsync(slaveId, address, valueToWrite);
                        LogFrame("TX_Write", $"Write Register {address} = {valueToWrite} (FC 06)", "Write OK");
                    }
                    else // FC 16
                    {
                        string fmtStr = e.Format.ToString();
                        if (fmtStr.Contains("Unsigned") && regsNeeded == 2)
                        {
                            if (!uint.TryParse(rawValue, out uint val)) throw new ArgumentException("Invalid UInt32 value.");
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertValueToRegisters(val, e.Format);
                        }
                        else if (fmtStr.Contains("Signed") && regsNeeded == 2)
                        {
                            if (!int.TryParse(rawValue, out int val)) throw new ArgumentException("Invalid Int32 value.");
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertValueToRegisters(val, e.Format);
                        }
                        else if (fmtStr.Contains("Float32"))
                        {
                            if (!float.TryParse(rawValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float val))
                                throw new ArgumentException("Invalid Float32 value.");
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertValueToRegisters(val, e.Format);
                        }
                        else if (fmtStr.Contains("Unsigned") && regsNeeded == 4)
                        {
                            if (!ulong.TryParse(rawValue, out ulong val)) throw new ArgumentException("Invalid UInt64 value.");
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertValueToRegisters(val, e.Format);
                        }
                        else if (fmtStr.Contains("Signed") && regsNeeded == 4)
                        {
                            if (!long.TryParse(rawValue, out long val)) throw new ArgumentException("Invalid Int64 value.");
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertValueToRegisters(val, e.Format);
                        }
                        else if (fmtStr.Contains("Double64") || fmtStr.Contains("Float64"))
                        {
                            if (!double.TryParse(rawValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double val))
                                throw new ArgumentException("Invalid Double64 value.");
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertValueToRegisters(val, e.Format);
                        }
                        else if (fmtStr.Contains("ASCII"))
                        {
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertAsciiToRegisters(rawValue, e.Format);
                        }
                        else throw new NotSupportedException($"Write logic for {e.Format} is not supported.");

                        await _modbusMaster.WriteMultipleRegistersAsync(slaveId, address, valuesToWrite);
                        LogFrame("TX_Write", $"Write {regsNeeded * 16}-bit Reg {address} = {rawValue} ({e.Format}, FC 16)", "Write OK");
                    }
                }

                e.ReadingsTab.ClearTabError();
                await PollDeviceOnce(); // Refresh values immediately after write
            }
            catch (MP_modbus.MyModbusSlaveException modbusEx)
            {
                string error = MP_modbus.ModbusUtils.GetExceptionName(modbusEx.SlaveExceptionCode);
                e.ReadingsTab.ShowTabError($"Modbus Error: {error}");
                LogFrame("Error", "", $"Write failed: {modbusEx.Message}");
                MessageBox.Show($"Device reported error during write:\n{error}", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                e.ReadingsTab.ShowTabError($"Write error: {ex.Message}");
                LogFrame("Error", "", $"Write failed: {ex.Message}");
                MessageBox.Show($"Failed to write value.\nDetails: {ex.Message}", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (wasPolling) startToolStripMenuItem_Click(this, EventArgs.Empty);
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e) => AddNewReadingsTab();

        private void AddNewReadingsTab()
        {
            TabPage newTab = new TabPage();
            tabNo++;
            newTab.Text = "Readings " + tabNo;

            ReadingsTab readingsTab = new ReadingsTab() { Dock = DockStyle.Fill };
            readingsTab.ChartDataUpdated += ReadingsTab_ChartDataUpdated;
            readingsTab.WriteValueRequested += ReadingsTab_WriteValueRequested;

            newTab.Controls.Add(readingsTab);
            tabPanel1.TabPages.Add(newTab);
            tabPanel1.SelectedTab = newTab;
        }

        private void chartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chartTabPage == null || chartTabPage.IsDisposed)
            {
                chartControl = new ChartTab() { Dock = DockStyle.Fill };
                chartTabPage = new TabPage("Chart");
                chartTabPage.Controls.Add(chartControl);
                tabPanel1.TabPages.Add(chartTabPage);
            }
            tabPanel1.SelectedTab = chartTabPage;
            UpdateChartDataFromAllTabs();
        }

        private void UpdateChartDataFromAllTabs()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateChartDataFromAllTabs));
                return;
            }

            if (chartControl == null || chartControl.IsDisposed) return;

            var allChartData = new List<ChartDataPoint>();
            foreach (TabPage page in tabPanel1.TabPages)
            {
                if (page.Controls.Count > 0 && page.Controls[0] is ReadingsTab readingsTab)
                {
                    allChartData.AddRange(readingsTab.GetChartData());
                }
            }
            chartControl.UpdateChart(allChartData);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabPanel1.SelectedTab != null) tabPanel1.TabPages.Remove(tabPanel1.SelectedTab);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e) => PerformTabRename();

        private void tabPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tabPanel1.TabCount; i++)
                {
                    if (tabPanel1.GetTabRect(i).Contains(e.Location))
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

            using (RenameForm renameDialog = new RenameForm())
            {
                renameDialog.Text = "Rename Group";
                renameDialog.newName = tabToRename.Text;

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
            string newName;
            using (RenameForm renameDialog = new RenameForm())
            {
                renameDialog.Text = "Save Device";
                renameDialog.newName = this.Text;
                if (renameDialog.ShowDialog() == DialogResult.OK) newName = renameDialog.newName;
                else return;
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Please provide a device name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.Text = newName;
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

            if (_isPolling) return;
            _isPolling = true;

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;

            await PollDeviceLoop();
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
            if (_isPolling == true)
            { _wasPolling = true; }
            else
            { _wasPolling = false; }

            _isPolling = false;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { startToolStripMenuItem.Enabled = true; stopToolStripMenuItem.Enabled = false; }));
            }
            else
            {
                if (form == 1)
                {
                    startToolStripMenuItem.Enabled = true;
                    stopToolStripMenuItem.Enabled = false;
                }
                else
                {
                    startToolStripMenuItem.Enabled = false;
                    stopToolStripMenuItem.Enabled = false;
                }
            }


        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e) => StopPolling(1);

        private void ShowDeviceError(string message)
        {
            if (InvokeRequired) { Invoke(new Action(() => ShowDeviceError(message))); return; }
            lblDeviceStatus.Visible = true;
            lblDeviceStatus.Text = $"Device Error: {message}";
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
                if (tabPage.Controls.Count == 0 || tabPage.Controls[0] is not ReadingsTab readingsTab || tabPage == chartTabPage) continue;

                try
                {
                    int func = readingsTab.GetFunctionCode();
                    ushort addr = (ushort)readingsTab.GetStartAddress();
                    ushort qty = (ushort)readingsTab.GetQuantity();
                    _txCounter++;
                    ushort[] data;

                    switch (func)
                    {
                        case 0: // Coils
                            data = (await _modbusMaster.ReadCoilsAsync(sId, addr, qty)).Select(c => c ? (ushort)1 : (ushort)0).ToArray();
                            break;
                        case 1: // Discrete Inputs
                            data = (await _modbusMaster.ReadInputsAsync(sId, addr, qty)).Select(i => i ? (ushort)1 : (ushort)0).ToArray();
                            break;
                        case 2: // Holding Registers
                            data = await _modbusMaster.ReadHoldingRegistersAsync(sId, addr, qty);
                            break;
                        case 3: // Input Registers
                            data = await _modbusMaster.ReadInputRegistersAsync(sId, addr, qty);
                            break;
                        default: continue;
                    }

                    _rxCounter++;
                    readingsTab.UpdateValues(data);
                    readingsTab.ClearTabError();
                    _consecutiveErrorCount = 0;
                }
                catch (MP_modbus.MyModbusSlaveException modbusEx)
                {
                    string fullError = MP_modbus.ModbusUtils.GetFullExceptionMessage(modbusEx.FunctionCode, modbusEx.SlaveExceptionCode);
                    _errorCounter++;
                    readingsTab.ShowTabError(MP_modbus.ModbusUtils.GetExceptionName(modbusEx.SlaveExceptionCode));
                    readingsTab.ClearDisplayValues();
                    LogFrame("Error", "", fullError);
                }
                catch (Exception ex)
                {
                    _errorCounter++;
                    _consecutiveErrorCount++;
                    LogFrame("Error", "", $"Comms Error: {ex.Message}");
                    readingsTab.ShowTabError($"Comms Error: {ex.Message} ({_consecutiveErrorCount})");

                    if (_consecutiveErrorCount >= maxRetries && maxRetries > 0)
                    {
                        ShowDeviceError($"Polling stopped after {maxRetries} failures.");
                        StopPolling(1);
                    }
                    else ShowDeviceError($"Comms Error (Attempt {_consecutiveErrorCount}/{(maxRetries > 0 ? maxRetries.ToString() : "inf")})");
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
            else
            {
                startToolStripMenuItem.Enabled = true;
            }
        }

        private void ModbusDevice_FormClosed(object sender, FormClosedEventArgs e) => StopPolling(1);
        private void ModbusDevice_FormClosing(object sender, FormClosingEventArgs e) => StopPolling(1);
    }
}