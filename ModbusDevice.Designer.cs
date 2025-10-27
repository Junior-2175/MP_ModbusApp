namespace MP_ModbusApp
{
    partial class ModbusDevice
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
            slaveId = new NumericUpDown();
            label4 = new Label();
            tabPanel1 = new TabControl();
            contextMenuStrip1 = new ContextMenuStrip(components);
            addToolStripMenuItem = new ToolStripMenuItem();
            removeToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            renameToolStripMenuItem = new ToolStripMenuItem();
            panel1 = new Panel();
            menuStrip1 = new MenuStrip();
            deviceToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            newReadingsToolStripMenuItem = new ToolStripMenuItem();
            pollingToolStripMenuItem = new ToolStripMenuItem();
            startToolStripMenuItem = new ToolStripMenuItem();
            stopToolStripMenuItem = new ToolStripMenuItem();
            txtRenameTab = new TextBox();
            ((System.ComponentModel.ISupportInitialize)slaveId).BeginInit();
            contextMenuStrip1.SuspendLayout();
            panel1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
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
            // tabPanel1
            // 
            tabPanel1.ContextMenuStrip = contextMenuStrip1;
            tabPanel1.Dock = DockStyle.Fill;
            tabPanel1.Location = new Point(0, 71);
            tabPanel1.Name = "tabPanel1";
            tabPanel1.SelectedIndex = 0;
            tabPanel1.Size = new Size(450, 410);
            tabPanel1.TabIndex = 5;
            tabPanel1.DoubleClick += tabPanel1_DoubleClick;
            tabPanel1.MouseDown += tabPanel1_MouseDown;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { addToolStripMenuItem, removeToolStripMenuItem, toolStripSeparator1, renameToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(211, 76);
            // 
            // addToolStripMenuItem
            // 
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Oemplus;
            addToolStripMenuItem.Size = new Size(210, 22);
            addToolStripMenuItem.Text = "Add";
            addToolStripMenuItem.Click += addToolStripMenuItem_Click;
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.OemMinus;
            removeToolStripMenuItem.Size = new Size(210, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += removeToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(207, 6);
            // 
            // renameToolStripMenuItem
            // 
            renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            renameToolStripMenuItem.ShortcutKeys = Keys.F2;
            renameToolStripMenuItem.Size = new Size(210, 22);
            renameToolStripMenuItem.Text = "Rename";
            renameToolStripMenuItem.Click += renameToolStripMenuItem_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(slaveId);
            panel1.Controls.Add(label4);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 24);
            panel1.Name = "panel1";
            panel1.Size = new Size(450, 47);
            panel1.TabIndex = 6;
            // 
            // menuStrip1
            // 
            menuStrip1.AllowMerge = false;
            menuStrip1.Items.AddRange(new ToolStripItem[] { deviceToolStripMenuItem, pollingToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(450, 24);
            menuStrip1.TabIndex = 7;
            menuStrip1.Text = "menuStrip1";
            // 
            // deviceToolStripMenuItem
            // 
            deviceToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { saveToolStripMenuItem, newReadingsToolStripMenuItem });
            deviceToolStripMenuItem.Name = "deviceToolStripMenuItem";
            deviceToolStripMenuItem.Size = new Size(54, 20);
            deviceToolStripMenuItem.Text = "Device";
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveToolStripMenuItem.Size = new Size(228, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // newReadingsToolStripMenuItem
            // 
            newReadingsToolStripMenuItem.Name = "newReadingsToolStripMenuItem";
            newReadingsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Oemplus;
            newReadingsToolStripMenuItem.Size = new Size(228, 22);
            newReadingsToolStripMenuItem.Text = "New readings";
            newReadingsToolStripMenuItem.Click += newReadingsToolStripMenuItem_Click;
            // 
            // pollingToolStripMenuItem
            // 
            pollingToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { startToolStripMenuItem, stopToolStripMenuItem });
            pollingToolStripMenuItem.Name = "pollingToolStripMenuItem";
            pollingToolStripMenuItem.Size = new Size(56, 20);
            pollingToolStripMenuItem.Text = "Polling";
            // 
            // startToolStripMenuItem
            // 
            startToolStripMenuItem.Name = "startToolStripMenuItem";
            startToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.S;
            startToolStripMenuItem.Size = new Size(142, 22);
            startToolStripMenuItem.Text = "Start";
            // 
            // stopToolStripMenuItem
            // 
            stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            stopToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.P;
            stopToolStripMenuItem.Size = new Size(142, 22);
            stopToolStripMenuItem.Text = "Pause";
            // 
            // txtRenameTab
            // 
            txtRenameTab.Location = new Point(351, 84);
            txtRenameTab.Name = "txtRenameTab";
            txtRenameTab.Size = new Size(100, 23);
            txtRenameTab.TabIndex = 8;
            txtRenameTab.KeyDown += txtRenameTab_KeyDown;
            // 
            // ModbusDevice
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(450, 481);
            Controls.Add(tabPanel1);
            Controls.Add(panel1);
            Controls.Add(menuStrip1);
            Controls.Add(txtRenameTab);
            Name = "ModbusDevice";
            Text = "New Device";
            Load += ModbusGroup_Load;
            Resize += ModbusDevice_Resize;
            ((System.ComponentModel.ISupportInitialize)slaveId).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private NumericUpDown slaveId;
        private Label label4;
        private TabControl tabPanel1;
        private Panel panel1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem deviceToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem removeToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem renameToolStripMenuItem;
        private ToolStripMenuItem pollingToolStripMenuItem;
        private ToolStripMenuItem startToolStripMenuItem;
        private ToolStripMenuItem stopToolStripMenuItem;
        private TextBox txtRenameTab;
        private ToolStripMenuItem newReadingsToolStripMenuItem;
    }
}