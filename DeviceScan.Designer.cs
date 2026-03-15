namespace MP_ModbusApp
{
    partial class DeviceScan
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceScan));
            menuStrip1 = new MenuStrip();
            startToolStripMenuItem = new ToolStripMenuItem();
            stopToolStripMenuItem1 = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            label4 = new Label();
            startId = new NumericUpDown();
            panel1 = new Panel();
            endId = new NumericUpDown();
            label1 = new Label();
            scanResultsGrid = new DataGridView();
            Slave_Id = new DataGridViewTextBoxColumn();
            Response = new DataGridViewTextBoxColumn();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)startId).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)endId).BeginInit();
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
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // startToolStripMenuItem
            // 
            startToolStripMenuItem.Image = Properties.Resources.icons8_play_48;
            startToolStripMenuItem.Name = "startToolStripMenuItem";
            startToolStripMenuItem.Size = new Size(59, 20);
            startToolStripMenuItem.Text = "Start";
            startToolStripMenuItem.Click += startToolStripMenuItem_Click;
            // 
            // stopToolStripMenuItem1
            // 
            stopToolStripMenuItem1.Enabled = false;
            stopToolStripMenuItem1.Image = Properties.Resources.icons8_stop_48;
            stopToolStripMenuItem1.Name = "stopToolStripMenuItem1";
            stopToolStripMenuItem1.Size = new Size(59, 20);
            stopToolStripMenuItem1.Text = "Stop";
            stopToolStripMenuItem1.Click += stopToolStripMenuItem1_Click;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Image = Properties.Resources.icons8_download_resume_48;
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(68, 20);
            exportToolStripMenuItem.Text = "Export";
            exportToolStripMenuItem.Click += exportToolStripMenuItem_Click_1;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F);
            label4.Location = new Point(2, 14);
            label4.Name = "label4";
            label4.Size = new Size(47, 15);
            label4.TabIndex = 3;
            label4.Text = "Start Id:";
            // 
            // startId
            // 
            startId.Font = new Font("Segoe UI", 9F);
            startId.Location = new Point(94, 12);
            startId.Maximum = new decimal(new int[] { 254, 0, 0, 0 });
            startId.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            startId.Name = "startId";
            startId.Size = new Size(69, 23);
            startId.TabIndex = 1;
            startId.Value = new decimal(new int[] { 1, 0, 0, 0 });
            startId.ValueChanged += slaveId_ValueChanged;
            // 
            // panel1
            // 
            panel1.Controls.Add(endId);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(startId);
            panel1.Controls.Add(label4);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 24);
            panel1.Name = "panel1";
            panel1.Size = new Size(334, 76);
            panel1.TabIndex = 7;
            // 
            // endId
            // 
            endId.Font = new Font("Segoe UI", 9F);
            endId.Location = new Point(94, 41);
            endId.Maximum = new decimal(new int[] { 254, 0, 0, 0 });
            endId.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            endId.Name = "endId";
            endId.Size = new Size(69, 23);
            endId.TabIndex = 4;
            endId.Value = new decimal(new int[] { 10, 0, 0, 0 });
            endId.ValueChanged += numericUpDown1_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F);
            label1.Location = new Point(2, 43);
            label1.Name = "label1";
            label1.Size = new Size(47, 15);
            label1.TabIndex = 5;
            label1.Text = "Stop Id:";
            // 
            // scanResultsGrid
            // 
            scanResultsGrid.AllowUserToAddRows = false;
            scanResultsGrid.AllowUserToDeleteRows = false;
            scanResultsGrid.AllowUserToResizeRows = false;
            scanResultsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            scanResultsGrid.Columns.AddRange(new DataGridViewColumn[] { Slave_Id, Response });
            scanResultsGrid.Dock = DockStyle.Fill;
            scanResultsGrid.Location = new Point(0, 100);
            scanResultsGrid.Name = "scanResultsGrid";
            scanResultsGrid.RowHeadersVisible = false;
            scanResultsGrid.RowTemplate.Height = 20;
            scanResultsGrid.Size = new Size(334, 301);
            scanResultsGrid.TabIndex = 8;
            // 
            // Slave_Id
            // 
            Slave_Id.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Slave_Id.HeaderText = "Slave Id";
            Slave_Id.Name = "Slave_Id";
            Slave_Id.ReadOnly = true;
            Slave_Id.Width = 72;
            // 
            // Response
            // 
            Response.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Response.HeaderText = "Response";
            Response.Name = "Response";
            Response.ReadOnly = true;
            // 
            // DeviceScan
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(334, 401);
            Controls.Add(scanResultsGrid);
            Controls.Add(panel1);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Name = "DeviceScan";
            Text = "DeviceScan";
            FormClosing += DeviceScan_FormClosing;
            Load += DeviceScan_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)startId).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)endId).EndInit();
            ((System.ComponentModel.ISupportInitialize)scanResultsGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private Label label4;
        private NumericUpDown startId;
        private Panel panel1;
        private NumericUpDown endId;
        private Label label1;
        private ToolStripMenuItem startToolStripMenuItem;
        private ToolStripMenuItem stopToolStripMenuItem1;
        private ToolStripMenuItem exportToolStripMenuItem;
        private DataGridView scanResultsGrid;
        private DataGridViewTextBoxColumn Slave_Id;
        private DataGridViewTextBoxColumn Response;
    }
}