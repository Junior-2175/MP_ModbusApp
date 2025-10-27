namespace MP_ModbusApp
{
    partial class ReadingsTab
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            numOfRegisters = new NumericUpDown();
            label3 = new Label();
            startRegister = new NumericUpDown();
            label2 = new Label();
            startRegisterHex = new NumericUpDown();
            label1 = new Label();
            comboBox1 = new ComboBox();
            groupBox1 = new GroupBox();
            lblTabError = new Label();
            startRegister_1 = new NumericUpDown();
            dataGridView1 = new DataGridView();
            contextMenuStrip1 = new ContextMenuStrip(components);
            toolStripMenuItem2 = new ToolStripMenuItem();
            unsignedToolStripMenuItem = new ToolStripMenuItem();
            signedToolStripMenuItem = new ToolStripMenuItem();
            binaryToolStripMenuItem = new ToolStripMenuItem();
            hexToolStripMenuItem = new ToolStripMenuItem();
            aSCIIToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripMenuItem();
            unsignedToolStripMenuItem1 = new ToolStripMenuItem();
            bigendianToolStripMenuItem = new ToolStripMenuItem();
            littleendianToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            bigendianToolStripMenuItem1 = new ToolStripMenuItem();
            littleendianToolStripMenuItem1 = new ToolStripMenuItem();
            signedToolStripMenuItem1 = new ToolStripMenuItem();
            bigendianToolStripMenuItem2 = new ToolStripMenuItem();
            littleendianToolStripMenuItem3 = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            bigendianByteSwapToolStripMenuItem = new ToolStripMenuItem();
            littleendianByteSwapToolStripMenuItem1 = new ToolStripMenuItem();
            realToolStripMenuItem = new ToolStripMenuItem();
            bigendianToolStripMenuItem3 = new ToolStripMenuItem();
            littleendianToolStripMenuItem2 = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            bigendianByteSwapToolStripMenuItem1 = new ToolStripMenuItem();
            littleendianByteSwapToolStripMenuItem = new ToolStripMenuItem();
            hexToolStripMenuItem1 = new ToolStripMenuItem();
            bigendianToolStripMenuItem4 = new ToolStripMenuItem();
            littleendianToolStripMenuItem5 = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            bigendianByteSwapToolStripMenuItem2 = new ToolStripMenuItem();
            littleendianByteSwapToolStripMenuItem2 = new ToolStripMenuItem();
            aSCIIToolStripMenuItem1 = new ToolStripMenuItem();
            bigendianToolStripMenuItem5 = new ToolStripMenuItem();
            littleendianToolStripMenuItem4 = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            bigendianByteSwapToolStripMenuItem3 = new ToolStripMenuItem();
            littleendianByteSwapToolStripMenuItem3 = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripMenuItem();
            unsignedToolStripMenuItem2 = new ToolStripMenuItem();
            bigendianToolStripMenuItem6 = new ToolStripMenuItem();
            littleendianToolStripMenuItem6 = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            bigendianByteSwapToolStripMenuItem4 = new ToolStripMenuItem();
            littleendianByteSwapToolStripMenuItem4 = new ToolStripMenuItem();
            signedToolStripMenuItem2 = new ToolStripMenuItem();
            bigendianToolStripMenuItem7 = new ToolStripMenuItem();
            littleendianToolStripMenuItem7 = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            bigendianByteSwapToolStripMenuItem5 = new ToolStripMenuItem();
            littleendianByteSwapToolStripMenuItem5 = new ToolStripMenuItem();
            realToolStripMenuItem1 = new ToolStripMenuItem();
            bigendianToolStripMenuItem8 = new ToolStripMenuItem();
            littleendianToolStripMenuItem8 = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            bigendianByteSwapToolStripMenuItem6 = new ToolStripMenuItem();
            littleendianByteSwapToolStripMenuItem6 = new ToolStripMenuItem();
            hexToolStripMenuItem2 = new ToolStripMenuItem();
            bigendianToolStripMenuItem9 = new ToolStripMenuItem();
            littleendianToolStripMenuItem9 = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            bigendianByteSwapToolStripMenuItem7 = new ToolStripMenuItem();
            littleendianByteSwapToolStripMenuItem7 = new ToolStripMenuItem();
            aSCIIToolStripMenuItem2 = new ToolStripMenuItem();
            bigendianToolStripMenuItem10 = new ToolStripMenuItem();
            littleendianToolStripMenuItem10 = new ToolStripMenuItem();
            toolStripSeparator10 = new ToolStripSeparator();
            bigendianByteSwapToolStripMenuItem8 = new ToolStripMenuItem();
            littleendianByteSwapToolStripMenuItem8 = new ToolStripMenuItem();
            timer1 = new System.Windows.Forms.Timer(components);
            Name = new DataGridViewTextBoxColumn();
            RegisterNumber = new DataGridViewTextBoxColumn();
            Value = new DataGridViewTextBoxColumn();
            DisplayFormatColumn = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)numOfRegisters).BeginInit();
            ((System.ComponentModel.ISupportInitialize)startRegister).BeginInit();
            ((System.ComponentModel.ISupportInitialize)startRegisterHex).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)startRegister_1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // numOfRegisters
            // 
            numOfRegisters.Font = new Font("Segoe UI", 9F);
            numOfRegisters.Location = new Point(101, 77);
            numOfRegisters.Maximum = new decimal(new int[] { 125, 0, 0, 0 });
            numOfRegisters.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numOfRegisters.Name = "numOfRegisters";
            numOfRegisters.Size = new Size(69, 23);
            numOfRegisters.TabIndex = 1;
            numOfRegisters.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numOfRegisters.ValueChanged += numOfRegisters_ValueChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F);
            label3.Location = new Point(9, 79);
            label3.Name = "label3";
            label3.Size = new Size(56, 15);
            label3.TabIndex = 3;
            label3.Text = "Quantity:";
            // 
            // startRegister
            // 
            startRegister.Font = new Font("Segoe UI", 9F);
            startRegister.Location = new Point(101, 48);
            startRegister.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            startRegister.Name = "startRegister";
            startRegister.Size = new Size(69, 23);
            startRegister.TabIndex = 1;
            startRegister.Value = new decimal(new int[] { 100, 0, 0, 0 });
            startRegister.ValueChanged += startRegister_ValueChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F);
            label2.Location = new Point(9, 50);
            label2.Name = "label2";
            label2.Size = new Size(76, 15);
            label2.TabIndex = 3;
            label2.Text = "Start register:";
            // 
            // startRegisterHex
            // 
            startRegisterHex.Font = new Font("Segoe UI", 9F);
            startRegisterHex.Hexadecimal = true;
            startRegisterHex.Location = new Point(251, 48);
            startRegisterHex.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            startRegisterHex.Name = "startRegisterHex";
            startRegisterHex.Size = new Size(69, 23);
            startRegisterHex.TabIndex = 1;
            startRegisterHex.Value = new decimal(new int[] { 100, 0, 0, 0 });
            startRegisterHex.ValueChanged += startRegisterHex_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F);
            label1.Location = new Point(9, 25);
            label1.Name = "label1";
            label1.Size = new Size(86, 15);
            label1.TabIndex = 3;
            label1.Text = "Function code:";
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Font = new Font("Segoe UI", 9F);
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "01 Coils (0x)", "02 Discrete Inputs (1x)", "03 Holding Registers (4x)", "04 Input Registers (3x)" });
            comboBox1.Location = new Point(101, 22);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(219, 23);
            comboBox1.TabIndex = 2;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lblTabError);
            groupBox1.Controls.Add(numOfRegisters);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(startRegister_1);
            groupBox1.Controls.Add(startRegister);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(startRegisterHex);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(comboBox1);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(332, 109);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Read setup";
            // 
            // lblTabError
            // 
            lblTabError.AutoSize = true;
            lblTabError.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblTabError.ForeColor = Color.FromArgb(192, 0, 0);
            lblTabError.Location = new Point(176, 79);
            lblTabError.Name = "lblTabError";
            lblTabError.Size = new Size(39, 15);
            lblTabError.TabIndex = 4;
            lblTabError.Text = "label4";
            // 
            // startRegister_1
            // 
            startRegister_1.Font = new Font("Segoe UI", 9F);
            startRegister_1.Location = new Point(176, 48);
            startRegister_1.Maximum = new decimal(new int[] { 465536, 0, 0, 0 });
            startRegister_1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            startRegister_1.Name = "startRegister_1";
            startRegister_1.Size = new Size(69, 23);
            startRegister_1.TabIndex = 1;
            startRegister_1.Value = new decimal(new int[] { 40101, 0, 0, 0 });
            startRegister_1.ValueChanged += startRegister_1_ValueChanged;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Name, RegisterNumber, Value, DisplayFormatColumn });
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 109);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Size = new Size(332, 329);
            dataGridView1.TabIndex = 6;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem2, toolStripMenuItem3, toolStripMenuItem4 });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(106, 70);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.DropDownItems.AddRange(new ToolStripItem[] { unsignedToolStripMenuItem, signedToolStripMenuItem, binaryToolStripMenuItem, hexToolStripMenuItem, aSCIIToolStripMenuItem });
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(105, 22);
            toolStripMenuItem2.Text = "16-bit";
            // 
            // unsignedToolStripMenuItem
            // 
            unsignedToolStripMenuItem.Name = "unsignedToolStripMenuItem";
            unsignedToolStripMenuItem.Size = new Size(124, 22);
            unsignedToolStripMenuItem.Text = "Unsigned";
            unsignedToolStripMenuItem.Click += unsignedToolStripMenuItem_Click;
            // 
            // signedToolStripMenuItem
            // 
            signedToolStripMenuItem.Name = "signedToolStripMenuItem";
            signedToolStripMenuItem.Size = new Size(124, 22);
            signedToolStripMenuItem.Text = "Signed";
            signedToolStripMenuItem.Click += signedToolStripMenuItem_Click;
            // 
            // binaryToolStripMenuItem
            // 
            binaryToolStripMenuItem.Name = "binaryToolStripMenuItem";
            binaryToolStripMenuItem.Size = new Size(124, 22);
            binaryToolStripMenuItem.Text = "Binary";
            binaryToolStripMenuItem.Click += binaryToolStripMenuItem_Click;
            // 
            // hexToolStripMenuItem
            // 
            hexToolStripMenuItem.Name = "hexToolStripMenuItem";
            hexToolStripMenuItem.Size = new Size(124, 22);
            hexToolStripMenuItem.Text = "Hex";
            hexToolStripMenuItem.Click += hexToolStripMenuItem_Click;
            // 
            // aSCIIToolStripMenuItem
            // 
            aSCIIToolStripMenuItem.Name = "aSCIIToolStripMenuItem";
            aSCIIToolStripMenuItem.Size = new Size(124, 22);
            aSCIIToolStripMenuItem.Text = "ASCII";
            aSCIIToolStripMenuItem.Click += aSCIIToolStripMenuItem_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[] { unsignedToolStripMenuItem1, signedToolStripMenuItem1, realToolStripMenuItem, hexToolStripMenuItem1, aSCIIToolStripMenuItem1 });
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(105, 22);
            toolStripMenuItem3.Text = "32-bit";
            // 
            // unsignedToolStripMenuItem1
            // 
            unsignedToolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { bigendianToolStripMenuItem, littleendianToolStripMenuItem, toolStripSeparator1, bigendianToolStripMenuItem1, littleendianToolStripMenuItem1 });
            unsignedToolStripMenuItem1.Name = "unsignedToolStripMenuItem1";
            unsignedToolStripMenuItem1.Size = new Size(124, 22);
            unsignedToolStripMenuItem1.Text = "Unsigned";
            // 
            // bigendianToolStripMenuItem
            // 
            bigendianToolStripMenuItem.Name = "bigendianToolStripMenuItem";
            bigendianToolStripMenuItem.Size = new Size(197, 22);
            bigendianToolStripMenuItem.Text = "Big-endian";
            bigendianToolStripMenuItem.Click += unsigned32BEToolStripMenuItem_Click;
            // 
            // littleendianToolStripMenuItem
            // 
            littleendianToolStripMenuItem.Name = "littleendianToolStripMenuItem";
            littleendianToolStripMenuItem.Size = new Size(197, 22);
            littleendianToolStripMenuItem.Text = "Little-endian";
            littleendianToolStripMenuItem.Click += unsigned32LEToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(194, 6);
            // 
            // bigendianToolStripMenuItem1
            // 
            bigendianToolStripMenuItem1.Name = "bigendianToolStripMenuItem1";
            bigendianToolStripMenuItem1.Size = new Size(197, 22);
            bigendianToolStripMenuItem1.Text = "Big-endian byte swap";
            // 
            // littleendianToolStripMenuItem1
            // 
            littleendianToolStripMenuItem1.Name = "littleendianToolStripMenuItem1";
            littleendianToolStripMenuItem1.Size = new Size(197, 22);
            littleendianToolStripMenuItem1.Text = "Little-endian byte swap";
            // 
            // signedToolStripMenuItem1
            // 
            signedToolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { bigendianToolStripMenuItem2, littleendianToolStripMenuItem3, toolStripSeparator2, bigendianByteSwapToolStripMenuItem, littleendianByteSwapToolStripMenuItem1 });
            signedToolStripMenuItem1.Name = "signedToolStripMenuItem1";
            signedToolStripMenuItem1.Size = new Size(124, 22);
            signedToolStripMenuItem1.Text = "Signed";
            // 
            // bigendianToolStripMenuItem2
            // 
            bigendianToolStripMenuItem2.Name = "bigendianToolStripMenuItem2";
            bigendianToolStripMenuItem2.Size = new Size(197, 22);
            bigendianToolStripMenuItem2.Text = "Big-endian";
            bigendianToolStripMenuItem2.Click += signed32BEToolStripMenuItem_Click;
            // 
            // littleendianToolStripMenuItem3
            // 
            littleendianToolStripMenuItem3.Name = "littleendianToolStripMenuItem3";
            littleendianToolStripMenuItem3.Size = new Size(197, 22);
            littleendianToolStripMenuItem3.Text = "Little-endian";
            littleendianToolStripMenuItem3.Click += signed32LEToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(194, 6);
            // 
            // bigendianByteSwapToolStripMenuItem
            // 
            bigendianByteSwapToolStripMenuItem.Name = "bigendianByteSwapToolStripMenuItem";
            bigendianByteSwapToolStripMenuItem.Size = new Size(197, 22);
            bigendianByteSwapToolStripMenuItem.Text = "Big-endian byte swap";
            // 
            // littleendianByteSwapToolStripMenuItem1
            // 
            littleendianByteSwapToolStripMenuItem1.Name = "littleendianByteSwapToolStripMenuItem1";
            littleendianByteSwapToolStripMenuItem1.Size = new Size(197, 22);
            littleendianByteSwapToolStripMenuItem1.Text = "Little-endian byte swap";
            // 
            // realToolStripMenuItem
            // 
            realToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { bigendianToolStripMenuItem3, littleendianToolStripMenuItem2, toolStripSeparator3, bigendianByteSwapToolStripMenuItem1, littleendianByteSwapToolStripMenuItem });
            realToolStripMenuItem.Name = "realToolStripMenuItem";
            realToolStripMenuItem.Size = new Size(124, 22);
            realToolStripMenuItem.Text = "Real";
            // 
            // bigendianToolStripMenuItem3
            // 
            bigendianToolStripMenuItem3.Name = "bigendianToolStripMenuItem3";
            bigendianToolStripMenuItem3.Size = new Size(197, 22);
            bigendianToolStripMenuItem3.Text = "Big-endian";
            bigendianToolStripMenuItem3.Click += float32BEToolStripMenuItem_Click;
            // 
            // littleendianToolStripMenuItem2
            // 
            littleendianToolStripMenuItem2.Name = "littleendianToolStripMenuItem2";
            littleendianToolStripMenuItem2.Size = new Size(197, 22);
            littleendianToolStripMenuItem2.Text = "Little-endian";
            littleendianToolStripMenuItem2.Click += float32LEToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(194, 6);
            // 
            // bigendianByteSwapToolStripMenuItem1
            // 
            bigendianByteSwapToolStripMenuItem1.Name = "bigendianByteSwapToolStripMenuItem1";
            bigendianByteSwapToolStripMenuItem1.Size = new Size(197, 22);
            bigendianByteSwapToolStripMenuItem1.Text = "Big-endian byte swap";
            // 
            // littleendianByteSwapToolStripMenuItem
            // 
            littleendianByteSwapToolStripMenuItem.Name = "littleendianByteSwapToolStripMenuItem";
            littleendianByteSwapToolStripMenuItem.Size = new Size(197, 22);
            littleendianByteSwapToolStripMenuItem.Text = "Little-endian byte swap";
            // 
            // hexToolStripMenuItem1
            // 
            hexToolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { bigendianToolStripMenuItem4, littleendianToolStripMenuItem5, toolStripSeparator4, bigendianByteSwapToolStripMenuItem2, littleendianByteSwapToolStripMenuItem2 });
            hexToolStripMenuItem1.Name = "hexToolStripMenuItem1";
            hexToolStripMenuItem1.Size = new Size(124, 22);
            hexToolStripMenuItem1.Text = "Hex";
            // 
            // bigendianToolStripMenuItem4
            // 
            bigendianToolStripMenuItem4.Name = "bigendianToolStripMenuItem4";
            bigendianToolStripMenuItem4.Size = new Size(197, 22);
            bigendianToolStripMenuItem4.Text = "Big-endian";
            // 
            // littleendianToolStripMenuItem5
            // 
            littleendianToolStripMenuItem5.Name = "littleendianToolStripMenuItem5";
            littleendianToolStripMenuItem5.Size = new Size(197, 22);
            littleendianToolStripMenuItem5.Text = "Little-endian";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(194, 6);
            // 
            // bigendianByteSwapToolStripMenuItem2
            // 
            bigendianByteSwapToolStripMenuItem2.Name = "bigendianByteSwapToolStripMenuItem2";
            bigendianByteSwapToolStripMenuItem2.Size = new Size(197, 22);
            bigendianByteSwapToolStripMenuItem2.Text = "Big-endian byte swap";
            // 
            // littleendianByteSwapToolStripMenuItem2
            // 
            littleendianByteSwapToolStripMenuItem2.Name = "littleendianByteSwapToolStripMenuItem2";
            littleendianByteSwapToolStripMenuItem2.Size = new Size(197, 22);
            littleendianByteSwapToolStripMenuItem2.Text = "Little-endian byte swap";
            // 
            // aSCIIToolStripMenuItem1
            // 
            aSCIIToolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { bigendianToolStripMenuItem5, littleendianToolStripMenuItem4, toolStripSeparator5, bigendianByteSwapToolStripMenuItem3, littleendianByteSwapToolStripMenuItem3 });
            aSCIIToolStripMenuItem1.Name = "aSCIIToolStripMenuItem1";
            aSCIIToolStripMenuItem1.Size = new Size(124, 22);
            aSCIIToolStripMenuItem1.Text = "ASCII";
            // 
            // bigendianToolStripMenuItem5
            // 
            bigendianToolStripMenuItem5.Name = "bigendianToolStripMenuItem5";
            bigendianToolStripMenuItem5.Size = new Size(197, 22);
            bigendianToolStripMenuItem5.Text = "Big-endian";
            // 
            // littleendianToolStripMenuItem4
            // 
            littleendianToolStripMenuItem4.Name = "littleendianToolStripMenuItem4";
            littleendianToolStripMenuItem4.Size = new Size(197, 22);
            littleendianToolStripMenuItem4.Text = "Little-endian";
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(194, 6);
            // 
            // bigendianByteSwapToolStripMenuItem3
            // 
            bigendianByteSwapToolStripMenuItem3.Name = "bigendianByteSwapToolStripMenuItem3";
            bigendianByteSwapToolStripMenuItem3.Size = new Size(197, 22);
            bigendianByteSwapToolStripMenuItem3.Text = "Big-endian byte swap";
            // 
            // littleendianByteSwapToolStripMenuItem3
            // 
            littleendianByteSwapToolStripMenuItem3.Name = "littleendianByteSwapToolStripMenuItem3";
            littleendianByteSwapToolStripMenuItem3.Size = new Size(197, 22);
            littleendianByteSwapToolStripMenuItem3.Text = "Little-endian byte swap";
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.DropDownItems.AddRange(new ToolStripItem[] { unsignedToolStripMenuItem2, signedToolStripMenuItem2, realToolStripMenuItem1, hexToolStripMenuItem2, aSCIIToolStripMenuItem2 });
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(105, 22);
            toolStripMenuItem4.Text = "64-bit";
            // 
            // unsignedToolStripMenuItem2
            // 
            unsignedToolStripMenuItem2.DropDownItems.AddRange(new ToolStripItem[] { bigendianToolStripMenuItem6, littleendianToolStripMenuItem6, toolStripSeparator6, bigendianByteSwapToolStripMenuItem4, littleendianByteSwapToolStripMenuItem4 });
            unsignedToolStripMenuItem2.Name = "unsignedToolStripMenuItem2";
            unsignedToolStripMenuItem2.Size = new Size(180, 22);
            unsignedToolStripMenuItem2.Text = "Unsigned";
            // 
            // bigendianToolStripMenuItem6
            // 
            bigendianToolStripMenuItem6.Name = "bigendianToolStripMenuItem6";
            bigendianToolStripMenuItem6.Size = new Size(197, 22);
            bigendianToolStripMenuItem6.Text = "Big-endian";
            bigendianToolStripMenuItem6.Click += bigendianToolStripMenuItem6_Click;
            // 
            // littleendianToolStripMenuItem6
            // 
            littleendianToolStripMenuItem6.Name = "littleendianToolStripMenuItem6";
            littleendianToolStripMenuItem6.Size = new Size(197, 22);
            littleendianToolStripMenuItem6.Text = "Little-endian";
            littleendianToolStripMenuItem6.Click += littleendianToolStripMenuItem6_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(194, 6);
            // 
            // bigendianByteSwapToolStripMenuItem4
            // 
            bigendianByteSwapToolStripMenuItem4.Name = "bigendianByteSwapToolStripMenuItem4";
            bigendianByteSwapToolStripMenuItem4.Size = new Size(197, 22);
            bigendianByteSwapToolStripMenuItem4.Text = "Big-endian byte swap";
            bigendianByteSwapToolStripMenuItem4.Click += bigendianByteSwapToolStripMenuItem4_Click;
            // 
            // littleendianByteSwapToolStripMenuItem4
            // 
            littleendianByteSwapToolStripMenuItem4.Name = "littleendianByteSwapToolStripMenuItem4";
            littleendianByteSwapToolStripMenuItem4.Size = new Size(197, 22);
            littleendianByteSwapToolStripMenuItem4.Text = "Little-endian byte swap";
            littleendianByteSwapToolStripMenuItem4.Click += littleendianByteSwapToolStripMenuItem4_Click;
            // 
            // signedToolStripMenuItem2
            // 
            signedToolStripMenuItem2.DropDownItems.AddRange(new ToolStripItem[] { bigendianToolStripMenuItem7, littleendianToolStripMenuItem7, toolStripSeparator7, bigendianByteSwapToolStripMenuItem5, littleendianByteSwapToolStripMenuItem5 });
            signedToolStripMenuItem2.Name = "signedToolStripMenuItem2";
            signedToolStripMenuItem2.Size = new Size(180, 22);
            signedToolStripMenuItem2.Text = "Signed";
            // 
            // bigendianToolStripMenuItem7
            // 
            bigendianToolStripMenuItem7.Name = "bigendianToolStripMenuItem7";
            bigendianToolStripMenuItem7.Size = new Size(197, 22);
            bigendianToolStripMenuItem7.Text = "Big-endian";
            bigendianToolStripMenuItem7.Click += bigendianToolStripMenuItem7_Click;
            // 
            // littleendianToolStripMenuItem7
            // 
            littleendianToolStripMenuItem7.Name = "littleendianToolStripMenuItem7";
            littleendianToolStripMenuItem7.Size = new Size(197, 22);
            littleendianToolStripMenuItem7.Text = "Little-endian";
            littleendianToolStripMenuItem7.Click += littleendianToolStripMenuItem7_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(194, 6);
            // 
            // bigendianByteSwapToolStripMenuItem5
            // 
            bigendianByteSwapToolStripMenuItem5.Name = "bigendianByteSwapToolStripMenuItem5";
            bigendianByteSwapToolStripMenuItem5.Size = new Size(197, 22);
            bigendianByteSwapToolStripMenuItem5.Text = "Big-endian byte swap";
            bigendianByteSwapToolStripMenuItem5.Click += bigendianByteSwapToolStripMenuItem5_Click;
            // 
            // littleendianByteSwapToolStripMenuItem5
            // 
            littleendianByteSwapToolStripMenuItem5.Name = "littleendianByteSwapToolStripMenuItem5";
            littleendianByteSwapToolStripMenuItem5.Size = new Size(197, 22);
            littleendianByteSwapToolStripMenuItem5.Text = "Little-endian byte swap";
            littleendianByteSwapToolStripMenuItem5.Click += littleendianByteSwapToolStripMenuItem5_Click;
            // 
            // realToolStripMenuItem1
            // 
            realToolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { bigendianToolStripMenuItem8, littleendianToolStripMenuItem8, toolStripSeparator8, bigendianByteSwapToolStripMenuItem6, littleendianByteSwapToolStripMenuItem6 });
            realToolStripMenuItem1.Name = "realToolStripMenuItem1";
            realToolStripMenuItem1.Size = new Size(180, 22);
            realToolStripMenuItem1.Text = "Real";
            // 
            // bigendianToolStripMenuItem8
            // 
            bigendianToolStripMenuItem8.Name = "bigendianToolStripMenuItem8";
            bigendianToolStripMenuItem8.Size = new Size(197, 22);
            bigendianToolStripMenuItem8.Text = "Big-endian";
            bigendianToolStripMenuItem8.Click += bigendianToolStripMenuItem8_Click;
            // 
            // littleendianToolStripMenuItem8
            // 
            littleendianToolStripMenuItem8.Name = "littleendianToolStripMenuItem8";
            littleendianToolStripMenuItem8.Size = new Size(197, 22);
            littleendianToolStripMenuItem8.Text = "Little-endian";
            littleendianToolStripMenuItem8.Click += littleendianToolStripMenuItem8_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(194, 6);
            // 
            // bigendianByteSwapToolStripMenuItem6
            // 
            bigendianByteSwapToolStripMenuItem6.Name = "bigendianByteSwapToolStripMenuItem6";
            bigendianByteSwapToolStripMenuItem6.Size = new Size(197, 22);
            bigendianByteSwapToolStripMenuItem6.Text = "Big-endian byte swap";
            bigendianByteSwapToolStripMenuItem6.Click += bigendianByteSwapToolStripMenuItem6_Click;
            // 
            // littleendianByteSwapToolStripMenuItem6
            // 
            littleendianByteSwapToolStripMenuItem6.Name = "littleendianByteSwapToolStripMenuItem6";
            littleendianByteSwapToolStripMenuItem6.Size = new Size(197, 22);
            littleendianByteSwapToolStripMenuItem6.Text = "Little-endian byte swap";
            littleendianByteSwapToolStripMenuItem6.Click += littleendianByteSwapToolStripMenuItem6_Click;
            // 
            // hexToolStripMenuItem2
            // 
            hexToolStripMenuItem2.DropDownItems.AddRange(new ToolStripItem[] { bigendianToolStripMenuItem9, littleendianToolStripMenuItem9, toolStripSeparator9, bigendianByteSwapToolStripMenuItem7, littleendianByteSwapToolStripMenuItem7 });
            hexToolStripMenuItem2.Name = "hexToolStripMenuItem2";
            hexToolStripMenuItem2.Size = new Size(180, 22);
            hexToolStripMenuItem2.Text = "Hex";
            // 
            // bigendianToolStripMenuItem9
            // 
            bigendianToolStripMenuItem9.Name = "bigendianToolStripMenuItem9";
            bigendianToolStripMenuItem9.Size = new Size(197, 22);
            bigendianToolStripMenuItem9.Text = "Big-endian";
            bigendianToolStripMenuItem9.Click += bigendianToolStripMenuItem9_Click;
            // 
            // littleendianToolStripMenuItem9
            // 
            littleendianToolStripMenuItem9.Name = "littleendianToolStripMenuItem9";
            littleendianToolStripMenuItem9.Size = new Size(197, 22);
            littleendianToolStripMenuItem9.Text = "Little-endian";
            littleendianToolStripMenuItem9.Click += littleendianToolStripMenuItem9_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(194, 6);
            // 
            // bigendianByteSwapToolStripMenuItem7
            // 
            bigendianByteSwapToolStripMenuItem7.Name = "bigendianByteSwapToolStripMenuItem7";
            bigendianByteSwapToolStripMenuItem7.Size = new Size(197, 22);
            bigendianByteSwapToolStripMenuItem7.Text = "Big-endian byte swap";
            bigendianByteSwapToolStripMenuItem7.Click += bigendianByteSwapToolStripMenuItem7_Click;
            // 
            // littleendianByteSwapToolStripMenuItem7
            // 
            littleendianByteSwapToolStripMenuItem7.Name = "littleendianByteSwapToolStripMenuItem7";
            littleendianByteSwapToolStripMenuItem7.Size = new Size(197, 22);
            littleendianByteSwapToolStripMenuItem7.Text = "Little-endian byte swap";
            littleendianByteSwapToolStripMenuItem7.Click += littleendianByteSwapToolStripMenuItem7_Click;
            // 
            // aSCIIToolStripMenuItem2
            // 
            aSCIIToolStripMenuItem2.DropDownItems.AddRange(new ToolStripItem[] { bigendianToolStripMenuItem10, littleendianToolStripMenuItem10, toolStripSeparator10, bigendianByteSwapToolStripMenuItem8, littleendianByteSwapToolStripMenuItem8 });
            aSCIIToolStripMenuItem2.Name = "aSCIIToolStripMenuItem2";
            aSCIIToolStripMenuItem2.Size = new Size(180, 22);
            aSCIIToolStripMenuItem2.Text = "ASCII";
            // 
            // bigendianToolStripMenuItem10
            // 
            bigendianToolStripMenuItem10.Name = "bigendianToolStripMenuItem10";
            bigendianToolStripMenuItem10.Size = new Size(197, 22);
            bigendianToolStripMenuItem10.Text = "Big-endian";
            bigendianToolStripMenuItem10.Click += bigendianToolStripMenuItem10_Click;
            // 
            // littleendianToolStripMenuItem10
            // 
            littleendianToolStripMenuItem10.Name = "littleendianToolStripMenuItem10";
            littleendianToolStripMenuItem10.Size = new Size(197, 22);
            littleendianToolStripMenuItem10.Text = "Little-endian";
            littleendianToolStripMenuItem10.Click += littleendianToolStripMenuItem10_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(194, 6);
            // 
            // bigendianByteSwapToolStripMenuItem8
            // 
            bigendianByteSwapToolStripMenuItem8.Name = "bigendianByteSwapToolStripMenuItem8";
            bigendianByteSwapToolStripMenuItem8.Size = new Size(197, 22);
            bigendianByteSwapToolStripMenuItem8.Text = "Big-endian byte swap";
            bigendianByteSwapToolStripMenuItem8.Click += bigendianByteSwapToolStripMenuItem8_Click;
            // 
            // littleendianByteSwapToolStripMenuItem8
            // 
            littleendianByteSwapToolStripMenuItem8.Name = "littleendianByteSwapToolStripMenuItem8";
            littleendianByteSwapToolStripMenuItem8.Size = new Size(197, 22);
            littleendianByteSwapToolStripMenuItem8.Text = "Little-endian byte swap";
            littleendianByteSwapToolStripMenuItem8.Click += littleendianByteSwapToolStripMenuItem8_Click;
            // 
            // Name
            // 
            Name.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Name.HeaderText = "Name";
            Name.Name = "Name";
            Name.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // RegisterNumber
            // 
            RegisterNumber.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            RegisterNumber.HeaderText = "Register Number";
            RegisterNumber.Name = "RegisterNumber";
            RegisterNumber.ReadOnly = true;
            RegisterNumber.SortMode = DataGridViewColumnSortMode.NotSortable;
            RegisterNumber.Width = 92;
            // 
            // Value
            // 
            Value.ContextMenuStrip = contextMenuStrip1;
            Value.HeaderText = "Value";
            Value.Name = "Value";
            Value.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // DisplayFormatColumn
            // 
            DisplayFormatColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            DisplayFormatColumn.HeaderText = "DisplayFormat";
            DisplayFormatColumn.Name = "DisplayFormatColumn";
            DisplayFormatColumn.ReadOnly = true;
            DisplayFormatColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            DisplayFormatColumn.Visible = false;
            DisplayFormatColumn.Width = 89;
            // 
            // ReadingsTab
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(dataGridView1);
            Controls.Add(groupBox1);
            Size = new Size(332, 438);
            Load += ReadingsTab_Load;
            ((System.ComponentModel.ISupportInitialize)numOfRegisters).EndInit();
            ((System.ComponentModel.ISupportInitialize)startRegister).EndInit();
            ((System.ComponentModel.ISupportInitialize)startRegisterHex).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)startRegister_1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private NumericUpDown numOfRegisters;
        private Label label3;
        private NumericUpDown startRegister;
        private Label label2;
        private NumericUpDown startRegisterHex;
        private Label label1;
        private ComboBox comboBox1;
        private GroupBox groupBox1;
        private DataGridView dataGridView1;
        private System.Windows.Forms.Timer timer1;
        private NumericUpDown startRegister_1;
        private Label lblTabError;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripMenuItem unsignedToolStripMenuItem;
        private ToolStripMenuItem signedToolStripMenuItem;
        private ToolStripMenuItem binaryToolStripMenuItem;
        private ToolStripMenuItem hexToolStripMenuItem;
        private ToolStripMenuItem aSCIIToolStripMenuItem;
        private ToolStripMenuItem unsignedToolStripMenuItem1;
        private ToolStripMenuItem bigendianToolStripMenuItem;
        private ToolStripMenuItem littleendianToolStripMenuItem;
        private ToolStripMenuItem signedToolStripMenuItem1;
        private ToolStripMenuItem realToolStripMenuItem;
        private ToolStripMenuItem hexToolStripMenuItem1;
        private ToolStripMenuItem aSCIIToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem bigendianToolStripMenuItem1;
        private ToolStripMenuItem littleendianToolStripMenuItem1;
        private ToolStripMenuItem bigendianToolStripMenuItem2;
        private ToolStripMenuItem littleendianToolStripMenuItem3;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem bigendianByteSwapToolStripMenuItem;
        private ToolStripMenuItem littleendianByteSwapToolStripMenuItem1;
        private ToolStripMenuItem bigendianToolStripMenuItem3;
        private ToolStripMenuItem littleendianToolStripMenuItem2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem bigendianByteSwapToolStripMenuItem1;
        private ToolStripMenuItem littleendianByteSwapToolStripMenuItem;
        private ToolStripMenuItem bigendianToolStripMenuItem4;
        private ToolStripMenuItem littleendianToolStripMenuItem5;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem bigendianByteSwapToolStripMenuItem2;
        private ToolStripMenuItem littleendianByteSwapToolStripMenuItem2;
        private ToolStripMenuItem bigendianToolStripMenuItem5;
        private ToolStripMenuItem littleendianToolStripMenuItem4;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem bigendianByteSwapToolStripMenuItem3;
        private ToolStripMenuItem littleendianByteSwapToolStripMenuItem3;
        private ToolStripMenuItem unsignedToolStripMenuItem2;
        private ToolStripMenuItem bigendianToolStripMenuItem6;
        private ToolStripMenuItem littleendianToolStripMenuItem6;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem bigendianByteSwapToolStripMenuItem4;
        private ToolStripMenuItem littleendianByteSwapToolStripMenuItem4;
        private ToolStripMenuItem signedToolStripMenuItem2;
        private ToolStripMenuItem bigendianToolStripMenuItem7;
        private ToolStripMenuItem littleendianToolStripMenuItem7;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem bigendianByteSwapToolStripMenuItem5;
        private ToolStripMenuItem littleendianByteSwapToolStripMenuItem5;
        private ToolStripMenuItem realToolStripMenuItem1;
        private ToolStripMenuItem bigendianToolStripMenuItem8;
        private ToolStripMenuItem littleendianToolStripMenuItem8;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem bigendianByteSwapToolStripMenuItem6;
        private ToolStripMenuItem littleendianByteSwapToolStripMenuItem6;
        private ToolStripMenuItem hexToolStripMenuItem2;
        private ToolStripMenuItem bigendianToolStripMenuItem9;
        private ToolStripMenuItem littleendianToolStripMenuItem9;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripMenuItem bigendianByteSwapToolStripMenuItem7;
        private ToolStripMenuItem littleendianByteSwapToolStripMenuItem7;
        private ToolStripMenuItem aSCIIToolStripMenuItem2;
        private ToolStripMenuItem bigendianToolStripMenuItem10;
        private ToolStripMenuItem littleendianToolStripMenuItem10;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem bigendianByteSwapToolStripMenuItem8;
        private ToolStripMenuItem littleendianByteSwapToolStripMenuItem8;
        private DataGridViewTextBoxColumn Name;
        private DataGridViewTextBoxColumn RegisterNumber;
        private DataGridViewTextBoxColumn Value;
        private DataGridViewTextBoxColumn DisplayFormatColumn;
    }
}
