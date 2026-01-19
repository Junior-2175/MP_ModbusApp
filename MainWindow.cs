using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using MP_ModbusApp.MP_modbus;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace MP_ModbusApp
{
    public partial class MainWindow : Form
    {
        private MP_modbus.IMyModbusMaster _modbusMaster;

        private SerialPort serialPort;
        private System.Net.Sockets.TcpClient tcpClient;
        private CommunicationLogWindow _commsLogWindow;
        private Cursor _deviceDragCursor;
        bool sidePanelHidden = false;
        private readonly ToolTip toolTip1 = new ToolTip();

        public MP_modbus.IMyModbusMaster ModbusMaster => _modbusMaster;

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

            // Create a custom cursor for dragging device nodes
            try
            {
                Image img = imageList1.Images[1]; // Device icon
                Bitmap bmp = new Bitmap(img);
                _deviceDragCursor = new Cursor(bmp.GetHicon());
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to create drag cursor: " + ex.Message);
                _deviceDragCursor = Cursors.Default;
            }

            // --- Initialize UI in a collapsed state ---
            sidePanel.Width = 21;
            setupButton.Height = 90;
            openMenu.Text = "Setup";
            openMenu.BackgroundImage = null;
            openMenu.BackgroundImageLayout = ImageLayout.Zoom;
            openMenu.Font = new Font("Segoe UI", 8F);
            openMenu.Height = 80;
            openMenu.Width = 15;
            openMenu.Left = 3;
            treeButton.Height = 110;
            openTree.Text = "Devices x";
            openTree.BackgroundImage = null;
            openTree.Font = new Font("Segoe UI", 8F);
            openTree.Height = 100;
            openTree.Width = 15;
            openTree.Left = 3;
            setupPanel.Visible = false;
            treeView.Visible = false;
            sidePanelHidden = true;
            this.Refresh();
            // --- End of collapsed state init ---

            cboxComPort.DrawMode = DrawMode.OwnerDrawFixed; // For custom drawing (e.g., warning icons)
            gboxIPSettings.Visible = false;


            PopulateComboBoxes();
            RefreshComPorts();
            LoadSettings();
            LoadDevicesToTree();

            // Event handlers for custom COM port tooltip
            cboxComPort.DrawItem += CboxComPort_DrawItem;
            cboxComPort.DropDownClosed += CboxComPort_DropDownClosed;
            cboxComPort.SelectedIndexChanged += cboxComPort_SelectedIndexChanged;

            UpdateUiState(false);
        }

        /// <summary>
        /// Handles clicking the "Setup" button to expand/collapse or switch the side panel.
        /// </summary>
        private void openMenu_Click(object sender, EventArgs e)
        {
            if (sidePanelHidden)
            {
                // --- State: Hidden -> Show Setup Panel ---
                sidePanel.Width = 350;
                setupButton.Height = 50;
                openMenu.Text = "";
                openMenu.BackgroundImage = Properties.Resources.icons8_double_left_50; // Collapse icon
                openMenu.BackgroundImageLayout = ImageLayout.Zoom;
                openMenu.Font = new Font("Segoe UI", 14.25F);
                openMenu.Height = 40;
                openMenu.Width = 40;
                openMenu.Left = 300;
                treeButton.Height = 50;
                openTree.Text = "";
                openTree.BackgroundImage = Properties.Resources.icons8_broadcasting_50; // Devices icon
                openTree.BackgroundImageLayout = ImageLayout.Zoom;
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
                    // --- State: Devices Visible -> Switch to Setup Panel ---
                    // (Button icons are already correct from openTree_Click)
                    setupPanel.Visible = true;
                    treeView.Visible = false;

                    // Update this button's icon to "collapse"
                    openMenu.BackgroundImage = Properties.Resources.icons8_double_left_50;
                    // Update the other button's icon to "devices"
                    openTree.BackgroundImage = Properties.Resources.icons8_broadcasting_50;
                }
                else
                {
                    // --- State: Setup Visible -> Hide Panel ---
                    sidePanel.Width = 21;
                    setupButton.Height = 90;
                    openMenu.Text = "Setup";
                    openMenu.BackgroundImage = null;
                    openMenu.Font = new Font("Segoe UI", 8F);
                    openMenu.Height = 80;
                    openMenu.Width = 15;
                    openMenu.Left = 3;
                    treeButton.Height = 110;
                    openTree.Text = "Devices x";
                    openTree.BackgroundImage = null;
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

        /// <summary>
        /// Handles clicking the "Devices" button to expand/collapse or switch the side panel.
        /// </summary>
        private void openTree_Click(object sender, EventArgs e)
        {
            if (sidePanelHidden)
            {
                // --- State: Hidden -> Show Devices Panel ---
                sidePanel.Width = 350;
                setupButton.Height = 50;
                openMenu.Text = "";
                openMenu.BackgroundImage = Properties.Resources.icons8_settings_50; // Setup icon
                openMenu.BackgroundImageLayout = ImageLayout.Zoom;
                openMenu.Font = new Font("Segoe UI", 14.25F);
                openMenu.Height = 40;
                openMenu.Width = 40;
                openMenu.Left = 300;
                treeButton.Height = 50;
                openTree.Text = "";
                openTree.BackgroundImage = Properties.Resources.icons8_double_left_50; // Collapse icon
                openTree.BackgroundImageLayout = ImageLayout.Zoom;
                openTree.Font = new Font("Segoe UI", 14.25F);
                openTree.Height = 40;
                openTree.Width = 40;
                openTree.Left = 300;
                setupPanel.Visible = false;
                treeView.Visible = true;
                sidePanelHidden = false;
                TreeNode rootNode = treeView.Nodes[0];
                rootNode.Expand();
            }
            else
            {
                if (!treeView.Visible)
                {
                    // --- State: Setup Visible -> Switch to Devices Panel ---
                    setupPanel.Visible = false;
                    treeView.Visible = true;

                    // Update this button's icon to "collapse"
                    openTree.BackgroundImage = Properties.Resources.icons8_double_left_50;
                    // Update the other button's icon to "setup"
                    openMenu.BackgroundImage = Properties.Resources.icons8_settings_50;

                    TreeNode rootNode = treeView.Nodes[0];
                    rootNode.Expand();
                }
                else
                {
                    // --- State: Devices Visible -> Hide Panel ---
                    sidePanel.Width = 21;
                    setupButton.Height = 90;
                    openMenu.Text = "Setup";
                    openMenu.BackgroundImage = null;
                    openMenu.Font = new Font("Segoe UI", 8F);
                    openMenu.Height = 80;
                    openMenu.Width = 15;
                    openMenu.Left = 3;
                    treeButton.Height = 110;
                    openTree.Text = "Devices x";
                    openTree.BackgroundImage = null;
                    openTree.Font = new Font("Segoe UI", 8F);
                    openTree.Height = 100;
                    openTree.Width = 15;
                    openTree.Left = 3;
                    setupPanel.Visible = false;
                    treeView.Visible = false;
                    sidePanelHidden = true;
                    TreeNode rootNode = treeView.Nodes[0];
                    rootNode.Collapse();
                }
            }
            this.Refresh();
        }
        #region Initialization and Settings Loading
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
            // Load settings from the SQLite database, using defaults if not found
            cboxConnection.SelectedIndex = int.Parse(DatabaseHelper.LoadSetting("ConnectionType", "0"));
            cBoxBaudRate.SelectedIndex = int.Parse(DatabaseHelper.LoadSetting("cBoxBaudRate", "5")); // Default: 9600
            cBoxDataBits.SelectedIndex = int.Parse(DatabaseHelper.LoadSetting("cBoxDataBits", "1")); // Default: 8
            cBoxParity.SelectedIndex = int.Parse(DatabaseHelper.LoadSetting("cBoxParity", "0")); // Default: None
            cBoxStopBits.SelectedIndex = int.Parse(DatabaseHelper.LoadSetting("cBoxStopBits", "0")); // Default: One
            numResponseTimeout.Value = decimal.Parse(DatabaseHelper.LoadSetting("numResponseTimeout", "1000"));
            numPollDelay.Value = decimal.Parse(DatabaseHelper.LoadSetting("numPollDelay", "500"));
            numMaxRetries.Value = decimal.Parse(DatabaseHelper.LoadSetting("numMaxRetries", "0"));
            numIPConnTimeout.Value = decimal.Parse(DatabaseHelper.LoadSetting("numIPConnTimeout", "2000"));
            numIPPort.Value = decimal.Parse(DatabaseHelper.LoadSetting("numIPPort", "502"));

            bool isRtuChecked = bool.Parse(DatabaseHelper.LoadSetting("IsRTU", "true"));
            if (isRtuChecked) rBtnRTU.Checked = true; else rBtnASCII.Checked = true;

            // Load saved IP addresses
            cboxIPAddress.Items.Clear();
            var savedIPs = DatabaseHelper.LoadIpAddresses();
            cboxIPAddress.Items.AddRange(savedIPs.ToArray());
            string lastUsedIP = DatabaseHelper.LoadSetting("LastUsedIPAddress", "127.0.0.1");
            if (cboxIPAddress.Items.Contains(lastUsedIP)) cboxIPAddress.SelectedItem = lastUsedIP;
            else if (cboxIPAddress.Items.Count > 0) cboxIPAddress.SelectedIndex = 0;
            else cboxIPAddress.Text = lastUsedIP; // Set text if not in list
        }
        #endregion

        #region COM Port Handling

        /// <summary>
        /// Refreshes the list of available COM ports using WMI and Registry lookup.
        /// Tries to find detailed info like friendly name and interface type (for MOXA).
        /// </summary>
        private void RefreshComPorts()
        {
            string previouslySelected = cboxComPort.SelectedItem?.ToString();
            cboxComPort.Items.Clear();
            string portsClassGuid = "{4d36e978-e325-11ce-bfc1-08002be10318}"; // WMI GUID for ports

            System.Collections.Generic.List<string> foundPorts = new System.Collections.Generic.List<string>();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity"))
                {
                    var devices = searcher.Get().Cast<ManagementObject>().ToList();
                    foreach (ManagementObject device in devices)
                    {
                        object classGuid = device.GetPropertyValue("ClassGuid");
                        if (classGuid == null || !classGuid.ToString().Equals(portsClassGuid, StringComparison.InvariantCultureIgnoreCase)) continue;

                        string pnpDeviceId = device.GetPropertyValue("PnpDeviceID")?.ToString() ?? "";
                        if (string.IsNullOrEmpty(pnpDeviceId)) continue;

                        // Get PortName from device parameters in Registry
                        string regPath = $"HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\{pnpDeviceId}\\Device Parameters";
                        string portName = Registry.GetValue(regPath, "PortName", "")?.ToString() ?? "";
                        if (string.IsNullOrEmpty(portName)) continue;

                        string caption = device.GetPropertyValue("Caption")?.ToString() ?? "";
                        string status = device.GetPropertyValue("Status")?.ToString() ?? "Unknown";

                        // Try to get a more user-friendly name
                        string friendlyName = Registry.GetValue(regPath, "FriendlyName", "")?.ToString() ?? "";
                        if (string.IsNullOrEmpty(friendlyName)) friendlyName = Registry.GetValue($"HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\{pnpDeviceId}", "FriendlyName", "")?.ToString() ?? "";

                        // Specific logic for MOXA devices to get interface type (RS-232/422/485)
                        string serInterfaceValue = Registry.GetValue(regPath, "SerInterface", "")?.ToString() ?? "";
                        string interfaceType = "";
                        if (!string.IsNullOrEmpty(serInterfaceValue) && (friendlyName.Contains("MOXA") || caption.Contains("MOXA")))
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

                        // Clean up the caption, remove the (COMx) part which is redundant
                        int comStringPos = caption.IndexOf(" (COM");
                        if (comStringPos > 0) caption = caption.Substring(0, comStringPos);

                        // Prepend a warning symbol if the device status is not "OK"
                        string finalDisplayName = (status != "OK" ? "⚠ " : "") + $"{portName} - {caption}{interfaceType}";
                        foundPorts.Add(finalDisplayName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to query WMI for COM ports: {ex.Message}");
                // Fallback to simple SerialPort.GetPortNames()
                var portNames = SerialPort.GetPortNames();
                foundPorts.AddRange(portNames);
            }

            // ULEPSZENIE: Sortowanie portów numeryczne (COM2 przed COM10)
            var sortedPorts = foundPorts.OrderBy(x =>
            {
                // Wyciągnij numer portu za pomocą RegEx dla poprawnego sortowania (COM2 < COM10)
                var match = Regex.Match(x, @"COM(\d+)");
                return match.Success ? int.Parse(match.Groups[1].Value) : 9999;
            }).ToArray();

            cboxComPort.Items.AddRange(sortedPorts);

            if (cboxComPort.Items.Count == 0)
            {
                cboxComPort.Items.Add("No COM ports found");
                cboxComPort.Enabled = false;
            }
            else
            {
                cboxComPort.Enabled = true;
                // Try to re-select the previously selected port
                int indexToSelect = previouslySelected != null ? cboxComPort.FindStringExact(previouslySelected) : -1;
                cboxComPort.SelectedIndex = (indexToSelect != -1) ? indexToSelect : 0;
            }
        }

        private void btnRefreshCom_Click(object sender, EventArgs e) => RefreshComPorts();

        /// <summary>
        /// Custom drawing for the COM port ComboBox to show a tooltip on hover.
        /// </summary>
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

        #region Saving Settings

        /// <summary>
        /// Handles selection change for the main connection type.
        /// </summary>
        private void cboxConnection_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isSerial = (cboxConnection.SelectedIndex == 0);
            gboxSerialSettings.Visible = isSerial;
            gboxIPSettings.Visible = !isSerial;

            // Disable RTU/ASCII mode selection for standard Modbus TCP (index 1)
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

        /// <summary>
        /// Validates and saves a new IP/Hostname when Enter is pressed in the ComboBox.
        /// </summary>
        private async void CboxIPAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Stop the "ding" sound
                string address = cboxIPAddress.Text.Trim();
                if (string.IsNullOrEmpty(address)) return;

                if (await IsValidIpOrHostname(address))
                {
                    DatabaseHelper.SaveIpAddress(address);
                    if (!cboxIPAddress.Items.Contains(address)) cboxIPAddress.Items.Insert(0, address);
                    cboxIPAddress.SelectedItem = address;
                    DatabaseHelper.SaveSetting("LastUsedIPAddress", address);
                }
                else
                {
                    MessageBox.Show($"'{address}' is not a valid IP address or resolvable host name.", "Invalid Address", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// Checks if a string is a valid IP or a DNS-resolvable hostname.
        /// </summary>
        private async Task<bool> IsValidIpOrHostname(string address)
        {
            if (IPAddress.TryParse(address, out _)) return true; // It's a valid IP
            try
            {
                return (await Dns.GetHostEntryAsync(address)).AddressList.Any(); // It's a resolvable hostname
            }
            catch
            {
                return false; // Not valid
            }
        }

        /// <summary>
        /// Generic event handler to save settings whenever a control's value changes.
        /// </summary>
        private void Setting_Changed(object sender, EventArgs e)
        {
            if (sender is ComboBox cb) DatabaseHelper.SaveSetting(cb.Name, cb.SelectedIndex.ToString());
            else if (sender is NumericUpDown nud) DatabaseHelper.SaveSetting(nud.Name, nud.Value.ToString());
            else if (sender is RadioButton rb && rb.Checked) DatabaseHelper.SaveSetting("IsRTU", rBtnRTU.Checked.ToString());
        }
        #endregion

        #region Connection Handling

        /// <summary>
        /// Attempts to connect using the settings specified in the UI.
        /// </summary>
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            string connectionPort = "";

            // ULEPSZENIE: Feedback dla użytkownika (klepsydra i blokada przycisku)
            Cursor.Current = Cursors.WaitCursor;
            btnConnect.Enabled = false;
            btnConnect.Text = "Łączenie...";

            try
            {
                // Ensure the log window is created
                if (_commsLogWindow == null || _commsLogWindow.IsDisposed)
                {
                    _commsLogWindow = new CommunicationLogWindow();
                    _commsLogWindow.MdiParent = this;
                }

                IMyModbusTransport transport;
                if (cboxConnection.SelectedIndex == 0) // Serial Port
                {
                    string fullPortName = cboxComPort.SelectedItem.ToString();
                    string portName = fullPortName.Split(' ')[0];
                    if (portName == "⚠") portName = fullPortName.Split(' ')[1]; // Handle warning icon

                    int baudRate = int.Parse(cBoxBaudRate.SelectedItem.ToString());
                    int dataBits = int.Parse(cBoxDataBits.SelectedItem.ToString());
                    System.IO.Ports.Parity parity = Enum.Parse<System.IO.Ports.Parity>(cBoxParity.SelectedItem.ToString());
                    System.IO.Ports.StopBits stopBits = Enum.Parse<System.IO.Ports.StopBits>(cBoxStopBits.SelectedItem.ToString());

                    serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
                    serialPort.Open();

                    transport = new MyModbusSerialTransport(serialPort, rBtnRTU.Checked, this);
                    connectionPort = $"{portName}/{baudRate}/{parity}/{dataBits}/{stopBits}";

                }
                else // TCP/IP based
                {
                    tcpClient = new System.Net.Sockets.TcpClient();
                    // Use a connection timeout
                    var connectTask = tcpClient.ConnectAsync(cboxIPAddress.Text, (int)numIPPort.Value);
                    var timeoutTask = Task.Delay((int)numIPConnTimeout.Value);

                    if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
                    {
                        throw new TimeoutException("Connection attempt timed out.");
                    }
                    await connectTask; // Propagate connection exceptions if it failed

                    if (cboxConnection.SelectedIndex == 1) // Modbus TCP
                    {
                        transport = new MyModbusTcpTransport(tcpClient, this);
                    }
                    else // Modbus RTU/ASCII over TCP
                    {
                        transport = new MyModbusTcpSerialTransport(tcpClient, rBtnRTU.Checked, this);
                    }
                    connectionPort = $"{cboxIPAddress.Text}:{numIPPort.Value}";
                }

                // Apply global timeouts to the transport
                transport.ReadTimeout = (int)numResponseTimeout.Value;
                transport.WriteTimeout = (int)numResponseTimeout.Value;

                _modbusMaster = new MyModbusMaster(transport);

                UpdateUiState(true);
                toolStripStatusLabel1.Text = "Connected: " + connectionPort;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect(); // Clean up any partial connection
            }
            finally
            {
                // Przywrócenie stanu interfejsu
                Cursor.Current = Cursors.Default;
                btnConnect.Text = "Connect"; // Przywróć oryginalny tekst (jeśli nie udało się połączyć, Disconnect w catch już obsłużył stan Enabled)

                // Jeśli łączenie się nie powiodło (czyli UpdateUiState(true) nie zostało wywołane),
                // musimy upewnić się, że przycisk jest odblokowany.
                if (_modbusMaster == null)
                {
                    btnConnect.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Disconnects and cleans up all resources.
        /// </summary>
        private void Disconnect()
        {
            // Stop polling in all child device windows
            foreach (Form childForm in this.MdiChildren)
            {
                if (childForm is ModbusDevice deviceWindow)
                {
                    deviceWindow.StopPolling();
                }
            }

            _modbusMaster?.Dispose(); // This will dispose the transport as well
            _modbusMaster = null;

            serialPort?.Close();
            serialPort = null;
            tcpClient?.Close();
            tcpClient = null;

            UpdateUiState(false);
        }

        /// <summary>
        /// Enables/disables UI elements based on connection state.
        /// </summary>
        private void UpdateUiState(bool isConnected)
        {
            gboxConnection.Enabled = !isConnected;
            gboxIPSettings.Enabled = !isConnected;
            gboxSerialSettings.Enabled = !isConnected;
            gBoxGlobalSettings.Enabled = !isConnected;
            numMaxRetries.Enabled = !isConnected;
            btnConnect.Enabled = !isConnected;
            btnDisconnect.Enabled = isConnected;

            btnConnect.BackColor = isConnected ? SystemColors.Control : Color.GreenYellow;
            btnDisconnect.BackColor = isConnected ? Color.Salmon : SystemColors.Control;

            statusStrip1.Visible = isConnected;
            if (!isConnected) toolStripStatusLabel1.Text = "Disconnected";
        }

        private void btnDisconnect_Click(object sender, EventArgs e) => Disconnect();
        #endregion

        /// <summary>
        /// Opens or focuses the communication log window.
        /// </summary>
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

        /// <summary>
        /// Creates a new, blank device window.
        /// </summary>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newDevice = new ModbusDevice
            {
                MdiParent = this
            };
            // Add event handler to refresh the tree when a new device is saved
            newDevice.DeviceSaved += (s, ev) => LoadDevicesToTree();
            newDevice.Show();
            newDevice.Activate();
        }


        /// <summary>
        /// Clears and reloads the device tree from the SQLite database.
        /// </summary>
        private void LoadDevicesToTree()
        {
            TreeNode rootNode = treeView.Nodes[0];
            if (rootNode == null) return;

            rootNode.Nodes.Clear();

            try
            {
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

                            // Create the device node
                            TreeNode deviceNode = new TreeNode(deviceName)
                            {
                                ImageIndex = 1,
                                SelectedImageIndex = 1,
                                Tag = deviceId // Store the DB ID in the Tag
                            };
                            rootNode.Nodes.Add(deviceNode);

                            // Get all reading groups for this device
                            var groupCmd = connection.CreateCommand();
                            groupCmd.CommandText = "SELECT GroupId, GroupName FROM ReadingGroups WHERE DeviceId = $deviceId ORDER BY GroupName";
                            groupCmd.Parameters.AddWithValue("$deviceId", deviceId);

                            using (var groupReader = groupCmd.ExecuteReader())
                            {
                                while (groupReader.Read())
                                {
                                    // Create the group node
                                    TreeNode groupNode = new TreeNode(groupReader.GetString(1))
                                    {
                                        ImageIndex = 2,
                                        SelectedImageIndex = 2,
                                        Tag = groupReader.GetInt64(0) // Store GroupId
                                    };
                                    deviceNode.Nodes.Add(groupNode);
                                }
                            }
                        }
                    }
                }
                rootNode.Expand(); // Expand the root node by default
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load devices from database: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens the device editor when a device node (Level 1) is double-clicked.
        /// </summary>
        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 1 && e.Node.Tag != null) // Level 1 is a device
            {
                long deviceId = (long)e.Node.Tag;
                OpenSavedDevice(deviceId);
            }
        }

        /// <summary>
        /// Loads a device configuration from the DB and opens it in a new ModbusDevice window.
        /// </summary>
        private void OpenSavedDevice(long deviceId)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
                {
                    connection.Open();

                    var deviceForm = new ModbusDevice { MdiParent = this };
                    deviceForm.DeviceSaved += (s, ev) => LoadDevicesToTree();

                    // Get device info (Name, SlaveId)
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
                        else
                        {
                            throw new Exception($"Device with ID {deviceId} not found in database.");
                        }
                    }

                    // Get all reading groups for this device
                    var groupCmd = connection.CreateCommand();
                    groupCmd.CommandText = "SELECT GroupId, GroupName, FunctionCode, StartAddress, Quantity FROM ReadingGroups WHERE DeviceId = $deviceId";
                    groupCmd.Parameters.AddWithValue("$deviceId", deviceId);

                    using (var groupReader = groupCmd.ExecuteReader())
                    {
                        deviceForm.DeviceTabControl.TabPages.Clear(); // Clear default tab

                        while (groupReader.Read())
                        {
                            long groupId = groupReader.GetInt64(0);
                            string groupName = groupReader.GetString(1);
                            int funcCode = Convert.ToInt32(groupReader.GetValue(2));
                            int startAddr = Convert.ToInt32(groupReader.GetValue(3));
                            int quantity = Convert.ToInt32(groupReader.GetValue(4));

                            // Create a new tab for each group
                            ReadingsTab readingsTab = new ReadingsTab { Dock = DockStyle.Fill };
                            readingsTab.SetConfiguration(funcCode, startAddr, quantity);

                            // FIX: Subskrypcja eventu do publicznej metody w ModbusDevice
                            readingsTab.ChartDataUpdated += deviceForm.ReadingsTab_ChartDataUpdated;

                            TabPage newTab = new TabPage(groupName);
                            newTab.Controls.Add(readingsTab);
                            deviceForm.DeviceTabControl.TabPages.Add(newTab);

                            // Get register definitions for this group
                            var regCmd = connection.CreateCommand();
                            regCmd.CommandText = "SELECT RegisterNumber, RegisterName, DisplayFormatColumn FROM RegisterDefinitions WHERE GroupId = $groupId";
                            regCmd.Parameters.AddWithValue("$groupId", groupId);

                            var registers = new List<Tuple<int, string, string>>();
                            using (var regReader = regCmd.ExecuteReader())
                            {
                                while (regReader.Read())
                                {
                                    int regNum;
                                    string regNumString = regReader.GetValue(0).ToString();

                                    // Handle parsing the register number (might be "100" or "100 - 103")
                                    if (!int.TryParse(regNumString, out regNum))
                                    {
                                        string[] parts = regNumString.Split(' ');
                                        if (parts.Length > 0 && int.TryParse(parts[0], out regNum))
                                        {
                                            // Successfully parsed the first number of a range (e.g., "100" from "100 - 103")
                                        }
                                        else
                                        {
                                            continue; // Skip this register if parsing fails
                                        }
                                    }

                                    string regName = regReader.GetString(1);
                                    string regFormat = regReader.IsDBNull(2) ? "Unsigned16" : regReader.GetString(2);

                                    registers.Add(new Tuple<int, string, string>(regNum, regName, regFormat));
                                }
                            }
                            // Pass register definitions to the tab
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

        #region Drag-and-Drop Device from Tree
        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));

                // Allow copying (drag-drop) only for device nodes (Level 1)
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

                // On drop, open the saved device
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

                // Initiate drag-drop for device nodes (Level 1)
                if (node.Level == 1 && node.Tag != null)
                {
                    treeView.DoDragDrop(node, DragDropEffects.Copy);
                }
            }
        }

        private void treeView_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            // Show the custom drag cursor
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
        #endregion

        #region Import/Export/Delete Device

        /// <summary>
        /// Handles the "Import" context menu click.
        /// </summary>
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
                        LoadDevicesToTree(); // Refresh tree
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

        /// <summary>
        /// Imports a device configuration from an XML file into the database.
        /// </summary>
        private void ImportDeviceFromXml(string filePath)
        {
            XDocument doc = XDocument.Load(filePath, LoadOptions.SetLineInfo);

            XElement root = doc.Root;
            // Validate root node
            if (root.Name != "ModbusDevice")
                throw new XmlException("The file is not a valid ModbusDevice configuration file (incorrect root node).");

            // Validate attributes
            XAttribute deviceNameAttr = root.Attribute("DeviceName") ?? throw new XmlException("Missing 'DeviceName' attribute in the root node.", null, (root as IXmlLineInfo).LineNumber, (root as IXmlLineInfo).LinePosition);
            XAttribute slaveIdAttr = root.Attribute("SlaveId") ?? throw new XmlException("Missing 'SlaveId' attribute in the root node.", null, (root as IXmlLineInfo).LineNumber, (root as IXmlLineInfo).LinePosition);

            string deviceName = deviceNameAttr.Value;
            if (!int.TryParse(slaveIdAttr.Value, out int slaveId) || slaveId < 1 || slaveId > 254)
                throw new XmlException($"The 'SlaveId' attribute has an invalid value: '{slaveIdAttr.Value}'. Expected a number 1-254.", null, (slaveIdAttr as IXmlLineInfo).LineNumber, (slaveIdAttr as IXmlLineInfo).LinePosition);

            using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                connection.Open();

                // Check for device name conflicts in the database
                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Devices WHERE DeviceName = $name;";
                checkCmd.Parameters.AddWithValue("$name", deviceName);
                if ((long)checkCmd.ExecuteScalar() > 0)
                {
                    // Conflict found, ask user for a new name
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

                // Start DB transaction
                using (var transaction = connection.BeginTransaction())
                {
                    // Insert device
                    var deviceCmd = connection.CreateCommand();
                    deviceCmd.Transaction = transaction;
                    deviceCmd.CommandText = "INSERT INTO Devices (DeviceName, SlaveId) VALUES ($name, $slaveId) RETURNING DeviceId;";
                    deviceCmd.Parameters.AddWithValue("$name", deviceName);
                    deviceCmd.Parameters.AddWithValue("$slaveId", slaveId);
                    long deviceId = (long)deviceCmd.ExecuteScalar();

                    // Loop through <ReadingGroup> elements
                    foreach (var groupNode in root.Elements("ReadingGroup"))
                    {
                        // Validate group attributes
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

                        // Insert group
                        var groupCmd = connection.CreateCommand();
                        groupCmd.Transaction = transaction;
                        groupCmd.CommandText = "INSERT INTO ReadingGroups (DeviceId, GroupName, FunctionCode, StartAddress, Quantity) VALUES ($deviceId, $groupName, $funcCode, $startAddr, $quantity) RETURNING GroupId;";
                        groupCmd.Parameters.AddWithValue("$deviceId", deviceId);
                        groupCmd.Parameters.AddWithValue("$groupName", groupNameAttr.Value);
                        groupCmd.Parameters.AddWithValue("$funcCode", funcCode);
                        groupCmd.Parameters.AddWithValue("$startAddr", startAddr);
                        groupCmd.Parameters.AddWithValue("$quantity", quantity);
                        long groupId = (long)groupCmd.ExecuteScalar();

                        // Loop through <Register> elements
                        foreach (var regNode in groupNode.Elements("Register"))
                        {
                            XAttribute regNumAttr = regNode.Attribute("RegisterNumber") ?? throw new XmlException("Missing 'RegisterNumber' attribute in 'Register' node.", null, (regNode as IXmlLineInfo).LineNumber, (regNode as IXmlLineInfo).LinePosition);
                            XAttribute regNameAttr = regNode.Attribute("RegisterName") ?? throw new XmlException("Missing 'RegisterName' attribute in 'Register' node.", null, (regNode as IXmlLineInfo).LineNumber, (regNode as IXmlLineInfo).LinePosition);
                            // Note: DisplayFormat is not in the import/export schema in this version, will use default.

                            if (!int.TryParse(regNumAttr.Value, out int regNum))
                                throw new XmlException($"Invalid 'RegisterNumber': {regNumAttr.Value}.", null, (regNumAttr as IXmlLineInfo).LineNumber, (regNumAttr as IXmlLineInfo).LinePosition);

                            // Insert register
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

        /// <summary>
        /// Handles the "Export" context menu click.
        /// </summary>
        private void exportDeviceContextMenuItem_Click(object sender, EventArgs e)
        {
            // Get selected device node
            TreeNode selectedNode = treeView.SelectedNode;
            if (selectedNode == null || selectedNode.Level != 1 || selectedNode.Tag == null)
            {
                return; // Not a valid device node
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

        /// <summary>
        /// Exports a device configuration from the database to an XML file.
        /// </summary>
        private void ExportDeviceToXml(long deviceId, string filePath)
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            XElement rootNode = null;

            using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                connection.Open();

                // Create XML root node <ModbusDevice>
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

                // Get all reading groups
                var groupCmd = connection.CreateCommand();
                groupCmd.CommandText = "SELECT GroupId, GroupName, FunctionCode, StartAddress, Quantity FROM ReadingGroups WHERE DeviceId = $deviceId";
                groupCmd.Parameters.AddWithValue("$deviceId", deviceId);

                using (var groupReader = groupCmd.ExecuteReader())
                {
                    while (groupReader.Read())
                    {
                        long groupId = groupReader.GetInt64(0);
                        // Create <ReadingGroup> element
                        var groupNode = new XElement("ReadingGroup",
                                            new XAttribute("GroupName", groupReader.GetString(1)),
                                            new XAttribute("FunctionCode", groupReader.GetInt32(2)),
                                            new XAttribute("StartAddress", groupReader.GetInt32(3)),
                                            new XAttribute("Quantity", groupReader.GetInt32(4))
                                        );

                        // Get all registers for this group
                        var regCmd = connection.CreateCommand();
                        regCmd.CommandText = "SELECT RegisterNumber, RegisterName FROM RegisterDefinitions WHERE GroupId = $groupId";
                        regCmd.Parameters.AddWithValue("$groupId", groupId);

                        using (var regReader = regCmd.ExecuteReader())
                        {
                            while (regReader.Read())
                            {
                                // Create <Register> element
                                var regNode = new XElement("Register",
                                                    new XAttribute("RegisterNumber", regReader.GetInt32(0)),
                                                    new XAttribute("RegisterName", regReader.GetString(1))
                                                // Note: DisplayFormat is not exported in this version
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

        /// <summary>
        /// Handles the "Delete" context menu click.
        /// </summary>
        private void deleteDeviceContextMenuItem_Click(object sender, EventArgs e)
        {
            // Get selected device node
            TreeNode selectedNode = treeView.SelectedNode;

            if (selectedNode == null || selectedNode.Level != 1 || selectedNode.Tag == null)
            {
                return; // Not a valid device node
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
                    LoadDevicesToTree(); // Refresh tree
                    MessageBox.Show($"Device '{deviceName}' removed successfully.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the device: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Deletes a device from the DB. Relies on "ON DELETE CASCADE" constraint.
        /// </summary>
        private void DeleteDeviceFromDb(long deviceId)
        {
            using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // CASCADE DELETE in the DB schema will handle deleting related groups and registers
                command.CommandText = "DELETE FROM Devices WHERE DeviceId = $deviceId;";
                command.Parameters.AddWithValue("$deviceId", deviceId);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Selects a node on right-click to prepare it for the context menu.
        /// </summary>
        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView.SelectedNode = e.Node;
            }
        }

        /// <summary>
        /// Enables/disables context menu items based on the selected node.
        /// </summary>
        private void treeViewContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TreeNode selectedNode = treeView.SelectedNode;

            importDeviceContextMenuItem.Enabled = true; // Import is always enabled

            // Export/Delete are enabled only for device nodes (Level 1)
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
        #endregion

        /// <summary>
        /// Public method for child windows to log communication frames.
        /// </summary>
        public void LogCommunicationEvent(ModbusFrameLog logEntry)
        {
            // Pass the log entry to the (potentially null) log window
            _commsLogWindow?.LogFrame(logEntry);

            // If it's an error, notify the log window to stop if "Stop on Error" is checked
            if (logEntry.Direction == "Error")
            {
                _commsLogWindow?.NotifyCommunicationError();
            }
        }

        // --- Window layout functions ---
        private void cascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Arrange all MDI child windows in a cascade layout
            this.LayoutMdi(MdiLayout.Cascade);
        }

        private void splitHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Tile all MDI child windows horizontally
            this.LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void splitVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Tile all MDI child windows vertically
            this.LayoutMdi(MdiLayout.TileVertical);
        }

        private void numMaxRetries_ValueChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Gets the maximum number of retries from the numeric up-down control.
        /// </summary>
        public int GetMaxRetries()
        {
            // Use Invoke if called from another thread
            if (numMaxRetries.InvokeRequired)
            {
                return (int)numMaxRetries.Invoke(new Func<int>(() => (int)numMaxRetries.Value));
            }
            return (int)numMaxRetries.Value;
        }
    }
}