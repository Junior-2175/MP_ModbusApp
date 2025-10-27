using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using NModbus;
using NModbus.Device;
using System;
using System.Diagnostics;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using NModbus.Device;
using NModbus.IO;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using NModbus.Logging;

namespace MP_ModbusApp
{
    public partial class MainWindow : Form
    {
        public IModbusMaster ModbusMaster => _modbusMaster;


        private SerialPort serialPort;
        private System.Net.Sockets.TcpClient tcpClient;
        private CommunicationLogWindow _commsLogWindow;
        private NModbusLogger _nmodbusLogger;
        private Cursor _deviceDragCursor;
        bool sidePanelHidden = false;
        private readonly ToolTip toolTip1 = new ToolTip();

        private IModbusMaster _modbusMaster;


        public int GetPollDelay()
        {
            return (int)numPollDelay.Value;
        }

        public MainWindow()
        {
            InitializeComponent();
            toolTip1.SetToolTip(openMenu, "Show / Hide connection setting");
            toolTip1.SetToolTip(openTree, "Show / Hide devices tree");
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.AllowDrop = true;

            try
            {
                Image img = imageList1.Images[1];
                Bitmap bmp = new Bitmap(img);
                _deviceDragCursor = new Cursor(bmp.GetHicon());
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to create drag cursor: " + ex.Message);
                _deviceDragCursor = Cursors.Default;
            }

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
            try
            {
                if (_commsLogWindow == null || _commsLogWindow.IsDisposed)
                {
                    _commsLogWindow = new CommunicationLogWindow();
                    _commsLogWindow.MdiParent = this;
                    _nmodbusLogger = new NModbusLogger(_commsLogWindow);
                    // Ważne: NIE pokazuj okna, tylko stwórz instancje w tle.
                    // _commsLogWindow.Show(); <-- Celowo pominięte
                }


                // (IEnumerable<IModbusFunctionService> functionServices, bool useOldStyleReadFunction, ILoggerFactory loggerFactory)
                var factory = new ModbusFactory(null, false, _nmodbusLogger);

                if (cboxConnection.SelectedIndex == 0) // Serial Port
                {
                    string fullPortName = cboxComPort.SelectedItem.ToString();
                    string portName = fullPortName.Split(' ')[0];
                    if (portName == "⚠") portName = fullPortName.Split(' ')[1];

                    int baudRate = int.Parse(cBoxBaudRate.SelectedItem.ToString());
                    int dataBits = int.Parse(cBoxDataBits.SelectedItem.ToString());
                    System.IO.Ports.Parity parity = Enum.Parse<System.IO.Ports.Parity>(cBoxParity.SelectedItem.ToString());
                    System.IO.Ports.StopBits stopBits = Enum.Parse<System.IO.Ports.StopBits>(cBoxStopBits.SelectedItem.ToString());

                    serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
                    serialPort.Open();

                    _modbusMaster = rBtnRTU.Checked
                    ? factory.CreateRtuMaster((IStreamResource)serialPort)
                    : factory.CreateAsciiMaster((IStreamResource)serialPort);
                }
                else // TCP/IP
                {
                    tcpClient = new System.Net.Sockets.TcpClient();
                    await tcpClient.ConnectAsync(cboxIPAddress.Text, (int)numIPPort.Value);
                    _modbusMaster = factory.CreateMaster(tcpClient);
                }

                _modbusMaster.Transport.ReadTimeout = (int)numResponseTimeout.Value;
                _modbusMaster.Transport.WriteTimeout = (int)numResponseTimeout.Value;

                UpdateUiState(true);
                toolStripStatusLabel1.Text = "Connected!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect();
            }
        }

