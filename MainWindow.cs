using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using MP_ModbusApp.MP_modbus;
using System;
using System.Collections.Generic;
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
using System.Diagnostics;

namespace MP_ModbusApp
{
    public partial class MainWindow : Form
    {
        private MP_modbus.IMyModbusMaster _modbusMaster;
        private SerialPort serialPort;
        private System.Net.Sockets.TcpClient tcpClient;
        private CommunicationLogWindow _commsLogWindow;
        private Cursor _deviceDragCursor;
        private bool sidePanelHidden = false;
        private readonly ToolTip toolTip1 = new ToolTip();

        public MP_modbus.IMyModbusMaster ModbusMaster => _modbusMaster;

        public int GetPollDelay()
        {
            return (int)numPollDelay.Value;
        }

        public MainWindow()
        {
            InitializeComponent();
            toolTip1.SetToolTip(openMenu, "Show / Hide connection settings");
            toolTip1.SetToolTip(openTree, "Show / Hide devices tree");
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.AllowDrop = true;

            // Initialize custom drag cursor for tree nodes
            try
            {
                if (imageList1.Images.Count > 1)
                {
                    Image img = imageList1.Images[1];
                    Bitmap bmp = new Bitmap(img);
                    _deviceDragCursor = new Cursor(bmp.GetHicon());
                    bmp.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to create drag cursor: " + ex.Message);
                _deviceDragCursor = Cursors.Default;
            }

            // Set initial UI state to collapsed
            CollapseSidePanel();

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

        private void CollapseSidePanel()
        {
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

        private void ExpandSidePanel(bool showSetup)
        {
            sidePanel.Width = 350;
            setupButton.Height = 50;
            openMenu.Text = "";
            openMenu.BackgroundImage = showSetup ? Properties.Resources.icons8_double_left_50 : Properties.Resources.icons8_settings_50;
            openMenu.BackgroundImageLayout = ImageLayout.Zoom;
            openMenu.Font = new Font("Segoe UI", 14.25F);
            openMenu.Height = 40;
            openMenu.Width = 40;
            openMenu.Left = 300;
            treeButton.Height = 50;
            openTree.Text = "";
            openTree.BackgroundImage = showSetup ? Properties.Resources.icons8_broadcasting_50 : Properties.Resources.icons8_double_left_50;
            openTree.BackgroundImageLayout = ImageLayout.Zoom;
            openTree.Font = new Font("Segoe UI", 14.25F);
            openTree.Height = 40;
            openTree.Width = 40;
            openTree.Left = 300;
            setupPanel.Visible = showSetup;
            treeView.Visible = !showSetup;
            sidePanelHidden = false;

            if (!showSetup) treeView.Nodes[0]?.Expand();
        }

        private void openMenu_Click(object sender, EventArgs e)
        {
            if (sidePanelHidden) ExpandSidePanel(true);
            else if (!setupPanel.Visible) ExpandSidePanel(true);
            else CollapseSidePanel();
            this.Refresh();
        }

        private void openTree_Click(object sender, EventArgs e)
        {
            if (sidePanelHidden) ExpandSidePanel(false);
            else if (!treeView.Visible) ExpandSidePanel(false);
            else CollapseSidePanel();
            this.Refresh();
        }

        #region Initialization and Settings Loading
        private void PopulateComboBoxes()
        {
            cboxConnection.Items.AddRange(new object[] { "Serial Port (RTU/ASCII)", "Modbus TCP/IP", "Modbus RTU/ASCII over TCP/IP" });
            cBoxBaudRate.Items.AddRange(new object[] { "300", "600", "1200", "2400", "4800", "9600", "14400", "19200", "38400", "56000", "57600", "115200" });
            cBoxDataBits.Items.AddRange(new object[] { "7", "8" });
            cBoxParity.Items.AddRange(Enum.GetNames(typeof(Parity)).Except(new[] { "Mark", "Space" }).ToArray());
            cBoxStopBits.Items.AddRange(Enum.GetNames(typeof(StopBits)).Except(new[] { "None", "OnePointFive" }).ToArray());
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
            numMaxRetries.Value = decimal.Parse(DatabaseHelper.LoadSetting("numMaxRetries", "0"));
            numIPConnTimeout.Value = decimal.Parse(DatabaseHelper.LoadSetting("numIPConnTimeout", "2000"));
            numIPPort.Value = decimal.Parse(DatabaseHelper.LoadSetting("numIPPort", "502"));

            if (bool.Parse(DatabaseHelper.LoadSetting("IsRTU", "true"))) rBtnRTU.Checked = true; else rBtnASCII.Checked = true;

            cboxIPAddress.Items.Clear();
            var savedIPs = DatabaseHelper.LoadIpAddresses();
            cboxIPAddress.Items.AddRange(savedIPs.ToArray());
            string lastUsedIP = DatabaseHelper.LoadSetting("LastUsedIPAddress", "127.0.0.1");
            if (cboxIPAddress.Items.Contains(lastUsedIP)) cboxIPAddress.SelectedItem = lastUsedIP;
            else cboxIPAddress.Text = lastUsedIP;
        }
        #endregion

        #region COM Port Handling
        private void RefreshComPorts()
        {
            string previouslySelected = cboxComPort.SelectedItem?.ToString();
            cboxComPort.Items.Clear();
            List<string> foundPorts = new List<string>();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ClassGuid = '{4d36e978-e325-11ce-bfc1-08002be10318}'"))
                {
                    foreach (ManagementObject device in searcher.Get())
                    {
                        string pnpDeviceId = device.GetPropertyValue("PnpDeviceID")?.ToString() ?? "";
                        string regPath = $"HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\{pnpDeviceId}\\Device Parameters";
                        string portName = Registry.GetValue(regPath, "PortName", "")?.ToString() ?? "";
                        if (string.IsNullOrEmpty(portName)) continue;

                        string caption = device.GetPropertyValue("Caption")?.ToString() ?? "";
                        string status = device.GetPropertyValue("Status")?.ToString() ?? "Unknown";
                        string friendlyName = Registry.GetValue(regPath, "FriendlyName", "")?.ToString() ?? "";

                        string interfaceType = "";
                        if (friendlyName.Contains("MOXA") || caption.Contains("MOXA"))
                        {
                            object serVal = Registry.GetValue(regPath, "SerInterface", null);
                            if (serVal != null && int.TryParse(serVal.ToString(), out int pInt))
                            {
                                switch (pInt)
                                {
                                    case 0: interfaceType = " (RS-232)"; break;
                                    case 1: interfaceType = " (RS-422)"; break;
                                    case 2: interfaceType = " (RS-485 2W)"; break;
                                    case 3: interfaceType = " (RS-485 4W)"; break;
                                }
                            }
                        }

                        int comIdx = caption.IndexOf(" (COM");
                        if (comIdx > 0) caption = caption.Substring(0, comIdx);

                        string finalDisplayName = (status != "OK" ? "⚠ " : "") + $"{portName} - {caption}{interfaceType}";
                        foundPorts.Add(finalDisplayName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WMI Query failed: {ex.Message}");
                foundPorts.AddRange(SerialPort.GetPortNames());
            }

            var sortedPorts = foundPorts.OrderBy(x =>
            {
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
                int idx = previouslySelected != null ? cboxComPort.FindStringExact(previouslySelected) : -1;
                cboxComPort.SelectedIndex = (idx != -1) ? idx : 0;
            }
        }

        private void btnRefreshCom_Click(object sender, EventArgs e) => RefreshComPorts();

        private void CboxComPort_DrawItem(object sender, DrawItemEventArgs e)
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
                else
                {
                    MessageBox.Show($"'{address}' is not a valid IP address or hostname.", "Invalid Address", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private async Task<bool> IsValidIpOrHostname(string address)
        {
            if (IPAddress.TryParse(address, out _)) return true;
            try
            {
                var host = await Dns.GetHostEntryAsync(address);
                return host.AddressList.Any();
            }
            catch { return false; }
        }

        private void Setting_Changed(object sender, EventArgs e)
        {
            if (sender is ComboBox cb) DatabaseHelper.SaveSetting(cb.Name, cb.SelectedIndex.ToString());
            else if (sender is NumericUpDown nud) DatabaseHelper.SaveSetting(nud.Name, nud.Value.ToString());
            else if (sender is RadioButton rb && rb.Checked) DatabaseHelper.SaveSetting("IsRTU", rBtnRTU.Checked.ToString());
        }
        #endregion

        #region Connection Handling
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            string connectionPort = "";
            Cursor.Current = Cursors.WaitCursor;
            btnConnect.Enabled = false;
            btnConnect.Text = "Connecting...";

            try
            {
                if (_commsLogWindow == null || _commsLogWindow.IsDisposed)
                {
                    _commsLogWindow = new CommunicationLogWindow();
                    _commsLogWindow.MdiParent = this;
                }

                IMyModbusTransport transport;
                if (cboxConnection.SelectedIndex == 0)
                {
                    string fullPortName = cboxComPort.SelectedItem.ToString();
                    string portName = fullPortName.Split(' ')[0];
                    if (portName == "⚠") portName = fullPortName.Split(' ')[1];

                    int baud = int.Parse(cBoxBaudRate.SelectedItem.ToString());
                    int bits = int.Parse(cBoxDataBits.SelectedItem.ToString());
                    Parity parity = Enum.Parse<Parity>(cBoxParity.SelectedItem.ToString());
                    StopBits stop = Enum.Parse<StopBits>(cBoxStopBits.SelectedItem.ToString());

                    serialPort = new SerialPort(portName, baud, parity, bits, stop);
                    serialPort.Open();
                    transport = new MyModbusSerialTransport(serialPort, rBtnRTU.Checked, this);
                    connectionPort = $"{portName}@{baud}";
                }
                else
                {
                    tcpClient = new System.Net.Sockets.TcpClient();
                    var connectTask = tcpClient.ConnectAsync(cboxIPAddress.Text, (int)numIPPort.Value);
                    if (await Task.WhenAny(connectTask, Task.Delay((int)numIPConnTimeout.Value)) != connectTask)
                        throw new TimeoutException("Connection timed out.");

                    await connectTask;

                    if (cboxConnection.SelectedIndex == 1) transport = new MyModbusTcpTransport(tcpClient, this);
                    else transport = new MyModbusTcpSerialTransport(tcpClient, rBtnRTU.Checked, this);

                    connectionPort = $"{cboxIPAddress.Text}:{numIPPort.Value}";
                }

                transport.ReadTimeout = (int)numResponseTimeout.Value;
                transport.WriteTimeout = (int)numResponseTimeout.Value;
                _modbusMaster = new MyModbusMaster(transport);

                UpdateUiState(true);
                toolStripStatusLabel1.Text = "Connected: " + connectionPort;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Disconnect();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                btnConnect.Text = "Connect";
                if (_modbusMaster == null) btnConnect.Enabled = true;
            }
        }

        private void Disconnect()
        {
            foreach (Form child in this.MdiChildren)
                if (child is ModbusDevice device) device.StopPolling(2);

            _modbusMaster?.Dispose();
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
                _commsLogWindow = new CommunicationLogWindow { MdiParent = this };
            }
            _commsLogWindow.Show();
            _commsLogWindow.Activate();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var device = new ModbusDevice { MdiParent = this };
            device.DeviceSaved += (s, ev) => LoadDevicesToTree();
            device.Show();
        }

        private void LoadDevicesToTree()
        {
            TreeNode root = treeView.Nodes[0];
            if (root == null) return;
            root.Nodes.Clear();

            try
            {
                using (var conn = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT DeviceId, DeviceName FROM Devices ORDER BY DeviceName";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long id = reader.GetInt64(0);
                            TreeNode devNode = new TreeNode(reader.GetString(1)) { ImageIndex = 1, SelectedImageIndex = 1, Tag = id };
                            root.Nodes.Add(devNode);

                            var gCmd = conn.CreateCommand();
                            gCmd.CommandText = "SELECT GroupId, GroupName FROM ReadingGroups WHERE DeviceId = $id";
                            gCmd.Parameters.AddWithValue("$id", id);
                            using (var gReader = gCmd.ExecuteReader())
                            {
                                while (gReader.Read())
                                {
                                    devNode.Nodes.Add(new TreeNode(gReader.GetString(1)) { ImageIndex = 2, SelectedImageIndex = 2, Tag = gReader.GetInt64(0) });
                                }
                            }
                        }
                    }
                }
                root.Expand();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 1 && e.Node.Tag != null) OpenSavedDevice((long)e.Node.Tag);
        }

        private void OpenSavedDevice(long deviceId)
        {
            try
            {
                using (var conn = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
                {
                    conn.Open();
                    var deviceForm = new ModbusDevice { MdiParent = this };
                    deviceForm.DeviceSaved += (s, ev) => LoadDevicesToTree();

                    var dCmd = conn.CreateCommand();
                    dCmd.CommandText = "SELECT DeviceName, SlaveId FROM Devices WHERE DeviceId = $id";
                    dCmd.Parameters.AddWithValue("$id", deviceId);

                    using (var dReader = dCmd.ExecuteReader())
                    {
                        if (dReader.Read())
                        {
                            deviceForm.Text = dReader.GetString(0);
                            deviceForm.DeviceName = dReader.GetString(0);
                            deviceForm.SlaveId = Convert.ToInt32(dReader.GetValue(1));
                        }
                        else return;
                    }

                    var gCmd = conn.CreateCommand();
                    gCmd.CommandText = "SELECT GroupId, GroupName, FunctionCode, StartAddress, Quantity FROM ReadingGroups WHERE DeviceId = $id";
                    gCmd.Parameters.AddWithValue("$id", deviceId);

                    using (var gReader = gCmd.ExecuteReader())
                    {
                        deviceForm.DeviceTabControl.TabPages.Clear();
                        while (gReader.Read())
                        {
                            long gId = gReader.GetInt64(0);
                            ReadingsTab tab = new ReadingsTab { Dock = DockStyle.Fill };
                            tab.SetConfiguration(Convert.ToInt32(gReader.GetValue(2)), Convert.ToInt32(gReader.GetValue(3)), Convert.ToInt32(gReader.GetValue(4)));
                            tab.ChartDataUpdated += deviceForm.ReadingsTab_ChartDataUpdated;

                            TabPage tp = new TabPage(gReader.GetString(1));
                            tp.Controls.Add(tab);
                            deviceForm.DeviceTabControl.TabPages.Add(tp);

                            var rCmd = conn.CreateCommand();
                            rCmd.CommandText = "SELECT RegisterNumber, RegisterName, RegisterDescription, DisplayFormatColumn FROM RegisterDefinitions WHERE GroupId = $gId";
                            rCmd.Parameters.AddWithValue("$gId", gId);

                            var regs = new List<Tuple<int, string, string, string>>();
                            using (var rReader = rCmd.ExecuteReader())
                            {
                                while (rReader.Read())
                                {
                                    if (int.TryParse(rReader.GetValue(0).ToString().Split(' ')[0], out int rNum))
                                        regs.Add(new Tuple<int, string, string, string>(rNum, rReader.GetString(1), rReader.GetString(2), rReader.IsDBNull(3) ? "Unsigned16" : rReader.GetString(3)));
                                }
                            }
                            tab.SetRegisterDefinitions(regs);
                        }
                    }
                    deviceForm.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load device: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Drag-and-Drop
        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)) && ((TreeNode)e.Data.GetData(typeof(TreeNode))).Level == 1)
                e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;
        }

        private void MainWindow_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
                if (node.Level == 1) OpenSavedDevice((long)node.Tag);
            }
        }

        private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left && ((TreeNode)e.Item).Level == 1)
                treeView.DoDragDrop(e.Item, DragDropEffects.Copy);
        }

