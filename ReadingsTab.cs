using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MP_ModbusApp
{
    public partial class ReadingsTab : UserControl
    {
        private bool _isUpdatingValues = false;

        public ReadingsTab()
        {

            InitializeComponent();
        }

        private void ReadingsTab_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 2;
            dataGridView1.RowCount = (int)numOfRegisters.Value;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            datagridUpdate();
        }

        private void startRegister_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingValues) return;
            _isUpdatingValues = true;

            try
            {
                startRegisterHex.Value = startRegister.Value;
                updateStertRegister();
                datagridUpdate();
            }
            finally
            {
                _isUpdatingValues = false;
            }
        }
        private void startRegisterHex_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingValues) return;
            _isUpdatingValues = true;

            try
            {
                startRegister.Value = startRegisterHex.Value;
                updateStertRegister();
                datagridUpdate();
            }
            finally
            {
                _isUpdatingValues = false;
            }
        }
        private void updateStertRegister()
        {
            decimal requestedFuncionNo = 0;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    requestedFuncionNo = 0;
                    break;
                case 1:
                    requestedFuncionNo = 1;
                    break;
                case 2:
                    requestedFuncionNo = 4;
                    break;
                case 3:
                    requestedFuncionNo = 3;
                    break;
                default:
                    break;
            }

            if (startRegister.Value > 0 && startRegister.Value < 10000)
            {
                startRegister_1.Value = requestedFuncionNo * 10000 + startRegister.Value + 1;
            }
            else
            {
                startRegister_1.Value = requestedFuncionNo * 100000 + startRegister.Value + 1;
            }

            }

        private void numOfRegisters_ValueChanged(object sender, EventArgs e)
        {
            datagridUpdate();
        }

        private void datagridUpdate()
        {
            dataGridView1.RowCount = (int)numOfRegisters.Value;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].IsNewRow) continue;

                dataGridView1.Rows[i].Cells["RegisterNumber"].Value = i + (int)startRegister.Value;
                dataGridView1.Rows[i].Cells["Name"].Value = "Register_" + (i + startRegister.Value);
            }
            


        }

        private void startRegister_1_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingValues) return;
            _isUpdatingValues = true;
           
            try
            {
                string originalString = (startRegister_1.Value - 1).ToString();
                string functionString = originalString.Substring(0,1);
                string registerString = originalString.Substring(1);
                decimal registerStringDouble = Convert.ToDecimal(registerString);
                string requestedFuncionNo = "";
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        if (startRegister_1.Value > 65536 || startRegister_1.Value < 1)
                        {
                            MessageBox.Show("Invalid Register Number. Register Number must be 1-65536");
                            return;
                        }
                        else
                        {
                            registerStringDouble = startRegister_1.Value - 1;
                        }
                            break;
                        
                    case 1:
                        requestedFuncionNo = "1";
                        break;
                    case 2:
                        requestedFuncionNo = "4";
                        break;
                    case 3:
                        requestedFuncionNo = "3";
                        break;
                    default:
                        break;
                }
                if (functionString != requestedFuncionNo && comboBox1.SelectedIndex !=0 || (registerStringDouble<0 && registerStringDouble >65535))
                {
                    MessageBox.Show("Invalid Register Number for the selected Function Code. Adjusting to match Function Code.");
                    return;
                }
                startRegister.Value = registerStringDouble;
                startRegisterHex.Value = registerStringDouble;
                datagridUpdate();
            }
            finally
            {
                _isUpdatingValues = false;
            }
        }


        // Metody publiczne do pobierania konfiguracji z tej kontrolki
        public int GetFunctionCode()
        {
            return comboBox1.SelectedIndex;
        }

        public int GetStartAddress()
        {
            return (int)startRegister.Value;
        }

        public int GetQuantity()
        {
            return (int)numOfRegisters.Value;
        }

        public DataGridViewRowCollection GetDataGridViewRows()
        {
            return dataGridView1.Rows;
        }

        // Metody publiczne do ustawiania konfiguracji (dla wczytywania)
        public void SetConfiguration(int funcCode, int startAddr, int quantity)
        {
            _isUpdatingValues = true; // Zapobiegamy pętlom zdarzeń
            try
            {
                comboBox1.SelectedIndex = funcCode;
                startRegister.Value = startAddr;
                numOfRegisters.Value = quantity;
                datagridUpdate(); // To zaktualizuje UI na podstawie nowych wartości
            }
            finally
            {
                _isUpdatingValues = false;
            }
        }

        public void SetRegisterDefinitions(List<Tuple<int, string>> registers)
        {
            foreach (var regDef in registers)
            {
                // Znajdź wiersz pasujący do numeru rejestru
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["RegisterNumber"].Value != null && (int)row.Cells["RegisterNumber"].Value == regDef.Item1)
                    {
                        row.Cells["Name"].Value = regDef.Item2;
                        break;
                    }
                }
            }
        }

    }
}