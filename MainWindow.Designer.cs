using Microsoft.VisualBasic.Devices;

namespace MP_ModbusApp
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            TreeNode treeNode1 = new TreeNode("Group1", 2, 2);
            TreeNode treeNode2 = new TreeNode("Device1", 1, 1, new TreeNode[] { treeNode1 });
            TreeNode treeNode3 = new TreeNode("Devices List", 0, 0, new TreeNode[] { treeNode2 });
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            sidePanel = new Panel();
            treeView = new TreeView();
            treeViewContextMenu = new ContextMenuStrip(components);
            importDeviceContextMenuItem = new ToolStripMenuItem();
            exportDeviceContextMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            deleteDeviceContextMenuItem = new ToolStripMenuItem();
            imageList1 = new ImageList(components);
            setupPanel = new Panel();
            btnConnect = new Button();
            gBoxGlobalSettings = new GroupBox();
            numMaxRetries = new NumericUpDown();
            numPollDelay = new NumericUpDown();
            numResponseTimeout = new NumericUpDown();
            gBoxSerialMode = new GroupBox();
            rBtnASCII = new RadioButton();
            rBtnRTU = new RadioButton();
            label2 = new Label();
            label13 = new Label();
            label1 = new Label();
            gboxIPSettings = new GroupBox();
            label9 = new Label();
            rbtnIPv6 = new RadioButton();
            rbtnIPv4 = new RadioButton();
            label8 = new Label();
            numIPConnTimeout = new NumericUpDown();
            label7 = new Label();
            numIPPort = new NumericUpDown();
            label6 = new Label();
            cboxIPAddress = new ComboBox();
            gboxSerialSettings = new GroupBox();
            label5 = new Label();
            label10 = new Label();
            btnRefreshCom = new Button();
            label11 = new Label();
            label12 = new Label();
            cBoxStopBits = new ComboBox();
            cBoxParity = new ComboBox();
            cBoxDataBits = new ComboBox();
            cBoxBaudRate = new ComboBox();
            cboxComPort = new ComboBox();
            btnDisconnect = new Button();
            gboxConnection = new GroupBox();
            cboxConnection = new ComboBox();
            treeButton = new Panel();
            label4 = new Label();
            openTree = new Button();
            setupButton = new Panel();
            label3 = new Label();
            openMenu = new Button();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newToolStripMenuItem = new ToolStripMenuItem();
            slaveScanToolStripMenuItem = new ToolStripMenuItem();
            addresScanToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            communicationToolStripMenuItem = new ToolStripMenuItem();
            layoutToolStripMenuItem = new ToolStripMenuItem();
            cascadeToolStripMenuItem = new ToolStripMenuItem();
            splitHorizontalToolStripMenuItem = new ToolStripMenuItem();
            splitVerticalToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            helpProvider1 = new HelpProvider();
            toolStripSeparator2 = new ToolStripSeparator();
            renameDeviceContextMenuItem = new ToolStripMenuItem();
            sidePanel.SuspendLayout();
            treeViewContextMenu.SuspendLayout();
            setupPanel.SuspendLayout();
            gBoxGlobalSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxRetries).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPollDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numResponseTimeout).BeginInit();
            gBoxSerialMode.SuspendLayout();
            gboxIPSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numIPConnTimeout).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numIPPort).BeginInit();
            gboxSerialSettings.SuspendLayout();
            gboxConnection.SuspendLayout();
            treeButton.SuspendLayout();
            setupButton.SuspendLayout();
            statusStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // sidePanel
            // 
            sidePanel.BackColor = SystemColors.ButtonFace;
            sidePanel.Controls.Add(treeView);
            sidePanel.Controls.Add(setupPanel);
            sidePanel.Controls.Add(treeButton);
            sidePanel.Controls.Add(setupButton);
            sidePanel.Dock = DockStyle.Left;
            sidePanel.Location = new Point(0, 0);
            sidePanel.Name = "sidePanel";
            sidePanel.Size = new Size(350, 598);
            sidePanel.TabIndex = 0;
            // 
            // treeView
            // 
            treeView.BackColor = SystemColors.ButtonFace;
            treeView.BorderStyle = BorderStyle.None;
            treeView.ContextMenuStrip = treeViewContextMenu;
            treeView.Dock = DockStyle.Fill;
            treeView.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            treeView.ImageIndex = 0;
            treeView.ImageList = imageList1;
            treeView.Location = new Point(0, 539);
            treeView.Name = "treeView";
            treeNode1.ImageIndex = 2;
            treeNode1.Name = "Group1";
            treeNode1.SelectedImageIndex = 2;
            treeNode1.StateImageIndex = 2;
            treeNode1.Text = "Group1";
            treeNode2.ImageIndex = 1;
            treeNode2.Name = "Device1";
            treeNode2.SelectedImageIndex = 1;
            treeNode2.StateImageIndex = 1;
            treeNode2.Text = "Device1";
            treeNode3.ImageIndex = 0;
            treeNode3.Name = "Saved_Devices";
            treeNode3.SelectedImageIndex = 0;
            treeNode3.StateImageIndex = 0;
            treeNode3.Text = "Devices List";
            treeView.Nodes.AddRange(new TreeNode[] { treeNode3 });
            treeView.SelectedImageIndex = 0;
            treeView.Size = new Size(350, 59);
            treeView.TabIndex = 8;
            treeView.ItemDrag += treeView_ItemDrag;
            treeView.NodeMouseClick += treeView_NodeMouseClick;
            treeView.NodeMouseDoubleClick += treeView_NodeMouseDoubleClick;
            treeView.GiveFeedback += treeView_GiveFeedback;
            // 
            // treeViewContextMenu
            // 
            treeViewContextMenu.Items.AddRange(new ToolStripItem[] { importDeviceContextMenuItem, exportDeviceContextMenuItem, toolStripSeparator2, renameDeviceContextMenuItem, toolStripSeparator1, deleteDeviceContextMenuItem });
            treeViewContextMenu.Name = "contextMenuStrip1";
            treeViewContextMenu.Size = new Size(181, 126);
            treeViewContextMenu.Opening += treeViewContextMenu_Opening;
            // 
            // importDeviceContextMenuItem
            // 
            importDeviceContextMenuItem.Name = "importDeviceContextMenuItem";
            importDeviceContextMenuItem.ShortcutKeys = Keys.Control | Keys.I;
            importDeviceContextMenuItem.Size = new Size(180, 22);
            importDeviceContextMenuItem.Text = "Import";
            importDeviceContextMenuItem.Click += importDeviceContextMenuItem_Click;
            // 
            // exportDeviceContextMenuItem
            // 
            exportDeviceContextMenuItem.Name = "exportDeviceContextMenuItem";
            exportDeviceContextMenuItem.ShortcutKeys = Keys.Control | Keys.E;
            exportDeviceContextMenuItem.Size = new Size(180, 22);
            exportDeviceContextMenuItem.Text = "Export";
            exportDeviceContextMenuItem.Click += exportDeviceContextMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(177, 6);
            // 
            // deleteDeviceContextMenuItem
            // 
            deleteDeviceContextMenuItem.Name = "deleteDeviceContextMenuItem";
            deleteDeviceContextMenuItem.ShortcutKeys = Keys.Delete;
            deleteDeviceContextMenuItem.Size = new Size(180, 22);
            deleteDeviceContextMenuItem.Text = "Remove";
            deleteDeviceContextMenuItem.Click += deleteDeviceContextMenuItem_Click;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "icons8-networking-manager-50.png");
            imageList1.Images.SetKeyName(1, "icons8-individual-server-50.png");
            imageList1.Images.SetKeyName(2, "icons8-circle-30.png");
            imageList1.Images.SetKeyName(3, "icons8-connected-50.png");
            imageList1.Images.SetKeyName(4, "icons8-disconnected-50.png");
            imageList1.Images.SetKeyName(5, "icons8-database-50.png");
            imageList1.Images.SetKeyName(6, "icons8-voltmeter-50.png");
            // 
            // setupPanel
            // 
            setupPanel.Controls.Add(btnConnect);
            setupPanel.Controls.Add(gBoxGlobalSettings);
            setupPanel.Controls.Add(gboxIPSettings);
            setupPanel.Controls.Add(gboxSerialSettings);
            setupPanel.Controls.Add(btnDisconnect);
            setupPanel.Controls.Add(gboxConnection);
            setupPanel.Dock = DockStyle.Top;
            setupPanel.Location = new Point(0, 100);
            setupPanel.Name = "setupPanel";
            setupPanel.Size = new Size(350, 439);
            setupPanel.TabIndex = 1;
            // 
            // btnConnect
            // 
            btnConnect.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnConnect.ImageAlign = ContentAlignment.MiddleLeft;
            btnConnect.ImageKey = "icons8-connected-50.png";
            btnConnect.ImageList = imageList1;
            btnConnect.Location = new Point(204, 405);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(116, 32);
            btnConnect.TabIndex = 6;
            btnConnect.Text = "Connect   ";
            btnConnect.TextAlign = ContentAlignment.MiddleRight;
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // gBoxGlobalSettings
            // 
            gBoxGlobalSettings.Controls.Add(numMaxRetries);
            gBoxGlobalSettings.Controls.Add(numPollDelay);
            gBoxGlobalSettings.Controls.Add(numResponseTimeout);
            gBoxGlobalSettings.Controls.Add(gBoxSerialMode);
            gBoxGlobalSettings.Controls.Add(label2);
            gBoxGlobalSettings.Controls.Add(label13);
            gBoxGlobalSettings.Controls.Add(label1);
            gBoxGlobalSettings.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gBoxGlobalSettings.Location = new Point(5, 238);
            gBoxGlobalSettings.Name = "gBoxGlobalSettings";
            gBoxGlobalSettings.Size = new Size(340, 161);
            gBoxGlobalSettings.TabIndex = 5;
            gBoxGlobalSettings.TabStop = false;
            gBoxGlobalSettings.Text = "Global settings";
            // 
            // numMaxRetries
            // 
            numMaxRetries.Location = new Point(126, 128);
            numMaxRetries.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            numMaxRetries.Name = "numMaxRetries";
            numMaxRetries.Size = new Size(92, 23);
            numMaxRetries.TabIndex = 4;
            // 
            // numPollDelay
            // 
            numPollDelay.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            numPollDelay.Location = new Point(126, 84);
            numPollDelay.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numPollDelay.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            numPollDelay.Name = "numPollDelay";
            numPollDelay.Size = new Size(92, 23);
            numPollDelay.TabIndex = 4;
            numPollDelay.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            numPollDelay.ValueChanged += Setting_Changed;
            // 
            // numResponseTimeout
            // 
            numResponseTimeout.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            numResponseTimeout.Location = new Point(126, 40);
            numResponseTimeout.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numResponseTimeout.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            numResponseTimeout.Name = "numResponseTimeout";
            numResponseTimeout.Size = new Size(92, 23);
            numResponseTimeout.TabIndex = 3;
            numResponseTimeout.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            numResponseTimeout.ValueChanged += Setting_Changed;
            // 
            // gBoxSerialMode
            // 
            gBoxSerialMode.Controls.Add(rBtnASCII);
            gBoxSerialMode.Controls.Add(rBtnRTU);
            gBoxSerialMode.Location = new Point(6, 22);
            gBoxSerialMode.Name = "gBoxSerialMode";
            gBoxSerialMode.Size = new Size(111, 70);
            gBoxSerialMode.TabIndex = 0;
            gBoxSerialMode.TabStop = false;
            gBoxSerialMode.Text = "Mode";
            // 
            // rBtnASCII
            // 
            rBtnASCII.AutoSize = true;
            rBtnASCII.Location = new Point(3, 44);
            rBtnASCII.Name = "rBtnASCII";
            rBtnASCII.Size = new Size(55, 19);
            rBtnASCII.TabIndex = 1;
            rBtnASCII.Text = "ASCII";
            rBtnASCII.UseVisualStyleBackColor = true;
            rBtnASCII.CheckedChanged += Setting_Changed;
            // 
            // rBtnRTU
            // 
            rBtnRTU.AutoSize = true;
            rBtnRTU.Checked = true;
            rBtnRTU.Location = new Point(3, 19);
            rBtnRTU.Name = "rBtnRTU";
            rBtnRTU.Size = new Size(49, 19);
            rBtnRTU.TabIndex = 0;
            rBtnRTU.TabStop = true;
            rBtnRTU.Text = "RTU";
            rBtnRTU.UseVisualStyleBackColor = true;
            rBtnRTU.CheckedChanged += Setting_Changed;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(126, 22);
            label2.Name = "label2";
            label2.Size = new Size(135, 15);
            label2.TabIndex = 1;
            label2.Text = "Response timeout [ms]";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(124, 110);
            label13.Name = "label13";
            label13.Size = new Size(123, 15);
            label13.TabIndex = 2;
            label13.Text = "Max retries on error:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(126, 66);
            label1.Name = "label1";
            label1.Size = new Size(146, 15);
            label1.TabIndex = 2;
            label1.Text = "Delay between polls [ms]";
            // 
            // gboxIPSettings
            // 
            gboxIPSettings.Controls.Add(label9);
            gboxIPSettings.Controls.Add(rbtnIPv6);
            gboxIPSettings.Controls.Add(rbtnIPv4);
            gboxIPSettings.Controls.Add(label8);
            gboxIPSettings.Controls.Add(numIPConnTimeout);
            gboxIPSettings.Controls.Add(label7);
            gboxIPSettings.Controls.Add(numIPPort);
            gboxIPSettings.Controls.Add(label6);
            gboxIPSettings.Controls.Add(cboxIPAddress);
            gboxIPSettings.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gboxIPSettings.Location = new Point(5, 62);
            gboxIPSettings.Name = "gboxIPSettings";
            gboxIPSettings.Size = new Size(340, 171);
            gboxIPSettings.TabIndex = 4;
            gboxIPSettings.TabStop = false;
            gboxIPSettings.Text = "TCP/IP settings";
            gboxIPSettings.Visible = false;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Enabled = false;
            label9.Location = new Point(6, 146);
            label9.Name = "label9";
            label9.Size = new Size(208, 15);
            label9.TabIndex = 6;
            label9.Text = "IP protocol version ...............................";
            label9.Visible = false;
            // 
            // rbtnIPv6
            // 
            rbtnIPv6.AutoSize = true;
            rbtnIPv6.Enabled = false;
            rbtnIPv6.Location = new Point(281, 140);
            rbtnIPv6.Name = "rbtnIPv6";
            rbtnIPv6.Size = new Size(50, 19);
            rbtnIPv6.TabIndex = 1;
            rbtnIPv6.Text = "IPv6";
            rbtnIPv6.UseVisualStyleBackColor = true;
            rbtnIPv6.Visible = false;
            rbtnIPv6.CheckedChanged += Setting_Changed;
            // 
            // rbtnIPv4
            // 
            rbtnIPv4.AutoSize = true;
            rbtnIPv4.Checked = true;
            rbtnIPv4.Enabled = false;
            rbtnIPv4.Location = new Point(220, 140);
            rbtnIPv4.Name = "rbtnIPv4";
            rbtnIPv4.Size = new Size(50, 19);
            rbtnIPv4.TabIndex = 0;
            rbtnIPv4.TabStop = true;
            rbtnIPv4.Text = "IPv4";
            rbtnIPv4.UseVisualStyleBackColor = true;
            rbtnIPv4.Visible = false;
            rbtnIPv4.CheckedChanged += Setting_Changed;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(6, 117);
            label8.Name = "label8";
            label8.Size = new Size(208, 15);
            label8.TabIndex = 5;
            label8.Text = "Connection timeout [ms] ....................";
            // 
            // numIPConnTimeout
            // 
            numIPConnTimeout.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            numIPConnTimeout.Location = new Point(220, 109);
            numIPConnTimeout.Maximum = new decimal(new int[] { 30000, 0, 0, 0 });
            numIPConnTimeout.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            numIPConnTimeout.Name = "numIPConnTimeout";
            numIPConnTimeout.Size = new Size(111, 23);
            numIPConnTimeout.TabIndex = 4;
            numIPConnTimeout.Value = new decimal(new int[] { 3000, 0, 0, 0 });
            numIPConnTimeout.ValueChanged += Setting_Changed;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(6, 88);
            label7.Name = "label7";
            label7.Size = new Size(210, 15);
            label7.TabIndex = 3;
            label7.Text = "Server port .............................................";
            // 
            // numIPPort
            // 
            numIPPort.Location = new Point(220, 80);
            numIPPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numIPPort.Name = "numIPPort";
            numIPPort.Size = new Size(111, 23);
            numIPPort.TabIndex = 2;
            numIPPort.Value = new decimal(new int[] { 502, 0, 0, 0 });
            numIPPort.ValueChanged += Setting_Changed;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(6, 30);
            label6.Name = "label6";
            label6.Size = new Size(136, 15);
            label6.TabIndex = 1;
            label6.Text = "IP address / Host Name";
            // 
            // cboxIPAddress
            // 
            cboxIPAddress.FormattingEnabled = true;
            cboxIPAddress.Location = new Point(6, 51);
            cboxIPAddress.Name = "cboxIPAddress";
            cboxIPAddress.Size = new Size(325, 23);
            cboxIPAddress.Sorted = true;
            cboxIPAddress.TabIndex = 0;
            cboxIPAddress.Text = "127.0.0.1";
            cboxIPAddress.SelectedIndexChanged += CboxIPAddress_SelectedIndexChanged;
            cboxIPAddress.KeyDown += CboxIPAddress_KeyDown;
            // 
            // gboxSerialSettings
            // 
            gboxSerialSettings.Controls.Add(label5);
            gboxSerialSettings.Controls.Add(label10);
            gboxSerialSettings.Controls.Add(btnRefreshCom);
            gboxSerialSettings.Controls.Add(label11);
            gboxSerialSettings.Controls.Add(label12);
            gboxSerialSettings.Controls.Add(cBoxStopBits);
            gboxSerialSettings.Controls.Add(cBoxParity);
            gboxSerialSettings.Controls.Add(cBoxDataBits);
            gboxSerialSettings.Controls.Add(cBoxBaudRate);
            gboxSerialSettings.Controls.Add(cboxComPort);
            gboxSerialSettings.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gboxSerialSettings.Location = new Point(5, 62);
            gboxSerialSettings.Name = "gboxSerialSettings";
            gboxSerialSettings.Size = new Size(340, 170);
            gboxSerialSettings.TabIndex = 5;
            gboxSerialSettings.TabStop = false;
            gboxSerialSettings.Text = "Serial port settings";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 142);
            label5.Name = "label5";
            label5.Size = new Size(56, 15);
            label5.TabIndex = 11;
            label5.Text = "Stop bits";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(6, 113);
            label10.Name = "label10";
            label10.Size = new Size(39, 15);
            label10.TabIndex = 10;
            label10.Text = "Parity";
            // 
            // btnRefreshCom
            // 
            btnRefreshCom.Location = new Point(235, 51);
            btnRefreshCom.Name = "btnRefreshCom";
            btnRefreshCom.Size = new Size(96, 23);
            btnRefreshCom.TabIndex = 3;
            btnRefreshCom.Text = "Refresh";
            btnRefreshCom.UseVisualStyleBackColor = true;
            btnRefreshCom.Click += btnRefreshCom_Click;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(6, 84);
            label11.Name = "label11";
            label11.Size = new Size(56, 15);
            label11.TabIndex = 9;
            label11.Text = "Data bits";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(6, 55);
            label12.Name = "label12";
            label12.Size = new Size(61, 15);
            label12.TabIndex = 3;
            label12.Text = "Baud rate";
            // 
            // cBoxStopBits
            // 
            cBoxStopBits.DropDownStyle = ComboBoxStyle.DropDownList;
            cBoxStopBits.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cBoxStopBits.FormattingEnabled = true;
            cBoxStopBits.Location = new Point(73, 138);
            cBoxStopBits.Name = "cBoxStopBits";
            cBoxStopBits.Size = new Size(156, 23);
            cBoxStopBits.TabIndex = 7;
            cBoxStopBits.SelectedIndexChanged += Setting_Changed;
            // 
            // cBoxParity
            // 
            cBoxParity.DropDownStyle = ComboBoxStyle.DropDownList;
            cBoxParity.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cBoxParity.FormattingEnabled = true;
            cBoxParity.Location = new Point(73, 109);
            cBoxParity.Name = "cBoxParity";
            cBoxParity.Size = new Size(156, 23);
            cBoxParity.TabIndex = 6;
            cBoxParity.SelectedIndexChanged += Setting_Changed;
            // 
            // cBoxDataBits
            // 
            cBoxDataBits.DropDownStyle = ComboBoxStyle.DropDownList;
            cBoxDataBits.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cBoxDataBits.FormattingEnabled = true;
            cBoxDataBits.Location = new Point(73, 80);
            cBoxDataBits.Name = "cBoxDataBits";
            cBoxDataBits.Size = new Size(156, 23);
            cBoxDataBits.TabIndex = 5;
            cBoxDataBits.SelectedIndexChanged += Setting_Changed;
            // 
            // cBoxBaudRate
            // 
            cBoxBaudRate.DropDownStyle = ComboBoxStyle.DropDownList;
            cBoxBaudRate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cBoxBaudRate.FormattingEnabled = true;
            cBoxBaudRate.Location = new Point(73, 51);
            cBoxBaudRate.Name = "cBoxBaudRate";
            cBoxBaudRate.Size = new Size(156, 23);
            cBoxBaudRate.TabIndex = 4;
            cBoxBaudRate.SelectedIndexChanged += Setting_Changed;
            // 
            // cboxComPort
            // 
            cboxComPort.DropDownStyle = ComboBoxStyle.DropDownList;
            cboxComPort.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cboxComPort.FormattingEnabled = true;
            cboxComPort.Location = new Point(6, 22);
            cboxComPort.Name = "cboxComPort";
            cboxComPort.Size = new Size(325, 23);
            cboxComPort.Sorted = true;
            cboxComPort.TabIndex = 3;
            cboxComPort.SelectedIndexChanged += cboxComPort_SelectedIndexChanged;
            cboxComPort.DropDownClosed += CboxComPort_DropDownClosed;
            // 
            // btnDisconnect
            // 
            btnDisconnect.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnDisconnect.ImageAlign = ContentAlignment.MiddleLeft;
            btnDisconnect.ImageKey = "icons8-disconnected-50.png";
            btnDisconnect.ImageList = imageList1;
            btnDisconnect.Location = new Point(31, 405);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.RightToLeft = RightToLeft.No;
            btnDisconnect.Size = new Size(116, 32);
            btnDisconnect.TabIndex = 5;
            btnDisconnect.Text = "Disconnect";
            btnDisconnect.TextAlign = ContentAlignment.MiddleRight;
            btnDisconnect.UseVisualStyleBackColor = true;
            btnDisconnect.Click += btnDisconnect_Click;
            // 
            // gboxConnection
            // 
            gboxConnection.Controls.Add(cboxConnection);
            gboxConnection.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gboxConnection.ForeColor = SystemColors.ControlText;
            gboxConnection.Location = new Point(5, 6);
            gboxConnection.Name = "gboxConnection";
            gboxConnection.Size = new Size(340, 50);
            gboxConnection.TabIndex = 6;
            gboxConnection.TabStop = false;
            gboxConnection.Text = "Connection";
            // 
            // cboxConnection
            // 
            cboxConnection.DropDownStyle = ComboBoxStyle.DropDownList;
            cboxConnection.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cboxConnection.FormattingEnabled = true;
            cboxConnection.Location = new Point(6, 22);
            cboxConnection.Name = "cboxConnection";
            cboxConnection.Size = new Size(325, 23);
            cboxConnection.TabIndex = 0;
            cboxConnection.SelectedIndexChanged += cboxConnection_SelectedIndexChanged;
            // 
            // treeButton
            // 
            treeButton.Controls.Add(label4);
            treeButton.Controls.Add(openTree);
            treeButton.Dock = DockStyle.Top;
            treeButton.Location = new Point(0, 50);
            treeButton.Name = "treeButton";
            treeButton.Size = new Size(350, 50);
            treeButton.TabIndex = 2;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label4.Location = new Point(223, 13);
            label4.Name = "label4";
            label4.Size = new Size(78, 25);
            label4.TabIndex = 1;
            label4.Text = "Devices";
            // 
            // openTree
            // 
            openTree.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            openTree.Location = new Point(305, 5);
            openTree.Name = "openTree";
            openTree.Size = new Size(40, 40);
            openTree.TabIndex = 0;
            openTree.Text = "⫷";
            openTree.UseVisualStyleBackColor = true;
            openTree.Click += openTree_Click;
            // 
            // setupButton
            // 
            setupButton.Controls.Add(label3);
            setupButton.Controls.Add(openMenu);
            setupButton.Dock = DockStyle.Top;
            setupButton.Location = new Point(0, 0);
            setupButton.Name = "setupButton";
            setupButton.Size = new Size(350, 50);
            setupButton.TabIndex = 0;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label3.Location = new Point(240, 13);
            label3.Name = "label3";
            label3.Size = new Size(61, 25);
            label3.TabIndex = 1;
            label3.Text = "Setup";
            // 
            // openMenu
            // 
            openMenu.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            openMenu.Location = new Point(305, 5);
            openMenu.Name = "openMenu";
            openMenu.Size = new Size(40, 40);
            openMenu.TabIndex = 0;
            openMenu.Text = "⫷";
            openMenu.UseVisualStyleBackColor = true;
            openMenu.Click += openMenu_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, toolStripStatusLabel2 });
            statusStrip1.Location = new Point(350, 576);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(321, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(170, 17);
            toolStripStatusLabel1.Text = "Connected: COM1/9600/8/N/1";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.ImageScaling = ToolStripItemImageScaling.None;
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(136, 17);
            toolStripStatusLabel2.Spring = true;
            toolStripStatusLabel2.Text = "toolStripStatusLabel2";
            toolStripStatusLabel2.TextAlign = ContentAlignment.MiddleRight;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem, layoutToolStripMenuItem, aboutToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(350, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(321, 24);
            menuStrip1.TabIndex = 6;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, slaveScanToolStripMenuItem, addresScanToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            newToolStripMenuItem.Image = Properties.Resources.icons8_file_50;
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            newToolStripMenuItem.Size = new Size(177, 22);
            newToolStripMenuItem.Text = "New";
            newToolStripMenuItem.Click += newToolStripMenuItem_Click;
            // 
            // slaveScanToolStripMenuItem
            // 
            slaveScanToolStripMenuItem.Image = (Image)resources.GetObject("slaveScanToolStripMenuItem.Image");
            slaveScanToolStripMenuItem.Name = "slaveScanToolStripMenuItem";
            slaveScanToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.D;
            slaveScanToolStripMenuItem.Size = new Size(177, 22);
            slaveScanToolStripMenuItem.Text = "Device Scan";
            slaveScanToolStripMenuItem.Click += slaveScanToolStripMenuItem_Click;
            // 
            // addresScanToolStripMenuItem
            // 
            addresScanToolStripMenuItem.Image = Properties.Resources.icons8_online_binary_code_50;
            addresScanToolStripMenuItem.Name = "addresScanToolStripMenuItem";
            addresScanToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.A;
            addresScanToolStripMenuItem.Size = new Size(177, 22);
            addresScanToolStripMenuItem.Text = "Addres Scan";
            addresScanToolStripMenuItem.Click += addresScanToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { communicationToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "View";
            // 
            // communicationToolStripMenuItem
            // 
            communicationToolStripMenuItem.Image = Properties.Resources.icons8_networking_manager_50;
            communicationToolStripMenuItem.Name = "communicationToolStripMenuItem";
            communicationToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.C;
            communicationToolStripMenuItem.Size = new Size(199, 22);
            communicationToolStripMenuItem.Text = "Communication";
            communicationToolStripMenuItem.Click += communicationToolStripMenuItem_Click;
            // 
            // layoutToolStripMenuItem
            // 
            layoutToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { cascadeToolStripMenuItem, splitHorizontalToolStripMenuItem, splitVerticalToolStripMenuItem });
            layoutToolStripMenuItem.Name = "layoutToolStripMenuItem";
            layoutToolStripMenuItem.Size = new Size(55, 20);
            layoutToolStripMenuItem.Text = "Layout";
            // 
            // cascadeToolStripMenuItem
            // 
            cascadeToolStripMenuItem.Image = Properties.Resources.icons8_static_view_level2_48;
            cascadeToolStripMenuItem.Name = "cascadeToolStripMenuItem";
            cascadeToolStripMenuItem.Size = new Size(155, 22);
            cascadeToolStripMenuItem.Text = "Cascade";
            cascadeToolStripMenuItem.Click += cascadeToolStripMenuItem_Click;
            // 
            // splitHorizontalToolStripMenuItem
            // 
            splitHorizontalToolStripMenuItem.Image = Properties.Resources.icons8_vertical_docking_48;
            splitHorizontalToolStripMenuItem.Name = "splitHorizontalToolStripMenuItem";
            splitHorizontalToolStripMenuItem.Size = new Size(155, 22);
            splitHorizontalToolStripMenuItem.Text = "Split Horizontal";
            splitHorizontalToolStripMenuItem.Click += splitHorizontalToolStripMenuItem_Click;
            // 
            // splitVerticalToolStripMenuItem
            // 
            splitVerticalToolStripMenuItem.Image = Properties.Resources.icons8_horizontal_docking_48;
            splitVerticalToolStripMenuItem.Name = "splitVerticalToolStripMenuItem";
            splitVerticalToolStripMenuItem.Size = new Size(155, 22);
            splitVerticalToolStripMenuItem.Text = "Split Vertical";
            splitVerticalToolStripMenuItem.Click += splitVerticalToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(52, 20);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.ShortcutKeys = Keys.F1;
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            helpToolStripMenuItem.Click += helpToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(177, 6);
            // 
            // renameDeviceContextMenuItem
            // 
            renameDeviceContextMenuItem.Name = "renameDeviceContextMenuItem";
            renameDeviceContextMenuItem.ShortcutKeys = Keys.F2;
            renameDeviceContextMenuItem.Size = new Size(180, 22);
            renameDeviceContextMenuItem.Text = "Rename";
            renameDeviceContextMenuItem.Click += renameDeviceContextMenuItem_Click;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(671, 598);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            Controls.Add(sidePanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            IsMdiContainer = true;
            MainMenuStrip = menuStrip1;
            MdiChildrenMinimizedAnchorBottom = false;
            MinimumSize = new Size(370, 530);
            Name = "MainWindow";
            Text = "MP ModbusApp";
            Load += MainWindow_Load;
            DragDrop += MainWindow_DragDrop;
            DragEnter += MainWindow_DragEnter;
            sidePanel.ResumeLayout(false);
            treeViewContextMenu.ResumeLayout(false);
            setupPanel.ResumeLayout(false);
            gBoxGlobalSettings.ResumeLayout(false);
            gBoxGlobalSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxRetries).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPollDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numResponseTimeout).EndInit();
            gBoxSerialMode.ResumeLayout(false);
            gBoxSerialMode.PerformLayout();
            gboxIPSettings.ResumeLayout(false);
            gboxIPSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numIPConnTimeout).EndInit();
            ((System.ComponentModel.ISupportInitialize)numIPPort).EndInit();
            gboxSerialSettings.ResumeLayout(false);
            gboxSerialSettings.PerformLayout();
            gboxConnection.ResumeLayout(false);
            treeButton.ResumeLayout(false);
            treeButton.PerformLayout();
            setupButton.ResumeLayout(false);
            setupButton.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Panel sidePanel;
        private Button openMenu;
        private GroupBox gboxSerialSettings;
        private Label label5;
        private Label label10;
        private Button btnRefreshCom;
        private Label label11;
        private Label label12;
        private ComboBox cBoxStopBits;
        private ComboBox cBoxParity;
        private ComboBox cBoxDataBits;
        private ComboBox cBoxBaudRate;
        private ComboBox cboxComPort;
        private GroupBox gboxIPSettings;
        private Label label9;
        private RadioButton rbtnIPv6;
        private RadioButton rbtnIPv4;
        private Label label8;
        private NumericUpDown numIPConnTimeout;
        private Label label7;
        private NumericUpDown numIPPort;
        private Label label6;
        private ComboBox cboxIPAddress;
        private Panel setupPanel;
        private Panel setupButton;
        private GroupBox gboxConnection;
        private ComboBox cboxConnection;
        private GroupBox gBoxGlobalSettings;
        private NumericUpDown numResponseTimeout;
        private GroupBox gBoxSerialMode;
        private RadioButton rBtnASCII;
        private RadioButton rBtnRTU;
        private Label label2;
        private NumericUpDown numPollDelay;
        private Label label1;
        private Label label3;
        private Button btnConnect;
        private Button btnDisconnect;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem communicationToolStripMenuItem;
        private Panel treeButton;
        private Label label4;
        private Button openTree;
        private TreeView treeView;
        private ImageList imageList1;
        private ContextMenuStrip treeViewContextMenu;
        private ToolStripMenuItem importDeviceContextMenuItem;
        private ToolStripMenuItem exportDeviceContextMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem deleteDeviceContextMenuItem;
        private ToolStripMenuItem layoutToolStripMenuItem;
        private ToolStripMenuItem cascadeToolStripMenuItem;
        private ToolStripMenuItem splitHorizontalToolStripMenuItem;
        private ToolStripMenuItem splitVerticalToolStripMenuItem;
        private NumericUpDown numMaxRetries;
        private Label label13;
        private ToolStripMenuItem slaveScanToolStripMenuItem;
        private ToolStripMenuItem addresScanToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private HelpProvider helpProvider1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem renameDeviceContextMenuItem;
    }
}
