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
            startRegister_1 = new NumericUpDown();
            dataGridView1 = new DataGridView();
            Name = new DataGridViewTextBoxColumn();
            RegisterNumber = new DataGridViewTextBoxColumn();
            Value = new DataGridViewTextBoxColumn();
            timer1 = new System.Windows.Forms.Timer(components);
            lblTabError = new Label();
            ((System.ComponentModel.ISupportInitialize)numOfRegisters).BeginInit();
            ((System.ComponentModel.ISupportInitialize)startRegister).BeginInit();
            ((System.ComponentModel.ISupportInitialize)startRegisterHex).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)startRegister_1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
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
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Name, RegisterNumber, Value });
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 109);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Size = new Size(332, 329);
            dataGridView1.TabIndex = 6;
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
            Value.HeaderText = "Value";
            Value.Name = "Value";
            Value.SortMode = DataGridViewColumnSortMode.NotSortable;
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
        private DataGridViewTextBoxColumn Name;
        private DataGridViewTextBoxColumn RegisterNumber;
        private DataGridViewTextBoxColumn Value;
        private NumericUpDown startRegister_1;
        private Label lblTabError;
    }
}
