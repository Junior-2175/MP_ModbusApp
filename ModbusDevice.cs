using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP_ModbusApp
{
    public partial class ModbusDevice : Form
    {

        private TabPage tabToRename = null;
        private int tabNo = 0;
        public ModbusDevice()
        {
            InitializeComponent();
        }

        private void ModbusGroup_Load(object sender, EventArgs e)
        {
            TabPage newTab = new TabPage();
            tabNo = tabNo + 1;
            tabPanel1.SelectedTab = newTab;
            newTab.Text = "Readings " + tabNo;
            newTab.Controls.Add(new ReadingsTab() { Dock = DockStyle.Fill });
            tabPanel1.TabPages.Add(newTab);
            txtRenameTab.Visible = false;
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage newTab = new TabPage();
            tabNo = tabNo + 1;
            tabPanel1.SelectedTab = newTab;
            newTab.Text = "Readings " + tabNo;
            newTab.Controls.Add(new ReadingsTab() { Dock = DockStyle.Fill });
            tabPanel1.TabPages.Add(newTab);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabPanel1.SelectedTab != null)
            {
                tabPanel1.TabPages.Remove(tabPanel1.SelectedTab);
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabToRename = tabPanel1.SelectedTab;
            if (tabToRename == null) return;

            Rectangle tabRect = tabPanel1.GetTabRect(tabPanel1.SelectedIndex);
            Point textCords = new Point(
                tabPanel1.Left + tabRect.Left,
                tabPanel1.Top + tabRect.Top);
            txtRenameTab.Location = textCords;
            txtRenameTab.Size = tabRect.Size;
            txtRenameTab.Text = tabToRename.Text;

            txtRenameTab.Visible = true;
            txtRenameTab.BringToFront();
            txtRenameTab.Focus();
            txtRenameTab.SelectAll();
        }

        private void tabPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tabPanel1.TabCount; i++)
                {
                    Rectangle tabRect = tabPanel1.GetTabRect(i);
                    if (tabRect.Contains(e.Location))
                    {
                        tabPanel1.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void txtRenameTab_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AllowRename();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                CancelRename();
            }
        }

        private void txtRenameTab_LostFocus(object sender, EventArgs e)
        {
            if (tabToRename != null && txtRenameTab.Visible)
            {
                AllowRename();
            }
        }

        private void AllowRename()
        {
            if (tabToRename == null) return;

            if (!string.IsNullOrWhiteSpace(txtRenameTab.Text))
            {
                tabToRename.Text = txtRenameTab.Text;
            }

            txtRenameTab.Visible = false;
            tabToRename = null;
            txtRenameTab.Parent = this;
        }

        private void CancelRename()
        {
            txtRenameTab.Visible = false;
            tabToRename = null;
            txtRenameTab.Parent = this;
        }
    }
}
