namespace MP_ModbusApp
{
    partial class CommunicationLogWindow
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommunicationLogWindow));
            dgvLog = new DataGridView();
            chkStopOnError = new CheckBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            copyToolStripMenuItem = new ToolStripMenuItem();
            btnStartLogging = new ToolStripButton();
            btnStopLogging = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnClearLog = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnExportToCsv = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStrip1 = new ToolStrip();
            lblFilter = new ToolStripLabel();
            cboDeviceFilter = new ToolStripComboBox();
            btnClearFilter = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            _bindingSource = new BindingSource(components);
            ((System.ComponentModel.ISupportInitialize)dgvLog).BeginInit();
            contextMenuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_bindingSource).BeginInit();
            SuspendLayout();
            // 
            // dgvLog
            // 
            dgvLog.AllowUserToAddRows = false;
            dgvLog.AllowUserToDeleteRows = false;
            dgvLog.AllowUserToResizeRows = false;
            dgvLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvLog.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvLog.Dock = DockStyle.Fill;
            dgvLog.Location = new Point(0, 25);
            dgvLog.Name = "dgvLog";
            dgvLog.RowTemplate.Height = 15;
            dgvLog.Size = new Size(599, 245);
            dgvLog.TabIndex = 0;
            dgvLog.MouseDown += dgvLog_MouseDown;
            // 
            // chkStopOnError
            // 
            chkStopOnError.AutoSize = true;
            chkStopOnError.Location = new Point(500, 3);
            chkStopOnError.Name = "chkStopOnError";
            chkStopOnError.Size = new Size(95, 19);
            chkStopOnError.TabIndex = 2;
            chkStopOnError.Text = "Stop on Error";
            chkStopOnError.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { copyToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(145, 26);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            copyToolStripMenuItem.Size = new Size(144, 22);
            copyToolStripMenuItem.Text = "Copy";
            copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
            // 
            // btnStartLogging
            // 
            btnStartLogging.Image = (Image)resources.GetObject("btnStartLogging.Image");
            btnStartLogging.ImageTransparentColor = Color.Magenta;
            btnStartLogging.Name = "btnStartLogging";
            btnStartLogging.Size = new Size(51, 22);
            btnStartLogging.Text = "Start";
            btnStartLogging.ToolTipText = "Start logging";
            btnStartLogging.Click += btnStartLogging_Click;
            // 
            // btnStopLogging
            // 
            btnStopLogging.Image = (Image)resources.GetObject("btnStopLogging.Image");
            btnStopLogging.ImageTransparentColor = Color.Magenta;
            btnStopLogging.Name = "btnStopLogging";
            btnStopLogging.Size = new Size(51, 22);
            btnStopLogging.Text = "Stop";
            btnStopLogging.ToolTipText = "Stop logging";
            btnStopLogging.Click += btnStopLogging_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // btnClearLog
            // 
            btnClearLog.Image = (Image)resources.GetObject("btnClearLog.Image");
            btnClearLog.ImageTransparentColor = Color.Magenta;
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(54, 22);
            btnClearLog.Text = "Clear";
            btnClearLog.ToolTipText = "Clear logs";
            btnClearLog.Click += btnClearLog_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // btnExportToCsv
            // 
            btnExportToCsv.Image = (Image)resources.GetObject("btnExportToCsv.Image");
            btnExportToCsv.ImageTransparentColor = Color.Magenta;
            btnExportToCsv.Name = "btnExportToCsv";
            btnExportToCsv.Size = new Size(60, 22);
            btnExportToCsv.Text = "Export";
            btnExportToCsv.ToolTipText = "Export to CSV";
            btnExportToCsv.Click += btnExportToCsv_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnStartLogging, btnStopLogging, toolStripSeparator1, btnClearLog, toolStripSeparator2, btnExportToCsv, toolStripSeparator3, lblFilter, cboDeviceFilter, btnClearFilter, toolStripSeparator4 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(599, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // lblFilter
            // 
            lblFilter.Name = "lblFilter";
            lblFilter.Size = new Size(89, 22);
            lblFilter.Text = "Filter by device:";
            // 
            // cboDeviceFilter
            // 
            cboDeviceFilter.Name = "cboDeviceFilter";
            cboDeviceFilter.Size = new Size(121, 25);
            cboDeviceFilter.Sorted = true;
            cboDeviceFilter.SelectedIndexChanged += cboDeviceFilter_SelectedIndexChanged;
            // 
            // btnClearFilter
            // 
            btnClearFilter.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnClearFilter.Image = (Image)resources.GetObject("btnClearFilter.Image");
            btnClearFilter.ImageTransparentColor = Color.Magenta;
            btnClearFilter.Name = "btnClearFilter";
            btnClearFilter.Size = new Size(38, 22);
            btnClearFilter.Text = "Clear";
            btnClearFilter.ToolTipText = "Clear";
            btnClearFilter.Click += btnClearFilter_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 25);
            // 
            // _bindingSource
            // 
            _bindingSource.Filter = "True";
            // 
            // CommunicationLogWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(599, 270);
            Controls.Add(chkStopOnError);
            Controls.Add(dgvLog);
            Controls.Add(toolStrip1);
            Name = "CommunicationLogWindow";
            Text = "Communication";
            Load += CommunicationLogWindow_Load;
            ((System.ComponentModel.ISupportInitialize)dgvLog).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_bindingSource).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvLog;
        private CheckBox chkStopOnError;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripButton btnStartLogging;
        private ToolStripButton btnStopLogging;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnClearLog;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton btnExportToCsv;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStrip toolStrip1;
        private ToolStripLabel lblFilter;
        private ToolStripComboBox cboDeviceFilter;
        private ToolStripButton btnClearFilter;
        private ToolStripSeparator toolStripSeparator4;
        private BindingSource _bindingSource;
    }
}