        private void treeView_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effect == DragDropEffects.Copy)
            {
                e.UseDefaultCursors = false;
                Cursor.Current = _deviceDragCursor;
            }
            else e.UseDefaultCursors = true;
        }
        #endregion

        #region Import/Export/Delete
        private void importDeviceContextMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ImportDeviceFromXml(ofd.FileName);
                        LoadDevicesToTree();
                        MessageBox.Show("Device imported successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Import error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ImportDeviceFromXml(string path)
        {
            XDocument doc = XDocument.Load(path);
            XElement root = doc.Root;
            if (root.Name != "ModbusDevice") throw new Exception("Invalid configuration file.");

            string name = root.Attribute("DeviceName").Value;
            int slave = int.Parse(root.Attribute("SlaveId").Value);

            using (var conn = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                conn.Open();
                var check = conn.CreateCommand();
                check.CommandText = "SELECT COUNT(*) FROM Devices WHERE DeviceName = $n";
                check.Parameters.AddWithValue("$n", name);

                if ((long)check.ExecuteScalar() > 0)
                {
                    using (RenameForm rf = new RenameForm { newName = name })
                    {
                        if (rf.ShowDialog() == DialogResult.OK) name = rf.newName;
                        else throw new OperationCanceledException();
                    }
                }

                using (var trans = conn.BeginTransaction())
                {
                    var dCmd = conn.CreateCommand();
                    dCmd.Transaction = trans;
                    dCmd.CommandText = "INSERT INTO Devices (DeviceName, SlaveId) VALUES ($n, $s) RETURNING DeviceId";
                    dCmd.Parameters.AddWithValue("$n", name);
                    dCmd.Parameters.AddWithValue("$s", slave);
                    long devId = (long)dCmd.ExecuteScalar();

                    foreach (var gNode in root.Elements("ReadingGroup"))
                    {
                        var gCmd = conn.CreateCommand();
                        gCmd.Transaction = trans;
                        gCmd.CommandText = "INSERT INTO ReadingGroups (DeviceId, GroupName, FunctionCode, StartAddress, Quantity) VALUES ($d, $n, $f, $s, $q) RETURNING GroupId";
                        gCmd.Parameters.AddWithValue("$d", devId);
                        gCmd.Parameters.AddWithValue("$n", gNode.Attribute("GroupName").Value);
                        gCmd.Parameters.AddWithValue("$f", int.Parse(gNode.Attribute("FunctionCode").Value));
                        gCmd.Parameters.AddWithValue("$s", int.Parse(gNode.Attribute("StartAddress").Value));
                        gCmd.Parameters.AddWithValue("$q", int.Parse(gNode.Attribute("Quantity").Value));
                        long gId = (long)gCmd.ExecuteScalar();

                        foreach (var rNode in gNode.Elements("Register"))
                        {
                            var rCmd = conn.CreateCommand();
                            rCmd.Transaction = trans;
                            rCmd.CommandText = "INSERT INTO RegisterDefinitions (GroupId, RegisterNumber, RegisterName, RegisterDescription, DisplayFormatColumn) VALUES ($g, $rn, $name, $description, $format)";
                            rCmd.Parameters.AddWithValue("$g", gId);
                            rCmd.Parameters.AddWithValue("$rn", int.Parse(rNode.Attribute("Number").Value));
                            rCmd.Parameters.AddWithValue("$name", rNode.Attribute("Name").Value);
                            rCmd.Parameters.AddWithValue("$description", rNode.Attribute("Description").Value);
                            rCmd.Parameters.AddWithValue("$format", rNode.Attribute("Format").Value);
                            rCmd.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();
                }
            }
        }

        private void exportDeviceContextMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Level != 1) return;
            long id = (long)treeView.SelectedNode.Tag;
            string name = treeView.SelectedNode.Text;

            using (SaveFileDialog sfd = new SaveFileDialog { FileName = $"{name}.xml", Filter = "XML Files (*.xml)|*.xml" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ExportDeviceToXml(id, sfd.FileName);
                    MessageBox.Show("Device exported successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ExportDeviceToXml(long id, string path)
        {
            using (var conn = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                conn.Open();
                var dCmd = conn.CreateCommand();
                dCmd.CommandText = "SELECT DeviceName, SlaveId FROM Devices WHERE DeviceId = $id";
                dCmd.Parameters.AddWithValue("$id", id);

                XElement root;
                using (var dr = dCmd.ExecuteReader())
                {
                    if (dr.Read()) root = new XElement("ModbusDevice", new XAttribute("DeviceName", dr.GetString(0)), new XAttribute("SlaveId", dr.GetInt32(1)));
                    else return;
                }

                var gCmd = conn.CreateCommand();
                gCmd.CommandText = "SELECT GroupId, GroupName, FunctionCode, StartAddress, Quantity FROM ReadingGroups WHERE DeviceId = $id";
                gCmd.Parameters.AddWithValue("$id", id);
                using (var gr = gCmd.ExecuteReader())
                {
                    while (gr.Read())
                    {
                        long gId = gr.GetInt64(0);
                        XElement gNode = new XElement("ReadingGroup", new XAttribute("GroupName", gr.GetString(1)), new XAttribute("FunctionCode", gr.GetInt32(2)), new XAttribute("StartAddress", gr.GetInt32(3)), new XAttribute("Quantity", gr.GetInt32(4)));

                        var rCmd = conn.CreateCommand();
                        rCmd.CommandText = "SELECT RegisterNumber, RegisterName, RegisterDescription, DisplayFormatColumn FROM RegisterDefinitions WHERE GroupId = $gId";
                        rCmd.Parameters.AddWithValue("$gId", gId);
                        using (var rr = rCmd.ExecuteReader())
                        {
                            while (rr.Read()) gNode.Add(new XElement("Register", new XAttribute("Number", rr.GetInt32(0)), new XAttribute("Name", rr.GetString(1)), new XAttribute("Description", rr.GetString(2)), new XAttribute("Format", rr.GetString(3))));
                        }
                        root.Add(gNode);
                    }
                }
                new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root).Save(path);
            }
        }

        private void deleteDeviceContextMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Level != 1) return;
            long id = (long)treeView.SelectedNode.Tag;
            string name = treeView.SelectedNode.Text;

            if (MessageBox.Show($"Delete device '{name}'? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (var conn = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM Devices WHERE DeviceId = $id";
                    cmd.Parameters.AddWithValue("$id", id);
                    cmd.ExecuteNonQuery();
                }
                LoadDevicesToTree();
            }
        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right) treeView.SelectedNode = e.Node;
        }

        private void treeViewContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool isDev = treeView.SelectedNode?.Level == 1;
            exportDeviceContextMenuItem.Enabled = isDev;
            deleteDeviceContextMenuItem.Enabled = isDev;
        }
        #endregion

        public void LogCommunicationEvent(ModbusFrameLog entry)
        {
            _commsLogWindow?.LogFrame(entry);
            if (entry.Direction == "Error") _commsLogWindow?.NotifyCommunicationError();
        }

        private void cascadeToolStripMenuItem_Click(object sender, EventArgs e) => this.LayoutMdi(MdiLayout.Cascade);
        private void splitHorizontalToolStripMenuItem_Click(object sender, EventArgs e) => this.LayoutMdi(MdiLayout.TileHorizontal);
        private void splitVerticalToolStripMenuItem_Click(object sender, EventArgs e) => this.LayoutMdi(MdiLayout.TileVertical);

        public int GetMaxRetries()
        {
            if (numMaxRetries.InvokeRequired) return (int)numMaxRetries.Invoke(new Func<int>(() => (int)numMaxRetries.Value));
            return (int)numMaxRetries.Value;
        }

        private void slaveScanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeviceScan scan = new DeviceScan(_modbusMaster) { MdiParent = this };
            scan.ScanningStateChanged += (s, isScanning) =>
            {
                foreach (Form child in this.MdiChildren)
                    if (child is ModbusDevice device) { if (isScanning) device.StopPolling(2); else device.StartPolling(); }
            };
            scan.Show();
        }

        private void addresScanToolStripMenuItem_Click(object sender, EventArgs e)
        {

            AddressScan scanForm = new AddressScan(this.ModbusMaster);

            scanForm.ScanningStateChanged += (s, isScanning) =>
            {
                if (isScanning)
                {
                    foreach (var child in this.MdiChildren)
                    {
                        if (child is ModbusDevice dev) dev.StopPolling(1);
                    }
                }
                else
                {
                    foreach (var child in this.MdiChildren)
                    {
                        if (child is ModbusDevice dev) dev.StartPolling();
                    }
                }
            };

            scanForm.MdiParent = this;
            scanForm.Show();

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            {
                aboutBox.ShowDialog();
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string readmePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "README.md");

            try
            {
                // Uruchamiamy proces "notepad.exe" i jako argument podajemy ścieżkę do pliku
                Process.Start("notepad.exe", readmePath);
            }
            catch (Exception ex)
            {
            }

        }

        private void renameDeviceContextMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Level != 1 || treeView.SelectedNode.Tag == null) return;

            long deviceId = (long)treeView.SelectedNode.Tag;
            string oldName = treeView.SelectedNode.Text;

            using (RenameForm renameDialog = new RenameForm() { Text = "Rename Device", newName = oldName })
            {
                if (renameDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(renameDialog.newName))
                {
                    string newName = renameDialog.newName.Trim();

                    if (newName == oldName) return;

                    try
                    {
                        using (var conn = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
                        {
                            conn.Open();
                            var cmd = conn.CreateCommand();
                            cmd.CommandText = "UPDATE Devices SET DeviceName = $newName WHERE DeviceId = $id";
                            cmd.Parameters.AddWithValue("$newName", newName);
                            cmd.Parameters.AddWithValue("$id", deviceId);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                treeView.SelectedNode.Text = newName;

                                foreach (Form child in this.MdiChildren)
                                {
                                    if (child is ModbusDevice devForm && devForm.Text == oldName)
                                    {
                                        devForm.Text = newName;
                                        devForm.DeviceName = newName;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update database: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            treeView.SelectedNode = null;

        }
    }
}