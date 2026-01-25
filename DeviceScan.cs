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
    public partial class DeviceScan : Form
    {
        public DeviceScan()
        {
            InitializeComponent();
        }

        private void slaveId_ValueChanged(object sender, EventArgs e)
        {
            int startValue = (int)startId.Value;
            int endValue = (int)endId.Value;
            if (startValue > endValue)
            {
                endId.Value = startValue;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int startValue = (int)startId.Value;
            int endValue = (int)endId.Value;
            if (startValue > endValue)
            {
                endId.Value = startValue;
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void stopToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }
    }
}
