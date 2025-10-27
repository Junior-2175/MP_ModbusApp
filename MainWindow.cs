using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP_ModbusApp
{
    public partial class MainWindow : Form
    {
        private SerialPort serialPort;
        private System.Net.Sockets.TcpClient tcpClient;
        private ModbusService modbusService;
        private CommunicationLogWindow _commsLogWindow;

        bool sidePanelHidden = false;
        private readonly ToolTip toolTip1 = new ToolTip();

        public MainWindow()
        {
            InitializeComponent();
            toolTip1.SetToolTip(openMenu, "Show / Hide connection setting");
            toolTip1.SetToolTip(openTree, "Show / Hide devices tree");
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            sidePanel.Width = 21;
            setupButton.Height = 90;
            openMenu.Text = "Setup";
            openMenu.Font = new Font("Segoe UI", 8F);
            openMenu.Height = 80;
            openMenu.Width = 15;
            openMenu.Left = 3;
            treeButton.Height = 110;
            openTree.Text = "Devices x";
            openTree.Font = new Font("Segoe UI", 8F);
            openTree.Height = 100;
            openTree.Width = 15;
            openTree.Left = 3;
            setupPanel.Visible = false;
            treeView.Visible = false;
            sidePanelHidden = true;
            this.Refresh();
            cboxComPort.DrawMode = DrawMode.OwnerDrawFixed;
            gboxIPSettings.Visible = false;


            PopulateComboBoxes();
            RefreshComPorts();
            LoadSettings();
            LoadDevicesToTree();

            cboxComPort.DrawItem += CboxComPort_DrawItem;
            cboxComPort.DropDownClosed += CboxComPort_DropDownClosed;
            cboxComPort.SelectedIndexChanged += cboxComPort_SelectedIndexChanged;
            UpdateUiState(false);

        }

        private void openMenu_Click(object sender, EventArgs e)
        {
            
            
            if (sidePanelHidden)
            {
                sidePanel.Width = 350;
                setupButton.Height = 50;
                openMenu.Text = "↩";
                openMenu.Font = new Font("Segoe UI", 14.25F);
                openMenu.Height = 40;
                openMenu.Width = 40;
                openMenu.Left = 300;
                openTree.Text = "↓";
                openTree.Font = new Font("Segoe UI", 14.25F);
                openTree.Height = 40;
                openTree.Width = 40;
                openTree.Left = 300;
                setupPanel.Visible = true;
                treeView.Visible = false;
                sidePanelHidden = false;
            }
            else
            {
                if (!setupPanel.Visible)
                {
                    sidePanel.Width = 350;
                    setupButton.Height = 50;
                    openMenu.Text = "↩";
                    openMenu.Font = new Font("Segoe UI", 14.25F);
                    openMenu.Height = 40;
                    openMenu.Width = 40;
                    openMenu.Left = 300;
                    openTree.Text = "↓";
                    openTree.Font = new Font("Segoe UI", 14.25F);
                    openTree.Height = 40;
                    openTree.Width = 40;
                    openTree.Left = 300;
                    setupPanel.Visible = true;
                    treeView.Visible = false;
                    sidePanelHidden = false;
                }
                else
                {
                    sidePanel.Width = 21;
                    setupButton.Height = 90;
                    openMenu.Text = "Setup";
                    openMenu.Font = new Font("Segoe UI", 8F);
                    openMenu.Height = 80;
                    openMenu.Width = 15;
                    openMenu.Left = 3;
                    treeButton.Height = 110;
                    openTree.Text = "Devices x";
                    openTree.Font = new Font("Segoe UI", 8F);
                    openTree.Height = 100;
                    openTree.Width = 15;
                    openTree.Left = 3;
                    setupPanel.Visible = false;
                    treeView.Visible = false;
                    sidePanelHidden = true;
                }
            }
            this.Refresh();
        }

        private void openTree_Click(object sender, EventArgs e)
        {
            if (sidePanelHidden)
            {
                sidePanel.Width = 350;
                setupButton.Height = 50;
                openMenu.Text = "↓";
                openMenu.Font = new Font("Segoe UI", 14.25F);
                openMenu.Height = 40;
                openMenu.Width = 40;
                openMenu.Left = 300;
                treeButton.Height = 50;
                openTree.Text = "↩";
                openTree.Font = new Font("Segoe UI", 14.25F);
                openTree.Height = 40;
                openTree.Width = 40;
                openTree.Left = 300;
                setupPanel.Visible = false;
                treeView.Visible = true;
                sidePanelHidden = false;
            }
            else
            {
                if (!treeView.Visible)
                {
                    sidePanel.Width = 350;
                    setupButton.Height = 50;
                    openMenu.Text = "↓";
                    openMenu.Font = new Font("Segoe UI", 14.25F);
                    openMenu.Height = 40;
                    openMenu.Width = 40;
                    openMenu.Left = 300;
                    treeButton.Height = 50;
                    openTree.Text = "↩";
                    openTree.Font = new Font("Segoe UI", 14.25F);
                    openTree.Height = 40;
                    openTree.Width = 40;
                    openTree.Left = 300;
                    this.Refresh();
                    setupPanel.Visible = false;
                    treeView.Visible = true;
                    sidePanelHidden = false;
                }
                else
                {
                    sidePanel.Width = 21;
                    setupButton.Height = 90;
                    openMenu.Text = "Setup";
                    openMenu.Font = new Font("Segoe UI", 8F);
                    openMenu.Height = 80;
                    openMenu.Width = 15;
                    openMenu.Left = 3;
                    treeButton.Height = 110;
                    openTree.Text = "Devices x";
                    openTree.Font = new Font("Segoe UI", 8F);
                    openTree.Height = 100;
                    openTree.Width = 15;
                    openTree.Left = 3;
                    this.Refresh();
                    setupPanel.Visible = false;
                    treeView.Visible = false;
                    sidePanelHidden = true;
                }
            }
            this.Refresh();
        }

        #region Inicjalizacja i ładowanie ustawień
        private void PopulateComboBoxes()
        {
            cboxConnection.Items.AddRange(new object[] { "Serial Port (RTU/ASCII)", "Modbus TCP/IP", "Modbus RTU/ASCII over TCP/IP" });
            cBoxBaudRate.Items.AddRange(new object[] { "300", "600", "1200", "2400", "4800", "9600", "14400", "19200", "38400", "56000", "57600", "115200" });
            cBoxDataBits.Items.AddRange(new object[] { "7", "8" });
            var parityOptions = Enum.GetNames(typeof(Parity)).Except(new[] { "Mark", "Space" }).ToArray();
            cBoxParity.Items.AddRange(parityOptions);
            var stopBitsOptions = Enum.GetNames(typeof(StopBits)).Except(new[] { "None", "OnePointFive" }).ToArray();
            cBoxStopBits.Items.AddRange(stopBitsOptions);
        }

        private void LoadSettings()
        {
            cboxConnection.SelectedIndex = int.Parse(DatabaseHelper.LoadSetting("ConnectionType", "0"));
            cBoxBaudRate.SelectedIndex = int.Parse(DatabaseHelper.LoadSetting("cBoxBaudRate", "5"));
            cBoxDataBits.SelectedIndex = int.Parse(DatabaseHelper.LoadSetting("cBoxDataBits", "1"));
            cBoxParity.SelectedIndex = int.Parse(DatabaseHelper.LoadSetting("cBoxParity", "0"));
            cBoxStopBits.SelectedIndex = int.Parse(DatabaseHelper.LoadSetting("cBoxStopBits", "0"));
            numResponseTimeout.Value = decimal.Parse(DatabaseHelper.LoadSetting("numResponseTimeout", "1000"));
            numPollDelay.Value = decimal.Parse(DatabaseHelper.LoadSetting("numPollDelay", "500"));
            numIPConnTimeout.Value = decimal.Parse(DatabaseHelper.LoadSetting("numIPConnTimeout", "2000"));
            numIPPort.Value = decimal.Parse(DatabaseHelper.LoadSetting("numIPPort", "502"));

            bool isRtuChecked = bool.Parse(DatabaseHelper.LoadSetting("IsRTU", "true"));
            if (isRtuChecked) rBtnRTU.Checked = true; else rBtnASCII.Checked = true;
            cboxIPAddress.Items.Clear();
            var savedIPs = DatabaseHelper.LoadIpAddresses();
            cboxIPAddress.Items.AddRange(savedIPs.ToArray());
            string lastUsedIP = DatabaseHelper.LoadSetting("LastUsedIPAddress", "127.0.0.1");
            if (cboxIPAddress.Items.Contains(lastUsedIP)) cboxIPAddress.SelectedItem = lastUsedIP;
            else if (cboxIPAddress.Items.Count > 0) cboxIPAddress.SelectedIndex = 0;
        }
        #endregion

        #region Obsługa Portów COM
        private void RefreshComPorts()
        {
            string previouslySelected = cboxComPort.SelectedItem?.ToString();
            cboxComPort.Items.Clear();
            string portsClassGuid = "{4d36e978-e325-11ce-bfc1-08002be10318}";
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity"))
            {
                var devices = searcher.Get().Cast<ManagementObject>().ToList();
                foreach (ManagementObject device in devices)
                {
                    object classGuid = device.GetPropertyValue("ClassGuid");
                    if (classGuid == null || !classGuid.ToString().Equals(portsClassGuid, StringComparison.InvariantCultureIgnoreCase)) continue;
                    string pnpDeviceId = device.GetPropertyValue("PnpDeviceID")?.ToString() ?? "";
                    string regPath = $"HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\{pnpDeviceId}\\Device Parameters";
                    string portName = Registry.GetValue(regPath, "PortName", "")?.ToString() ?? "";
                    if (string.IsNullOrEmpty(portName)) continue;
                    string caption = device.GetPropertyValue("Caption")?.ToString() ?? "";
                    string status = device.GetPropertyValue("Status")?.ToString() ?? "Unknown";
                    string friendlyName = Registry.GetValue(regPath, "FriendlyName", "")?.ToString() ?? "";
                    string serInterfaceValue = Registry.GetValue(regPath, "SerInterface", "")?.ToString() ?? "";
                    string interfaceType = "";
                    if (string.IsNullOrEmpty(friendlyName)) friendlyName = Registry.GetValue($"HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\{pnpDeviceId}", "FriendlyName", "")?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(serInterfaceValue) && friendlyName.Contains("MOXA"))
                    {
                        if (int.TryParse(serInterfaceValue, out int portInterface))
                        {
                            switch (portInterface)
                            {
                                case 0: interfaceType = " (RS-232)"; break;
                                case 1: interfaceType = " (RS-422)"; break;
                                case 2: interfaceType = " (RS-485 2W)"; break;
                                case 3: interfaceType = " (RS-485 4W)"; break;
                            }
                        }
                    }
                    int comStringPos = caption.IndexOf(" (COM");
                    if (comStringPos > 0) caption = caption.Substring(0, comStringPos);
                    string finalDisplayName = (status != "OK" ? "⚠ " : "") + $"{portName} - {caption}{interfaceType}";
                    cboxComPort.Items.Add(finalDisplayName);
                }
            }
            if (cboxComPort.Items.Count == 0)
            {
                cboxComPort.Items.Add("No COM ports found");
                cboxComPort.Enabled = false;
            }
            else
            {
                cboxComPort.Enabled = true;
                int indexToSelect = previouslySelected != null ? cboxComPort.FindStringExact(previouslySelected) : -1;
                cboxComPort.SelectedIndex = (indexToSelect != -1) ? indexToSelect : 0;
            }
        }

        private void btnRefreshCom_Click(object sender, EventArgs e) => RefreshComPorts();
        private void CboxComPort_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            string text = cboxComPort.GetItemText(cboxComPort.Items[e.Index]);
            e.DrawBackground();
            using (SolidBrush br = new SolidBrush(e.ForeColor)) { e.Graphics.DrawString(text, e.Font, br, e.Bounds); }
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && cboxComPort.DroppedDown) { toolTip1.Show(text, cboxComPort, e.Bounds.Right, e.Bounds.Bottom, 2000); }
            e.DrawFocusRectangle();
        }
        private void CboxComPort_DropDownClosed(object sender, EventArgs e) => toolTip1.Hide(cboxComPort);
        #endregion

        #region Zapisywanie ustawień
        private void cboxConnection_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isSerial = (cboxConnection.SelectedIndex == 0);
            gboxSerialSettings.Visible = isSerial;
            gboxIPSettings.Visible = !isSerial;
            gBoxSerialMode.Enabled = (cboxConnection.SelectedIndex != 1);
            DatabaseHelper.SaveSetting("ConnectionType", cboxConnection.SelectedIndex.ToString());
        }

        private void cboxComPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboxComPort.SelectedItem != null) DatabaseHelper.SaveSetting("ComPortName", cboxComPort.SelectedItem.ToString());
        }

        private void CboxIPAddress_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboxIPAddress.SelectedItem != null) DatabaseHelper.SaveSetting("LastUsedIPAddress", cboxIPAddress.SelectedItem.ToString());
        }

        private async void CboxIPAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string address = cboxIPAddress.Text.Trim();
                if (string.IsNullOrEmpty(address)) return;
                if (await IsValidIpOrHostname(address))
                {
                    DatabaseHelper.SaveIpAddress(address);
                    if (!cboxIPAddress.Items.Contains(address)) cboxIPAddress.Items.Insert(0, address);
                    cboxIPAddress.SelectedItem = address;
                    DatabaseHelper.SaveSetting("LastUsedIPAddress", address);
                }
            }
        }

        private async Task<bool> IsValidIpOrHostname(string address)
        {
            if (IPAddress.TryParse(address, out _)) return true;
            try { return (await Dns.GetHostEntryAsync(address)).AddressList.Any(); }
            catch { return false; }
        }

        private void Setting_Changed(object sender, EventArgs e)
        {
            if (sender is ComboBox cb) DatabaseHelper.SaveSetting(cb.Name, cb.SelectedIndex.ToString());
            else if (sender is NumericUpDown nud) DatabaseHelper.SaveSetting(nud.Name, nud.Value.ToString());
            else if (sender is RadioButton rb && rb.Checked) DatabaseHelper.SaveSetting("IsRTU", rBtnRTU.Checked.ToString());
        }
        #endregion

        #region Obsługa połączenia
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (serialPort?.IsOpen == true || tcpClient?.Connected == true) { Disconnect(); return; }
            try
            {
                bool success = cboxConnection.SelectedIndex switch
                {
                    0 => await ConnectSerialAsync(),
                    1 or 2 => await ConnectTcpAsync(),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }

        private Task<bool> ConnectSerialAsync()
        {
            if (cboxComPort.SelectedItem == null || cboxComPort.Text.Contains("No COM ports")) throw new InvalidOperationException("No valid COM port selected.");
            string portName = cboxComPort.Text.Split('-')[0].Trim();
            serialPort = new SerialPort(portName, int.Parse(cBoxBaudRate.Text), (Parity)Enum.Parse(typeof(Parity), cBoxParity.Text), int.Parse(cBoxDataBits.Text), (StopBits)Enum.Parse(typeof(StopBits), cBoxStopBits.Text))
            {
                ReadTimeout = (int)numResponseTimeout.Value,
                WriteTimeout = (int)numResponseTimeout.Value
            };
            serialPort.Open();
            UpdateUiState(true);
            toolStripStatusLabel1.Text = $"Connected: {portName}/{cBoxBaudRate.Text}/{cBoxDataBits.Text}/{cBoxParity.Text[0]}/{cBoxStopBits.SelectedIndex + 1}";
            return Task.FromResult(true);
        }

        private async Task<bool> ConnectTcpAsync()
        {
            string ipAddress = cboxIPAddress.Text;
            if (string.IsNullOrWhiteSpace(ipAddress)) throw new InvalidOperationException("IP Address or Hostname cannot be empty.");

            tcpClient = new System.Net.Sockets.TcpClient();
            await tcpClient.ConnectAsync(ipAddress, (int)numIPPort.Value);

            modbusService = new ModbusService(tcpClient.GetStream());
            modbusService.FrameDataAvailable += (logEntry) =>
            {
                if (_commsLogWindow != null && !_commsLogWindow.IsDisposed)
                {
                    _commsLogWindow.LogFrame(logEntry);
                }
            };
            UpdateUiState(true);
            toolStripStatusLabel1.Text = $"Connected: {ipAddress}:{(int)numIPPort.Value}";
            return true;
        }

        private void Disconnect()
        {
            serialPort?.Close();
            serialPort = null;
            tcpClient?.Close();
            tcpClient = null;
            modbusService = null;
            UpdateUiState(false);
        }

        private void UpdateUiState(bool isConnected)
        {
            gboxConnection.Enabled = !isConnected;
            gboxIPSettings.Enabled = !isConnected;
            gboxSerialSettings.Enabled = !isConnected;
            gBoxGlobalSettings.Enabled = !isConnected;
            btnConnect.Enabled = !isConnected;
            btnDisconnect.Enabled = isConnected;
            btnConnect.BackColor = isConnected ? SystemColors.Control : Color.GreenYellow;
            btnDisconnect.BackColor = isConnected ? Color.Salmon : SystemColors.Control;
            statusStrip1.Visible = isConnected;
            if (!isConnected) toolStripStatusLabel1.Text = "Disconnected";
        }

        private void btnDisconnect_Click(object sender, EventArgs e) => Disconnect();
        #endregion


        private void communicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_commsLogWindow == null || _commsLogWindow.IsDisposed)
            {
                _commsLogWindow = new CommunicationLogWindow();
                _commsLogWindow.MdiParent = this;
            }
            _commsLogWindow.Show();
            _commsLogWindow.Activate();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newDevice = new ModbusDevice
            {
                MdiParent = this
            };
            newDevice.DeviceSaved += (s, ev) => LoadDevicesToTree();
            newDevice.Show();
            newDevice.Activate();
        }



        private void LoadDevicesToTree()
        {
            TreeNode rootNode = treeView.Nodes[0];
            if (rootNode == null) return;

            rootNode.Nodes.Clear();

            using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                connection.Open();
                var deviceCmd = connection.CreateCommand();
                deviceCmd.CommandText = "SELECT DeviceId, DeviceName, SlaveId FROM Devices ORDER BY DeviceName";

                using (var deviceReader = deviceCmd.ExecuteReader())
                {
                    while (deviceReader.Read())
                    {
                        long deviceId = deviceReader.GetInt64(0);
                        string deviceName = deviceReader.GetString(1);
                        int slaveId = deviceReader.GetInt32(2);

                        TreeNode deviceNode = new TreeNode(deviceName)
                        {
                            ImageIndex = 1,
                            SelectedImageIndex = 1,
                            Tag = deviceId
                        };
                        rootNode.Nodes.Add(deviceNode);

                        var groupCmd = connection.CreateCommand();
                        groupCmd.CommandText = "SELECT GroupId, GroupName FROM ReadingGroups WHERE DeviceId = $deviceId ORDER BY GroupName";
                        groupCmd.Parameters.AddWithValue("$deviceId", deviceId);

                        using (var groupReader = groupCmd.ExecuteReader())
                        {
                            while (groupReader.Read())
                            {
                                TreeNode groupNode = new TreeNode(groupReader.GetString(1))
                                {
                                    ImageIndex = 2,
                                    SelectedImageIndex = 2,
                                    Tag = groupReader.GetInt64(0)
                                };
                                deviceNode.Nodes.Add(groupNode);
                            }
                        }
                    }
                }
            }
            //rootNode.Expand();
        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 1 && e.Node.Tag != null)
            {
                long deviceId = (long)e.Node.Tag;
                OpenSavedDevice(deviceId);
            }
        }

        private void OpenSavedDevice(long deviceId)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
                {
                    connection.Open();

                    var deviceForm = new ModbusDevice { MdiParent = this };
                    deviceForm.DeviceSaved += (s, ev) => LoadDevicesToTree();

                    var deviceCmd = connection.CreateCommand();
                    deviceCmd.CommandText = "SELECT DeviceName, SlaveId FROM Devices WHERE DeviceId = $deviceId";
                    deviceCmd.Parameters.AddWithValue("$deviceId", deviceId);

                    using (var deviceReader = deviceCmd.ExecuteReader())
                    {
                        if (deviceReader.Read())
                        {
                            deviceForm.Text = deviceReader.GetString(0);
                            deviceForm.DeviceName = deviceReader.GetString(0);
                            deviceForm.SlaveId = Convert.ToInt32(deviceReader.GetValue(1));
                        }
                    }

                    var groupCmd = connection.CreateCommand();
                    groupCmd.CommandText = "SELECT GroupId, GroupName, FunctionCode, StartAddress, Quantity FROM ReadingGroups WHERE DeviceId = $deviceId";
                    groupCmd.Parameters.AddWithValue("$deviceId", deviceId);

                    using (var groupReader = groupCmd.ExecuteReader())
                    {
                        deviceForm.DeviceTabControl.TabPages.Clear();

                        while (groupReader.Read())
                        {
                            long groupId = groupReader.GetInt64(0);
                            string groupName = groupReader.GetString(1);
                            int funcCode = Convert.ToInt32(groupReader.GetValue(2));
                            int startAddr = Convert.ToInt32(groupReader.GetValue(3));
                            int quantity = Convert.ToInt32(groupReader.GetValue(4));

                            ReadingsTab readingsTab = new ReadingsTab { Dock = DockStyle.Fill };
                            readingsTab.SetConfiguration(funcCode, startAddr, quantity);

                            TabPage newTab = new TabPage(groupName);
                            newTab.Controls.Add(readingsTab);
                            deviceForm.DeviceTabControl.TabPages.Add(newTab);

                            var regCmd = connection.CreateCommand();
                            regCmd.CommandText = "SELECT RegisterNumber, RegisterName FROM RegisterDefinitions WHERE GroupId = $groupId";
                            regCmd.Parameters.AddWithValue("$groupId", groupId);

                            var registers = new List<Tuple<int, string>>();
                            using (var regReader = regCmd.ExecuteReader())
                            {
                                while (regReader.Read())
                                {
                                    registers.Add(new Tuple<int, string>(Convert.ToInt32(regReader.GetValue(0)), regReader.GetString(1)));
                                }
                                }
                            readingsTab.SetRegisterDefinitions(registers);
                        }
                    }

                    deviceForm.Show();
                    deviceForm.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading device: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}