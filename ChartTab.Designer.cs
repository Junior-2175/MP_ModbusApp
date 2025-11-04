namespace MP_ModbusApp
{
    partial class ChartTab
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartTab));
            chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            toolStrip1 = new ToolStrip();
            btnClearLog = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnExportToCsv = new ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)chart1).BeginInit();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            chart1.ChartAreas.Add(chartArea1);
            chart1.Dock = DockStyle.Fill;
            legend1.Name = "Legend1";
            chart1.Legends.Add(legend1);
            chart1.Location = new Point(0, 25);
            chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            chart1.Series.Add(series1);
            chart1.Size = new Size(332, 413);
            chart1.TabIndex = 0;
            chart1.Text = "chart1";
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnClearLog, toolStripSeparator2, btnExportToCsv });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(332, 25);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnClearLog
            // 
            btnClearLog.Image = (Image)resources.GetObject("btnClearLog.Image");
            btnClearLog.ImageTransparentColor = Color.Magenta;
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(54, 22);
            btnClearLog.Text = "Clear";
            btnClearLog.ToolTipText = "Clear logs";
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
            // 
            // ChartTab
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(chart1);
            Controls.Add(toolStrip1);
            Name = "ChartTab";
            Size = new Size(332, 438);
            ((System.ComponentModel.ISupportInitialize)chart1).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private ToolStrip toolStrip1;
        private ToolStripButton btnClearLog;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton btnExportToCsv;
    }
}
