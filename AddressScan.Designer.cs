namespace MP_ModbusApp
{
    partial class AddressScan
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            startToolStripMenuItem = new ToolStripMenuItem();
            stopToolStripMenuItem1 = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            panel1 = new Panel();
            slaveId = new NumericUpDown();
            label4 = new Label();
            groupBox1 = new GroupBox();
            numOfRegisters = new NumericUpDown();
            label3 = new Label();
            startRegister_1 = new NumericUpDown();
            startRegister = new NumericUpDown();
            label2 = new Label();
            startRegisterHex = new NumericUpDown();
            label1 = new Label();
            comboBox1 = new ComboBox();
            scanResultsGrid = new DataGridView();
            reg_adr = new DataGridViewTextBoxColumn();
            Response = new DataGridViewTextBoxColumn();
            Resp_x = new DataGridViewTextBoxColumn();
            menuStrip1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)slaveId).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numOfRegisters).BeginInit();
            ((System.ComponentModel.ISupportInitialize)startRegister_1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)startRegister).BeginInit();
            ((System.ComponentModel.ISupportInitialize)startRegisterHex).BeginInit();
            ((System.ComponentModel.ISupportInitialize)scanResultsGrid).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.AllowMerge = false;
            menuStrip1.Items.AddRange(new ToolStripItem[] { startToolStripMenuItem, stopToolStripMenuItem1, exportToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(334, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // startToolStripMenuItem
            // 
            startToolStripMenuItem.Image = Properties.Resources.icons8_play_48;
            startToolStripMenuItem.Name = "startToolStripMenuItem";
            startToolStripMenuItem.Size = new Size(59, 20);
            startToolStripMenuItem.Text = "Start";
            // 
            // stopToolStripMenuItem1
            // 
            stopToolStripMenuItem1.Enabled = false;
            stopToolStripMenuItem1.Image = Properties.Resources.icons8_stop_48;
            stopToolStripMenuItem1.Name = "stopToolStripMenuItem1";
            stopToolStripMenuItem1.Size = new Size(59, 20);
            stopToolStripMenuItem1.Text = "Stop";
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Image = Properties.Resources.icons8_download_resume_48;
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(68, 20);
            exportToolStripMenuItem.Text = "Export";
            // 
            // panel1
            // 
            panel1.Controls.Add(slaveId);
            panel1.Controls.Add(label4);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 24);
            panel1.Name = "panel1";
            panel1.Size = new Size(334, 47);
            panel1.TabIndex = 7;
            // 
            // slaveId
            // 
            slaveId.Font = new Font("Segoe UI", 9F);
            slaveId.Location = new Point(94, 12);
            slaveId.Maximum = new decimal(new int[] { 254, 0, 0, 0 });
            slaveId.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            slaveId.Name = "slaveId";
            slaveId.Size = new Size(69, 23);
            slaveId.TabIndex = 1;
            slaveId.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F);
            label4.Location = new Point(2, 14);
            label4.Name = "label4";
            label4.Size = new Size(50, 15);
            label4.TabIndex = 3;
            label4.Text = "Slave Id:";
            // 
            // groupBox1
            // 
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
            groupBox1.Location = new Point(0, 71);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(334, 109);
            groupBox1.TabIndex = 8;
            groupBox1.TabStop = false;
            groupBox1.Text = "Read setup";
            // 
            // numOfRegisters
            // 
            numOfRegisters.Font = new Font("Segoe UI", 9F);
            numOfRegisters.Location = new Point(101, 77);
            numOfRegisters.Maximum = new decimal(new int[] { 65534, 0, 0, 0 });
            numOfRegisters.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numOfRegisters.Name = "numOfRegisters";
            numOfRegisters.Size = new Size(69, 23);
            numOfRegisters.TabIndex = 1;
            numOfRegisters.Value = new decimal(new int[] { 10, 0, 0, 0 });
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
            // 
            // scanResultsGrid
            // 
            scanResultsGrid.AllowUserToAddRows = false;
            scanResultsGrid.AllowUserToDeleteRows = false;
            scanResultsGrid.AllowUserToResizeRows = false;
            scanResultsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            scanResultsGrid.Columns.AddRange(new DataGridViewColumn[] { reg_adr, Response, Resp_x });
            scanResultsGrid.Dock = DockStyle.Fill;
            scanResultsGrid.Location = new Point(0, 180);
            scanResultsGrid.Name = "scanResultsGrid";
            scanResultsGrid.RowHeadersVisible = false;
            scanResultsGrid.RowTemplate.Height = 20;
            scanResultsGrid.Size = new Size(334, 221);
            scanResultsGrid.TabIndex = 9;
            // 
            // reg_adr
            // 
            reg_adr.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            reg_adr.HeaderText = "Register Address";
            reg_adr.MinimumWidth = 120;
            reg_adr.Name = "reg_adr";
            reg_adr.ReadOnly = true;
            reg_adr.Width = 120;
            // 
            // Response
            // 
            Response.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Response.HeaderText = "Response";
            Response.Name = "Response";
            Response.ReadOnly = true;
            Response.Width = 82;
            // 
            // Resp_x
            // 
            Resp_x.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Resp_x.HeaderText = "Response HEX";
            Resp_x.Name = "Resp_x";
            Resp_x.ReadOnly = true;
            // 
            // AddressScan
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(334, 401);
            Controls.Add(scanResultsGrid);
            Controls.Add(groupBox1);
            Controls.Add(panel1);
            Controls.Add(menuStrip1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "AddressScan";
            Text = "AddressScan";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)slaveId).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numOfRegisters).EndInit();
            ((System.ComponentModel.ISupportInitialize)startRegister_1).EndInit();
            ((System.ComponentModel.ISupportInitialize)startRegister).EndInit();
            ((System.ComponentModel.ISupportInitialize)startRegisterHex).EndInit();
            ((System.ComponentModel.ISupportInitialize)scanResultsGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem startToolStripMenuItem;
        private ToolStripMenuItem stopToolStripMenuItem1;
        private ToolStripMenuItem exportToolStripMenuItem;
        private Panel panel1;
        private NumericUpDown slaveId;
        private Label label4;
        private GroupBox groupBox1;
        private NumericUpDown numOfRegisters;
        private Label label3;
        private NumericUpDown startRegister_1;
        private NumericUpDown startRegister;
        private Label label2;
        private NumericUpDown startRegisterHex;
        private Label label1;
        private ComboBox comboBox1;
        private DataGridView scanResultsGrid;
        private DataGridViewTextBoxColumn reg_adr;
        private DataGridViewTextBoxColumn Response;
        private DataGridViewTextBoxColumn Resp_x;
    }
}