        private void Disconnect()
        {
            _modbusMaster?.Dispose(); // Ważne!
            _modbusMaster = null;

            serialPort?.Close();
            serialPort = null;
            tcpClient?.Close();
            tcpClient = null;
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

                // --- ZMIENIONY KOD ---
                // Tworzymy instancję naszego niestandardowego loggera NModbus
                _nmodbusLogger = new NModbusLogger(_commsLogWindow);

                if (_modbusMaster != null)
                {
                    MessageBox.Show("Logger ramek HEX aktywowany. \nProszę się rozłączyć i połączyć ponownie, aby rozpocząć przechwytywanie.", "Logger Aktywowany",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }
                // --- KONIEC ZMIENIONEGO KODU ---
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
                            regCmd.CommandText = "SELECT RegisterNumber, RegisterName, DisplayFormatColumn FROM RegisterDefinitions WHERE GroupId = $groupId";
                            regCmd.Parameters.AddWithValue("$groupId", groupId);

                            var registers = new List<Tuple<int, string, string>>();
                            using (var regReader = regCmd.ExecuteReader())
                            {
                                while (regReader.Read())
                                {
                                    // ---- ZAKTUALIZOWANA LOGIKA WCZYTYWANIA ----
                                    int regNum;
                                    string regNumString = regReader.GetValue(0).ToString();

                                    // Próbujemy sparsować numer rejestru
                                    if (!int.TryParse(regNumString, out regNum))
                                    {
                                        // Jeśli się nie uda (bo to np. "100 - 103"),
                                        // próbujemy odzyskać pierwszą liczbę.
                                        string[] parts = regNumString.Split(' ');
                                        if (parts.Length > 0 && int.TryParse(parts[0], out regNum))
                                        {
                                            // Sukces, odzyskaliśmy "100"
                                        }
                                        else
                                        {
                                            // Całkowicie uszkodzone dane, pomiń ten rejestr
                                            continue;
                                        }
                                    }
                                    // ---- KONIEC ZAKTUALIZOWANEJ LOGIKI ----

                                    string regName = regReader.GetString(1);
                                    string regFormat = regReader.IsDBNull(2) ? "Unsigned16" : regReader.GetString(2);

                                    registers.Add(new Tuple<int, string, string>(regNum, regName, regFormat));
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

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));

                if (node.Level == 1 && node.Tag != null)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void MainWindow_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));

                if (node.Level == 1 && node.Tag != null)
                {
                    try
                    {
                        long deviceId = (long)node.Tag;
                        OpenSavedDevice(deviceId);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening device: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeNode node = (TreeNode)e.Item;

                if (node.Level == 1 && node.Tag != null)
                {
                    treeView.DoDragDrop(node, DragDropEffects.Copy);
                }
            }
        }

        private void treeView_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effect == DragDropEffects.Copy)
            {
                e.UseDefaultCursors = false;
                Cursor.Current = _deviceDragCursor;
            }
            else
            {
                e.UseDefaultCursors = true;
            }
        }

        private void importDeviceContextMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "XML Files (*.xml)|*.xml";
                ofd.Title = "Import device configuration";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ImportDeviceFromXml(ofd.FileName);
                        LoadDevicesToTree();
                        MessageBox.Show("Import completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (XmlException xmlEx)
                    {
                        MessageBox.Show($"XML file validation error:: {xmlEx.Message}\n\nLineNumber: {xmlEx.LineNumber}, Position: {xmlEx.LinePosition}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error during import: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void ImportDeviceFromXml(string filePath)
        {
            XDocument doc = XDocument.Load(filePath, LoadOptions.SetLineInfo);

            XElement root = doc.Root;
            if (root.Name != "ModbusDevice")
                throw new XmlException("The file is not a valid ModbusDevice configuration file (incorrect root node).");

            XAttribute deviceNameAttr = root.Attribute("DeviceName") ?? throw new XmlException("Missing 'DeviceName' attribute in the root node.", null, (root as IXmlLineInfo).LineNumber, (root as IXmlLineInfo).LinePosition);
            XAttribute slaveIdAttr = root.Attribute("SlaveId") ?? throw new XmlException("Missing 'SlaveId' attribute in the root node.", null, (root as IXmlLineInfo).LineNumber, (root as IXmlLineInfo).LinePosition);

            string deviceName = deviceNameAttr.Value;
            if (!int.TryParse(slaveIdAttr.Value, out int slaveId) || slaveId < 1 || slaveId > 254)
                throw new XmlException($"The 'SlaveId' attribute has an invalid value: '{slaveIdAttr.Value}'. Expected a number 1-254.", null, (slaveIdAttr as IXmlLineInfo).LineNumber, (slaveIdAttr as IXmlLineInfo).LinePosition);

            using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                connection.Open();

                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Devices WHERE DeviceName = $name;";
                checkCmd.Parameters.AddWithValue("$name", deviceName);
                if ((long)checkCmd.ExecuteScalar() > 0)
                {
                    using (RenameForm renameDialog = new RenameForm())
                    {
                        renameDialog.Text = "Name Conflict";
                        renameDialog.newName = deviceName;
                        MessageBox.Show($"A device named '{deviceName}' already exists. Please provide a new name.", "Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        if (renameDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(renameDialog.newName))
                        {
                            deviceName = renameDialog.newName;
                        }
                        else
                        {
                            throw new Exception("Import canceled by user due to name conflict.");
                        }
                    }
                }

                using (var transaction = connection.BeginTransaction())
                {
                    var deviceCmd = connection.CreateCommand();
                    deviceCmd.Transaction = transaction;
                    deviceCmd.CommandText = "INSERT INTO Devices (DeviceName, SlaveId) VALUES ($name, $slaveId) RETURNING DeviceId;";
                    deviceCmd.Parameters.AddWithValue("$name", deviceName);
                    deviceCmd.Parameters.AddWithValue("$slaveId", slaveId);
                    long deviceId = (long)deviceCmd.ExecuteScalar();

                    foreach (var groupNode in root.Elements("ReadingGroup"))
                    {
                        XAttribute groupNameAttr = groupNode.Attribute("GroupName") ?? throw new XmlException("Missing 'GroupName' attribute in 'ReadingGroup' node.", null, (groupNode as IXmlLineInfo).LineNumber, (groupNode as IXmlLineInfo).LinePosition);
                        XAttribute funcCodeAttr = groupNode.Attribute("FunctionCode") ?? throw new XmlException("Missing 'FunctionCode' attribute.", null, (groupNode as IXmlLineInfo).LineNumber, (groupNode as IXmlLineInfo).LinePosition);
                        XAttribute startAddrAttr = groupNode.Attribute("StartAddress") ?? throw new XmlException("Missing 'StartAddress' attribute.", null, (groupNode as IXmlLineInfo).LineNumber, (groupNode as IXmlLineInfo).LinePosition);
                        XAttribute quantityAttr = groupNode.Attribute("Quantity") ?? throw new XmlException("Missing 'Quantity' attribute.", null, (groupNode as IXmlLineInfo).LineNumber, (groupNode as IXmlLineInfo).LinePosition);

                        if (!int.TryParse(funcCodeAttr.Value, out int funcCode) || funcCode < 0 || funcCode > 3)
                            throw new XmlException($"Invalid 'FunctionCode': {funcCodeAttr.Value}. Expected 0-3.", null, (funcCodeAttr as IXmlLineInfo).LineNumber, (funcCodeAttr as IXmlLineInfo).LinePosition);
                        if (!int.TryParse(startAddrAttr.Value, out int startAddr) || startAddr < 0 || startAddr > 65535)
                            throw new XmlException($"Invalid 'StartAddress': {startAddrAttr.Value}. Expected 0-65535.", null, (startAddrAttr as IXmlLineInfo).LineNumber, (startAddrAttr as IXmlLineInfo).LinePosition);
                        if (!int.TryParse(quantityAttr.Value, out int quantity) || quantity < 1 || quantity > 125)
                            throw new XmlException($"Invalid 'Quantity': {quantityAttr.Value}. Expected 1-125.", null, (quantityAttr as IXmlLineInfo).LineNumber, (quantityAttr as IXmlLineInfo).LinePosition);

                        var groupCmd = connection.CreateCommand();
                        groupCmd.Transaction = transaction;
                        groupCmd.CommandText = "INSERT INTO ReadingGroups (DeviceId, GroupName, FunctionCode, StartAddress, Quantity) VALUES ($deviceId, $groupName, $funcCode, $startAddr, $quantity) RETURNING GroupId;";
                        groupCmd.Parameters.AddWithValue("$deviceId", deviceId);
                        groupCmd.Parameters.AddWithValue("$groupName", groupNameAttr.Value);
                        groupCmd.Parameters.AddWithValue("$funcCode", funcCode);
                        groupCmd.Parameters.AddWithValue("$startAddr", startAddr);
                        groupCmd.Parameters.AddWithValue("$quantity", quantity);
                        long groupId = (long)groupCmd.ExecuteScalar();

                        foreach (var regNode in groupNode.Elements("Register"))
                        {
                            XAttribute regNumAttr = regNode.Attribute("RegisterNumber") ?? throw new XmlException("Missing 'RegisterNumber' attribute in 'Register' node.", null, (regNode as IXmlLineInfo).LineNumber, (regNode as IXmlLineInfo).LinePosition);
                            XAttribute regNameAttr = regNode.Attribute("RegisterName") ?? throw new XmlException("Missing 'RegisterName' attribute in 'Register' node.", null, (regNode as IXmlLineInfo).LineNumber, (regNode as IXmlLineInfo).LinePosition);

                            if (!int.TryParse(regNumAttr.Value, out int regNum))
                                throw new XmlException($"Invalid 'RegisterNumber': {regNumAttr.Value}.", null, (regNumAttr as IXmlLineInfo).LineNumber, (regNumAttr as IXmlLineInfo).LinePosition);

                            var regCmd = connection.CreateCommand();
                            regCmd.Transaction = transaction;
                            regCmd.CommandText = "INSERT INTO RegisterDefinitions (GroupId, RegisterNumber, RegisterName) VALUES ($groupId, $regNum, $regName);";
                            regCmd.Parameters.AddWithValue("$groupId", groupId);
                            regCmd.Parameters.AddWithValue("$regNum", regNum);
                            regCmd.Parameters.AddWithValue("$regName", regNameAttr.Value);
                            regCmd.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }

        }
        private void exportDeviceContextMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView.SelectedNode;
            if (selectedNode == null || selectedNode.Level != 1 || selectedNode.Tag == null)
            {
                return;
            }

            long deviceId = (long)selectedNode.Tag;
            string deviceName = selectedNode.Text;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.FileName = $"{deviceName.Replace(" ", "_")}.xml";
                sfd.Filter = "XML Files (*.xml)|*.xml";
                sfd.Title = "Export device configuration";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ExportDeviceToXml(deviceId, sfd.FileName);
                        MessageBox.Show("Export completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"\"Error exporting: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportDeviceToXml(long deviceId, string filePath)
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            XElement rootNode = null;

            using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                connection.Open();

                var deviceCmd = connection.CreateCommand();
                deviceCmd.CommandText = "SELECT DeviceName, SlaveId FROM Devices WHERE DeviceId = $deviceId";
                deviceCmd.Parameters.AddWithValue("$deviceId", deviceId);

                using (var deviceReader = deviceCmd.ExecuteReader())
                {
                    if (deviceReader.Read())
                    {
                        rootNode = new XElement("ModbusDevice",
                                    new XAttribute("DeviceName", deviceReader.GetString(0)),
                                    new XAttribute("SlaveId", deviceReader.GetInt32(1))
                                );
                    }
                }

                if (rootNode == null)
                {
                    throw new Exception("Device not found.");
                }

                var groupCmd = connection.CreateCommand();
                groupCmd.CommandText = "SELECT GroupId, GroupName, FunctionCode, StartAddress, Quantity FROM ReadingGroups WHERE DeviceId = $deviceId";
                groupCmd.Parameters.AddWithValue("$deviceId", deviceId);

                using (var groupReader = groupCmd.ExecuteReader())
                {
                    while (groupReader.Read())
                    {
                        long groupId = groupReader.GetInt64(0);
                        var groupNode = new XElement("ReadingGroup",
                                            new XAttribute("GroupName", groupReader.GetString(1)),
                                            new XAttribute("FunctionCode", groupReader.GetInt32(2)),
                                            new XAttribute("StartAddress", groupReader.GetInt32(3)),
                                            new XAttribute("Quantity", groupReader.GetInt32(4))
                                        );

                        var regCmd = connection.CreateCommand();
                        regCmd.CommandText = "SELECT RegisterNumber, RegisterName FROM RegisterDefinitions WHERE GroupId = $groupId";
                        regCmd.Parameters.AddWithValue("$groupId", groupId);

                        using (var regReader = regCmd.ExecuteReader())
                        {
                            while (regReader.Read())
                            {
                                var regNode = new XElement("Register",
                                                    new XAttribute("RegisterNumber", regReader.GetInt32(0)),
                                                    new XAttribute("RegisterName", regReader.GetString(1))
                                                );
                                groupNode.Add(regNode);
                            }
                        }
                        rootNode.Add(groupNode);
                    }
                }
                doc.Add(rootNode);
            }
            doc.Save(filePath);
        }

        private void deleteDeviceContextMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView.SelectedNode;

            if (selectedNode == null || selectedNode.Level != 1 || selectedNode.Tag == null)
            {
                return;
            }

            long deviceId = (long)selectedNode.Tag;
            string deviceName = selectedNode.Text;

            var result = MessageBox.Show($"Are you sure you want to delete the device '{deviceName}'? This operation cannot be undone.",
                                         "Confirm deletion",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    DeleteDeviceFromDb(deviceId);
                    LoadDevicesToTree();
                    MessageBox.Show($"Device '{deviceName}' removed successfully.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the device: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteDeviceFromDb(long deviceId)
        {
            using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Devices WHERE DeviceId = $deviceId;";
                command.Parameters.AddWithValue("$deviceId", deviceId);
                command.ExecuteNonQuery();
            }
        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView.SelectedNode = e.Node;
            }
        }

        private void treeViewContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TreeNode selectedNode = treeView.SelectedNode;

            importDeviceContextMenuItem.Enabled = true;

            if (selectedNode != null && selectedNode.Level == 1)
            {
                exportDeviceContextMenuItem.Enabled = true;
                deleteDeviceContextMenuItem.Enabled = true;
            }
            else
            {
                exportDeviceContextMenuItem.Enabled = false;
                deleteDeviceContextMenuItem.Enabled = false;
            }
        }
        public void LogCommunicationEvent(ModbusFrameLog logEntry)
        {
            _commsLogWindow?.LogFrame(logEntry);
        }

        #region NModbus Logger Implementation

        /// <summary>
        /// Implementacja interfejsu NModbus.IModbusLogger, która przekazuje 
        /// surowe ramki HEX do naszego okna CommunicationLogWindow.
        /// (Wersja OSTATECZNA z parsowaniem błędów)
        /// </summary>
        public class NModbusLogger : IModbusLogger
        {
            private readonly CommunicationLogWindow _logWindow;

            public NModbusLogger(CommunicationLogWindow logWindow)
            {
                _logWindow = logWindow;
            }

            public bool ShouldLog(LoggingLevel level)
            {
                return level == LoggingLevel.Trace;
            }

            public void Log(LoggingLevel level, string message)
            {
                if (!ShouldLog(level) || _logWindow == null || _logWindow.IsDisposed)
                {
                    return;
                }

                if (message.StartsWith("TX: ", StringComparison.OrdinalIgnoreCase) ||
                    message.StartsWith("RX: ", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string direction = message.Substring(0, 2).ToUpper(); // "TX" lub "RX"
                        string hexData = message.Substring(4); // "01 03 00 64..." (bez spacji na początku)
                        string hexFrame = hexData;//.Replace(" ", ":"); // "01-03-00-..."
                        string errorDesc = "";

                        // --- NOWA LOGIKA PARSOWANIA BŁĘDÓW ---
                        if (direction == "RX")
                        {
                            string[] bytes = hexData.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            // Sprawdzamy, czy to TCP (ramka > 6 bajtów i zaczyna się od Trans. ID)
                            // czy RTU (ramka krótsza)
                            int fcIndex = -1;
                            if (bytes.Length > 7 && int.TryParse(bytes[0], System.Globalization.NumberStyles.HexNumber, null, out _))
                            {
                                // TCP: [MBAP: 6B] [SlaveID: 1B] [FC: 1B] [Dane/Błąd: NB]
                                // Przykład błędu: 00 01 00 00 00 03 01 83 03
                                fcIndex = 7;
                            }
                            else if (bytes.Length > 1) // Zakładamy RTU/ASCII
                            {
                                // RTU: [SlaveID: 1B] [FC: 1B] [Dane/Błąd: NB] [CRC: 2B]
                                fcIndex = 1;
                            }

                            if (fcIndex != -1 && bytes.Length > fcIndex)
                            {
                                // Sprawdź kod funkcji (FC)
                                if (int.TryParse(bytes[fcIndex], System.Globalization.NumberStyles.HexNumber, null, out int fc) && fc > 128)
                                {
                                    // To jest ramka błędu! (FC > 0x80)
                                    int originalFc = fc - 128;
                                    string exceptionCode = "??";
                                    if (bytes.Length > fcIndex + 1)
                                    {
                                        exceptionCode = bytes[fcIndex + 1]; // Kod błędu (np. 03)
                                    }

                                    string errorName;
                                    switch (exceptionCode)
                                    {
                                        case "01": errorName = "Illegal Function"; break;
                                        case "02": errorName = "Illegal Data Address"; break;
                                        case "03": errorName = "Illegal Data Value"; break; // Pana błąd
                                        case "04": errorName = "Slave Device Failure"; break;
                                        case "05": errorName = "Acknowledge"; break;
                                        case "06": errorName = "Slave Device Busy"; break;
                                        default: errorName = $"Unknown Exception"; break;
                                    }
                                    errorDesc = $"Modbus Error (FC: {fc}, Code: {exceptionCode}) - {errorName}";
                                }
                            }
                        }
                        // --- KONIEC NOWEJ LOGIKI ---

                        var logEntry = new ModbusFrameLog
                        {
                            Timestamp = DateTime.Now,
                            Direction = direction,
                            DataFrame = hexFrame,
                            ErrorDescription = errorDesc // Wypełniamy kolumnę błędu
                        };

                        _logWindow.LogFrame(logEntry);
                    }
                    catch (Exception)
                    {
                        // Ignoruj błędy parsowania logów, aby nie zawiesić aplikacji
                    }
                }
            }
        }

        #endregion
    } // Ostatnia klamra zamykająca namespace
}