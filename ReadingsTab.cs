using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace MP_ModbusApp
{
    public partial class ReadingsTab : UserControl
    {
        public event EventHandler<ChartDataUpdateEventArgs> ChartDataUpdated;
        private ushort[] _prevRawData = null;

        private Dictionary<int, DateTime> _lastChangeTimes = new Dictionary<int, DateTime>();
        private const int FADE_DURATION_MS = 1000;
        private readonly Color _flashColor = ColorTranslator.FromHtml("#00A2E8");

        // Event triggered when a user edits a cell to write a value to the device
        public event EventHandler<WriteRequestedEventArgs> WriteValueRequested;

        private bool _isUpdatingValues = false;
        private ushort[] _rawData = null;

        public enum DisplayFormat
        {
            // 16-bit
            Unsigned16,
            Signed16,
            Hex16,
            Binary16,
            Bool16,
            ASCII,

            // 32-bit (BE = Big-Endian, LE = Little-Endian, BS = Byte-Swapped)
            Unsigned32_BE,
            Signed32_BE,
            Float32_BE,
            Unsigned32_LE,
            Signed32_LE,
            Float32_LE,
            Unsigned32_BE_BS,
            Signed32_BE_BS,
            Float32_BE_BS,
            Unsigned32_LE_BS,
            Signed32_LE_BS,
            Float32_LE_BS,
            ASCII32_BE,
            ASCII32_LE,
            ASCII32_BE_BS,
            ASCII32_LE_BS,
            Hex32_BE,
            Hex32_LE,
            Hex32_BE_BS,
            Hex32_LE_BS,

            // 64-bit
            Unsigned64_BE,
            Signed64_BE,
            Double64_BE,
            Unsigned64_LE,
            Signed64_LE,
            Double64_LE,
            Unsigned64_BE_BS,
            Signed64_BE_BS,
            Float64_BE_BS,
            Unsigned64_LE_BS,
            Signed64_LE_BS,
            Float64_LE_BS,
            ASCII64_BE,
            ASCII64_LE,
            ASCII64_BE_BS,
            ASCII64_LE_BS,
            Hex64_BE,
            Hex64_LE,
            Hex64_BE_BS,
            Hex64_LE_BS
        }

        public ReadingsTab()
        {
            InitializeComponent();

            // Link menu items to DisplayFormat enums
            this.unsignedToolStripMenuItem.Tag = DisplayFormat.Unsigned16;
            this.signedToolStripMenuItem.Tag = DisplayFormat.Signed16;
            this.binaryToolStripMenuItem.Tag = DisplayFormat.Binary16;
            this.hexToolStripMenuItem.Tag = DisplayFormat.Hex16;
            this.boolToolStripMenuItem.Tag = DisplayFormat.Bool16;
            this.aSCIIToolStripMenuItem.Tag = DisplayFormat.ASCII;

            this.bigendianToolStripMenuItem.Tag = DisplayFormat.Unsigned32_BE;
            this.littleendianToolStripMenuItem.Tag = DisplayFormat.Unsigned32_LE;
            this.bigendianToolStripMenuItem2.Tag = DisplayFormat.Signed32_BE;
            this.littleendianToolStripMenuItem3.Tag = DisplayFormat.Signed32_LE;
            this.bigendianToolStripMenuItem3.Tag = DisplayFormat.Float32_BE;
            this.littleendianToolStripMenuItem2.Tag = DisplayFormat.Float32_LE;

            this.bigendianToolStripMenuItem6.Tag = DisplayFormat.Unsigned64_BE;
            this.littleendianToolStripMenuItem6.Tag = DisplayFormat.Unsigned64_LE;
            this.bigendianByteSwapToolStripMenuItem4.Tag = DisplayFormat.Unsigned64_BE_BS;
            this.littleendianByteSwapToolStripMenuItem4.Tag = DisplayFormat.Unsigned64_LE_BS;
            this.bigendianToolStripMenuItem7.Tag = DisplayFormat.Signed64_BE;
            this.littleendianToolStripMenuItem7.Tag = DisplayFormat.Signed64_LE;
            this.bigendianByteSwapToolStripMenuItem5.Tag = DisplayFormat.Signed64_BE_BS;
            this.littleendianByteSwapToolStripMenuItem5.Tag = DisplayFormat.Signed64_LE_BS;
            this.bigendianToolStripMenuItem8.Tag = DisplayFormat.Double64_BE;
            this.littleendianToolStripMenuItem8.Tag = DisplayFormat.Double64_LE;
            this.bigendianByteSwapToolStripMenuItem6.Tag = DisplayFormat.Float64_BE_BS;
            this.littleendianByteSwapToolStripMenuItem6.Tag = DisplayFormat.Float64_LE_BS;
            this.bigendianToolStripMenuItem10.Tag = DisplayFormat.ASCII64_BE;
            this.littleendianToolStripMenuItem10.Tag = DisplayFormat.ASCII64_LE;
            this.bigendianByteSwapToolStripMenuItem8.Tag = DisplayFormat.ASCII64_BE_BS;
            this.littleendianByteSwapToolStripMenuItem8.Tag = DisplayFormat.ASCII64_LE_BS;

            this.bigendianToolStripMenuItem1.Click += new System.EventHandler(this.unsigned32BEBSToolStripMenuItem_Click);
            this.bigendianToolStripMenuItem1.Tag = DisplayFormat.Unsigned32_BE_BS;
            this.littleendianToolStripMenuItem1.Click += new System.EventHandler(this.unsigned32LEBSToolStripMenuItem_Click);
            this.littleendianToolStripMenuItem1.Tag = DisplayFormat.Unsigned32_LE_BS;

            this.bigendianByteSwapToolStripMenuItem.Click += new System.EventHandler(this.signed32BEBSToolStripMenuItem_Click);
            this.bigendianByteSwapToolStripMenuItem.Tag = DisplayFormat.Signed32_BE_BS;
            this.littleendianByteSwapToolStripMenuItem1.Click += new System.EventHandler(this.signed32LEBSToolStripMenuItem_Click);
            this.littleendianByteSwapToolStripMenuItem1.Tag = DisplayFormat.Signed32_LE_BS;

            this.bigendianByteSwapToolStripMenuItem1.Click += new System.EventHandler(this.float32BEBSToolStripMenuItem_Click);
            this.bigendianByteSwapToolStripMenuItem1.Tag = DisplayFormat.Float32_BE_BS;
            this.littleendianByteSwapToolStripMenuItem.Click += new System.EventHandler(this.float32LEBSToolStripMenuItem_Click);
            this.littleendianByteSwapToolStripMenuItem.Tag = DisplayFormat.Float32_LE_BS;

            this.bigendianToolStripMenuItem4.Click += new System.EventHandler(this.hex32BEToolStripMenuItem_Click);
            this.bigendianToolStripMenuItem4.Tag = DisplayFormat.Hex32_BE;
            this.littleendianToolStripMenuItem5.Click += new System.EventHandler(this.hex32LEToolStripMenuItem_Click);
            this.littleendianToolStripMenuItem5.Tag = DisplayFormat.Hex32_LE;
            this.bigendianByteSwapToolStripMenuItem2.Click += new System.EventHandler(this.hex32BEBSToolStripMenuItem_Click);
            this.bigendianByteSwapToolStripMenuItem2.Tag = DisplayFormat.Hex32_BE_BS;
            this.littleendianByteSwapToolStripMenuItem2.Click += new System.EventHandler(this.hex32LEBSToolStripMenuItem_Click);
            this.littleendianByteSwapToolStripMenuItem2.Tag = DisplayFormat.Hex32_LE_BS;

            this.bigendianToolStripMenuItem5.Click += new System.EventHandler(this.ascii32BEToolStripMenuItem_Click);
            this.bigendianToolStripMenuItem5.Tag = DisplayFormat.ASCII32_BE;
            this.littleendianToolStripMenuItem4.Click += new System.EventHandler(this.ascii32LEToolStripMenuItem_Click);
            this.littleendianToolStripMenuItem4.Tag = DisplayFormat.ASCII32_LE;
            this.bigendianByteSwapToolStripMenuItem3.Click += new System.EventHandler(this.ascii32BEBSToolStripMenuItem_Click);
            this.bigendianByteSwapToolStripMenuItem3.Tag = DisplayFormat.ASCII32_BE_BS;
            this.littleendianByteSwapToolStripMenuItem3.Click += new System.EventHandler(this.ascii32LEBSToolStripMenuItem_Click);
            this.littleendianByteSwapToolStripMenuItem3.Tag = DisplayFormat.ASCII32_LE_BS;

            this.bigendianToolStripMenuItem9.Click += new System.EventHandler(this.hex64BEToolStripMenuItem_Click);
            this.bigendianToolStripMenuItem9.Tag = DisplayFormat.Hex64_BE;
            this.littleendianToolStripMenuItem9.Click += new System.EventHandler(this.hex64LEToolStripMenuItem_Click);
            this.littleendianToolStripMenuItem9.Tag = DisplayFormat.Hex64_LE;
            this.bigendianByteSwapToolStripMenuItem7.Click += new System.EventHandler(this.hex64BEBSToolStripMenuItem_Click);
            this.bigendianByteSwapToolStripMenuItem7.Tag = DisplayFormat.Hex64_BE_BS;
            this.littleendianByteSwapToolStripMenuItem7.Click += new System.EventHandler(this.hex64LEBSToolStripMenuItem_Click);
            this.littleendianByteSwapToolStripMenuItem7.Tag = DisplayFormat.Hex64_LE_BS;

            this.dataGridView1.CellValueChanged += new DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            this.dataGridView1.CurrentCellDirtyStateChanged += new EventHandler(this.dataGridView1_CurrentCellDirtyStateChanged);
            this.dataGridView1.CellEndEdit += new DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
        }

        private void ReadingsTab_Load(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
            {
                comboBox1.SelectedIndex = 2;
            }
            dataGridView1.RowCount = (int)numOfRegisters.Value;
            lblTabError.Visible = false;
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty && dataGridView1.CurrentCell.ColumnIndex == Chart.Index)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == Chart.Index)
            {
                OnChartDataUpdated(new ChartDataUpdateEventArgs(GetChartData()));
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != Value.Index) return;

            int funcCode = GetFunctionCode();

            // Writing is only allowed for Coils and Holding Registers
            if (funcCode != 0 && funcCode != 2)
            {
                MessageBox.Show("Writing is only allowed for Coils (0x) and Holding Registers (4x).", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RefreshDisplayValues();
                return;
            }

            int startAddressForTab = GetStartAddress();
            ushort transactionAddress = (ushort)(startAddressForTab + e.RowIndex);

            string valueString = dataGridView1.Rows[e.RowIndex].Cells[Value.Index].Value?.ToString() ?? "";
            DisplayFormat format = (DisplayFormat)dataGridView1.Rows[e.RowIndex].Cells["DisplayFormatColumn"].Value;
            int regsNeeded = GetRegistersForFormat(format);

            if (string.IsNullOrWhiteSpace(valueString)) return;

            // Only allow editing the first register of a multi-register group
            if (regsNeeded > 1 && dataGridView1.Rows[e.RowIndex].Cells["Value"].ReadOnly)
            {
                MessageBox.Show("Editing is only allowed on the first register of a multi-register group.", "Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RefreshDisplayValues();
                return;
            }

            dataGridView1.Rows[e.RowIndex].Cells[Value.Index].Value = "---";

            OnWriteValueRequested(new WriteRequestedEventArgs(
                funcCode,
                transactionAddress,
                valueString,
                this,
                format,
                e.RowIndex
            ));
        }

        protected virtual void OnChartDataUpdated(ChartDataUpdateEventArgs e)
        {
            var handlers = ChartDataUpdated;
            if (handlers == null) return;
            foreach (EventHandler<ChartDataUpdateEventArgs> h in handlers.GetInvocationList())
            {
                try { h(this, e); }
                catch (Exception ex)
                {
                    // Log ex.ToString() including stack trace and continue
                }
            }
        }

        protected virtual void OnWriteValueRequested(WriteRequestedEventArgs e)
        {
            WriteValueRequested?.Invoke(this, e);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            datagridUpdate();
            updateStartAddressDisplay();
        }

        private void startRegister_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingValues) return;
            _isUpdatingValues = true;

            try
            {
                startRegisterHex.Value = startRegister.Value;
                updateStartAddressDisplay();
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
                updateStartAddressDisplay();
                datagridUpdate();
            }
            finally
            {
                _isUpdatingValues = false;
            }
        }

        private void updateStartAddressDisplay()
        {
            decimal requestedFuncionNo = 0;
            switch (comboBox1.SelectedIndex)
            {
                case 0: requestedFuncionNo = 0; break;
                case 1: requestedFuncionNo = 1; break;
                case 2: requestedFuncionNo = 4; break;
                case 3: requestedFuncionNo = 3; break;
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
            dataGridView1.SuspendLayout();
            int newRowCount = (int)numOfRegisters.Value;

            var formats = new Dictionary<int, DisplayFormat>();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells["RegisterNumber"].Value != null)
                {
                    int regNum = 0;
                    string regVal = dataGridView1.Rows[i].Cells["RegisterNumber"].Value.ToString();
                    int.TryParse(regVal.Split(' ')[0], out regNum);

                    if (regNum != 0 && dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value != null)
                    {
                        formats[regNum] = (DisplayFormat)dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value;
                    }
                }
            }

            dataGridView1.RowCount = newRowCount;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].IsNewRow) continue;

                int currentRegNum = i + (int)startRegister.Value;
                dataGridView1.Rows[i].Cells["RegisterNumber"].Value = currentRegNum;
                dataGridView1.Rows[i].Cells["Name"].Value = "Register_" + currentRegNum;
                dataGridView1.Rows[i].Cells["Description"].Value = "";
                dataGridView1.Rows[i].Visible = true;
                if (formats.ContainsKey(currentRegNum))
                {
                    dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value = formats[currentRegNum];
                }
                else
                {
                    dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value = DisplayFormat.Unsigned16;
                }
            }

            dataGridView1.ResumeLayout();
            RefreshDisplayValues();
        }

        private void ApplyFormatToSelected(DisplayFormat format)
        {
            if (dataGridView1.SelectedCells.Count == 0) return;

            int regsNeeded = GetRegistersForFormat(format);
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                if (cell.ColumnIndex == Value.Index)
                {
                    if (cell.RowIndex + regsNeeded > dataGridView1.Rows.Count)
                    {
                        MessageBox.Show($"Cannot apply format requiring {regsNeeded} registers at row {cell.RowIndex}. Not enough subsequent registers.", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                if (cell.ColumnIndex == Value.Index)
                {
                    dataGridView1.Rows[cell.RowIndex].Cells["DisplayFormatColumn"].Value = format;
                    for (int i = 1; i < regsNeeded; i++)
                    {
                        if (cell.RowIndex + i < dataGridView1.Rows.Count)
                        {
                            dataGridView1.Rows[cell.RowIndex + i].Cells["DisplayFormatColumn"].Value = DisplayFormat.Unsigned16;
                        }
                    }
                }
            }
            RefreshDisplayValues();
        }

        private bool UpdateMenuChecks(ToolStripItemCollection items, DisplayFormat currentFormat)
        {
            bool anyChildInThisListIsChecked = false;

            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    bool isThisItemTheOne = false;
                    bool isDescendantChecked = false;

                    if (menuItem.Tag is DisplayFormat itemFormat)
                    {
                        if (itemFormat == currentFormat)
                        {
                            isThisItemTheOne = true;
                            anyChildInThisListIsChecked = true;
                        }
                    }

                    if (menuItem.HasDropDownItems)
                    {
                        isDescendantChecked = UpdateMenuChecks(menuItem.DropDownItems, currentFormat);
                        if (isDescendantChecked) anyChildInThisListIsChecked = true;
                    }

                    menuItem.Checked = (isThisItemTheOne || isDescendantChecked);
                }
            }
            return anyChildInThisListIsChecked;
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            DataGridViewCell firstSelectedCell = null;
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                if (cell.ColumnIndex == Value.Index)
                {
                    firstSelectedCell = cell;
                    break;
                }
            }

            DisplayFormat currentFormat = DisplayFormat.Unsigned16;
            if (firstSelectedCell != null && dataGridView1.Rows[firstSelectedCell.RowIndex].Cells["DisplayFormatColumn"].Value != null)
            {
                currentFormat = (DisplayFormat)dataGridView1.Rows[firstSelectedCell.RowIndex].Cells["DisplayFormatColumn"].Value;
            }

            UpdateMenuChecks(contextMenuStrip1.Items, currentFormat);

            int maxRowIndex = -1;
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                if (cell.ColumnIndex == Value.Index && cell.RowIndex > maxRowIndex)
                {
                    maxRowIndex = cell.RowIndex;
                }
            }

            if (maxRowIndex == -1)
            {
                toolStripMenuItem2.Enabled = false;
                toolStripMenuItem3.Enabled = false;
                toolStripMenuItem4.Enabled = false;
                return;
            }

            int rowsAvailable = dataGridView1.Rows.Count - maxRowIndex;
            toolStripMenuItem2.Enabled = (rowsAvailable >= 1);
            toolStripMenuItem3.Enabled = (rowsAvailable >= 2);
            toolStripMenuItem4.Enabled = (rowsAvailable >= 4);
        }

        private void unsignedToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Unsigned16);
        private void signedToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Signed16);
        private void binaryToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Binary16);
        private void hexToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex16);
        private void aSCIIToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.ASCII);

        private void unsigned32BEToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Unsigned32_BE);
        private void unsigned32LEToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Unsigned32_LE);
        private void signed32BEToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Signed32_BE);
        private void signed32LEToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Signed32_LE);
        private void float32BEToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Float32_BE);
        private void float32LEToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Float32_LE);
        private void unsigned32BEBSToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Unsigned32_BE_BS);
        private void unsigned32LEBSToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Unsigned32_LE_BS);
        private void signed32BEBSToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Signed32_BE_BS);
        private void signed32LEBSToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Signed32_LE_BS);
        private void float32BEBSToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Float32_BE_BS);
        private void float32LEBSToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Float32_LE_BS);

        private void ascii32BEToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.ASCII32_BE);
        private void ascii32LEToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.ASCII32_LE);
        private void ascii32BEBSToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.ASCII32_BE_BS);
        private void ascii32LEBSToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.ASCII32_LE_BS);

        private void hex32BEToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex32_BE);
        private void hex32LEToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex32_LE);
        private void hex32BEBSToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex32_BE_BS);
        private void hex32LEBSToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex32_LE_BS);

        private void startRegister_1_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingValues) return;
            _isUpdatingValues = true;

            try
            {
                string originalString = (startRegister_1.Value - 1).ToString();
                if (originalString.Length == 0) { _isUpdatingValues = false; return; }

                string functionString = originalString.Substring(0, 1);
                string registerString = originalString.Substring(1);
                decimal registerStringDecimal = Convert.ToDecimal(registerString);
                string requestedFunctionPrefix = "";

                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        if (startRegister_1.Value > 65536 || startRegister_1.Value < 1)
                        {
                            MessageBox.Show("Invalid Register Number. Must be 1-65536.");
                            _isUpdatingValues = false;
                            return;
                        }
                        registerStringDecimal = startRegister_1.Value - 1;
                        break;
                    case 1: requestedFunctionPrefix = "1"; break;
                    case 2: requestedFunctionPrefix = "4"; break;
                    case 3: requestedFunctionPrefix = "3"; break;
                }

                if (functionString != requestedFunctionPrefix && comboBox1.SelectedIndex != 0 || (registerStringDecimal < 0 || registerStringDecimal > 65535))
                {
                    MessageBox.Show("Invalid register range for selected function code. Correcting value.");
                }

                startRegister.Value = registerStringDecimal;
                startRegisterHex.Value = registerStringDecimal;
                datagridUpdate();
            }
            finally
            {
                _isUpdatingValues = false;
            }
        }

        #region Public Interface
        public int GetFunctionCode() => comboBox1.SelectedIndex;
        public int GetStartAddress() => (int)startRegister.Value;
        public int GetQuantity() => (int)numOfRegisters.Value;
        public DataGridViewRowCollection GetDataGridViewRows() => dataGridView1.Rows;

        public void SetConfiguration(int funcCode, int startAddr, int quantity)
        {
            _isUpdatingValues = true;
            try
            {
                comboBox1.SelectedIndex = funcCode;
                startRegister.Value = startAddr;
                numOfRegisters.Value = quantity;
                datagridUpdate();
            }
            finally { _isUpdatingValues = false; }
        }

        public void SetRegisterDefinitions(List<Tuple<int, string, string, string>> registers)
        {
            foreach (var regDef in registers)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow || row.Cells["RegisterNumber"].Value == null) continue;

                    if (int.TryParse(row.Cells["RegisterNumber"].Value?.ToString(), out int regNumInCell))
                    {
                        if (regNumInCell == regDef.Item1)
                        {
                            row.Cells["Name"].Value = regDef.Item2;
                            row.Cells["Description"].Value = regDef.Item3;


                            if (Enum.TryParse<DisplayFormat>(regDef.Item4, out var displayFormat))
                            {
                                row.Cells["DisplayFormatColumn"].Value = displayFormat;
                            }
                            else row.Cells["DisplayFormatColumn"].Value = DisplayFormat.Unsigned16;
                            break;
                        }
                    }
                }
            }

            RefreshDisplayValues();
        }

        public void ClearDisplayValues()
        {
            if (InvokeRequired) { Invoke(new Action(ClearDisplayValues)); return; }
            _rawData = null;
            RefreshDisplayValues();
        }

        public void ShowTabError(string message)
        {
            if (InvokeRequired) { Invoke(new Action(() => ShowTabError(message))); return; }
            lblTabError.Text = message;
            lblTabError.Visible = true;
        }

        public void ClearTabError()
        {
            if (InvokeRequired) { Invoke(new Action(ClearTabError)); return; }
            lblTabError.Visible = false;
        }

        public void UpdateValues(ushort[] data)
        {
            if (InvokeRequired) { Invoke(new Action(() => UpdateValues(data))); return; }
            if (data == null) return;
            _rawData = data;
            RefreshDisplayValues();
        }

        public List<ChartDataPoint> GetChartData()
        {
            if (_rawData == null) return new List<ChartDataPoint>();

            var chartData = new List<ChartDataPoint>();
            DateTime now = DateTime.Now;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].IsNewRow || !dataGridView1.Rows[i].Visible) continue;

                bool isChartChecked = (bool)(dataGridView1.Rows[i].Cells["Chart"].Value ?? false);
                DisplayFormat format = (DisplayFormat)dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value;
                int regsNeeded = GetRegistersForFormat(format);

                if (isChartChecked && IsNumericFormat(format))
                {
                    string stringValue = FormatValue(i);
                    if (double.TryParse(stringValue.Replace("0x", "").Replace(" ", ""), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value))
                    {
                        string seriesName = dataGridView1.Rows[i].Cells["Name"].Value?.ToString() ?? "Register";
                        string fullSeriesName = this.Parent?.Text + " - " + seriesName;

                        chartData.Add(new ChartDataPoint { SeriesName = fullSeriesName, Value = value, Timestamp = now });
                    }
                }
                if (regsNeeded > 1) i += (regsNeeded - 1);
            }
            return chartData;
        }
        #endregion

        #region Formatting Logic
        private bool IsNumericFormat(DisplayFormat format)
        {
            string fmtStr = format.ToString();
            if (fmtStr.Contains("Hex") || fmtStr.Contains("Binary") || fmtStr.Contains("ASCII") || format == DisplayFormat.Bool16) return false;
            return true;
        }

        public int GetRegistersForFormat(DisplayFormat format)
        {
            string fmtStr = format.ToString();
            if (fmtStr.Contains("64")) return 4;
            if (fmtStr.Contains("32")) return 2;
            return 1;
        }

        private byte[] BuildByteArray(int rowIndex, int numRegisters, bool isLE_Format, bool isBS_Format)
        {
            byte[] bytes = new byte[numRegisters * 2];
            ushort[] regs = new ushort[numRegisters];
            for (int i = 0; i < numRegisters; i++)
            {
                ushort raw = _rawData[rowIndex + i];
                regs[i] = isBS_Format ? (ushort)((raw << 8) | (raw >> 8)) : raw;
            }

            for (int i = 0; i < numRegisters; i++)
            {
                int regIndex = isLE_Format ? (numRegisters - 1 - i) : i;
                byte[] regBytes = BitConverter.GetBytes(regs[regIndex]);
                bytes[i * 2] = regBytes[1];
                bytes[i * 2 + 1] = regBytes[0];
            }
            return bytes;
        }

        private string FormatValue(int rowIndex)
        {
            if (_rawData == null || rowIndex >= _rawData.Length) return "---";

            DisplayFormat format = (DisplayFormat)dataGridView1.Rows[rowIndex].Cells["DisplayFormatColumn"].Value;
            int regsNeeded = GetRegistersForFormat(format);
            string fmtStr = format.ToString();

            if (rowIndex + regsNeeded > _rawData.Length) return "ERROR";

            try
            {
                if (regsNeeded == 1)
                {
                    ushort val = _rawData[rowIndex];
                    switch (format)
                    {
                        case DisplayFormat.Signed16: return ((short)val).ToString();
                        case DisplayFormat.Hex16: return $"0x{val:X4}";
                        case DisplayFormat.Binary16: return Convert.ToString(val, 2).PadLeft(16, '0');
                        case DisplayFormat.Bool16: return (val != 0) ? "True" : "False";
                        case DisplayFormat.ASCII:
                            byte[] ab = BitConverter.GetBytes(val);
                            return BitConverter.IsLittleEndian ? $"{(char)ab[0]}{(char)ab[1]}" : $"{(char)ab[1]}{(char)ab[0]}";
                        default: return val.ToString();
                    }
                }

                bool isLE = fmtStr.Contains("_LE");
                bool isBS = fmtStr.Contains("_BS");
                bool isAscii = fmtStr.Contains("ASCII");
                bool isHex = fmtStr.Contains("Hex");

                if (regsNeeded == 2 || regsNeeded == 4)
                {
                    byte[] bytes = BuildByteArray(rowIndex, regsNeeded, isLE, isBS);
                    if (!isAscii && !isHex && BitConverter.IsLittleEndian) Array.Reverse(bytes);

                    if (regsNeeded == 2)
                    {
                        switch (format)
                        {
                            case DisplayFormat.Unsigned32_BE:
                            case DisplayFormat.Unsigned32_LE:
                            case DisplayFormat.Unsigned32_BE_BS:
                            case DisplayFormat.Unsigned32_LE_BS: return BitConverter.ToUInt32(bytes, 0).ToString();
                            case DisplayFormat.Signed32_BE:
                            case DisplayFormat.Signed32_LE:
                            case DisplayFormat.Signed32_BE_BS:
                            case DisplayFormat.Signed32_LE_BS: return BitConverter.ToInt32(bytes, 0).ToString();
                            case DisplayFormat.Float32_BE:
                            case DisplayFormat.Float32_LE:
                            case DisplayFormat.Float32_BE_BS:
                            case DisplayFormat.Float32_LE_BS: return BitConverter.ToSingle(bytes, 0).ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                            case DisplayFormat.ASCII32_BE:
                            case DisplayFormat.ASCII32_LE:
                            case DisplayFormat.ASCII32_BE_BS:
                            case DisplayFormat.ASCII32_LE_BS: return Encoding.ASCII.GetString(bytes).Replace("\0", " ");
                            case DisplayFormat.Hex32_BE:
                            case DisplayFormat.Hex32_LE:
                            case DisplayFormat.Hex32_BE_BS:
                            case DisplayFormat.Hex32_LE_BS: return "0x" + BitConverter.ToString(bytes).Replace("-", "");
                        }
                    }
                    else
                    {
                        switch (format)
                        {
                            case DisplayFormat.Unsigned64_BE:
                            case DisplayFormat.Unsigned64_LE:
                            case DisplayFormat.Unsigned64_BE_BS:
                            case DisplayFormat.Unsigned64_LE_BS: return BitConverter.ToUInt64(bytes, 0).ToString();
                            case DisplayFormat.Signed64_BE:
                            case DisplayFormat.Signed64_LE:
                            case DisplayFormat.Signed64_BE_BS:
                            case DisplayFormat.Signed64_LE_BS: return BitConverter.ToInt64(bytes, 0).ToString();
                            case DisplayFormat.Double64_BE:
                            case DisplayFormat.Double64_LE:
                            case DisplayFormat.Float64_BE_BS:
                            case DisplayFormat.Float64_LE_BS: return BitConverter.ToDouble(bytes, 0).ToString("F5", System.Globalization.CultureInfo.InvariantCulture);
                            case DisplayFormat.ASCII64_BE:
                            case DisplayFormat.ASCII64_LE:
                            case DisplayFormat.ASCII64_BE_BS:
                            case DisplayFormat.ASCII64_LE_BS: return Encoding.ASCII.GetString(bytes).Replace("\0", " ");
                            case DisplayFormat.Hex64_BE:
                            case DisplayFormat.Hex64_LE:
                            case DisplayFormat.Hex64_BE_BS:
                            case DisplayFormat.Hex64_LE_BS: return "0x" + BitConverter.ToString(bytes).Replace("-", "");
                        }
                    }
                }
            }
            catch { return "ERR"; }
            return "---";
        }

        public void RefreshDisplayValues()
        {
            if (InvokeRequired) { Invoke(new Action(RefreshDisplayValues)); return; }
            dataGridView1.SuspendLayout();

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].IsNewRow) continue;
                dataGridView1.Rows[i].Visible = true;

                if (dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value == null) dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value = DisplayFormat.Unsigned16;
                DisplayFormat format = (DisplayFormat)dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value;
                int regsNeeded = GetRegistersForFormat(format);

                bool isNumeric = IsNumericFormat(format);
                DataGridViewCheckBoxCell chartCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["Chart"];
                chartCell.ReadOnly = !isNumeric;
                if (!isNumeric) chartCell.Value = false;
                chartCell.Style.BackColor = isNumeric ? dataGridView1.DefaultCellStyle.BackColor : SystemColors.ControlLight;

                bool isCoilOrHolding = (GetFunctionCode() == 0 || GetFunctionCode() == 2);
                dataGridView1.Rows[i].Cells["Value"].ReadOnly = !(isCoilOrHolding && (i + regsNeeded <= dataGridView1.Rows.Count));

                int baseRegNum = i + (int)startRegister.Value;
                dataGridView1.Rows[i].Cells["RegisterNumber"].Value = regsNeeded == 1 ? baseRegNum.ToString() : $"{baseRegNum} - {baseRegNum + regsNeeded - 1}";
                dataGridView1.Rows[i].Cells["Value"].Value = FormatValue(i);

                bool valueChanged = false;
                // safe comparison
                if (_rawData != null && _prevRawData != null)
                {
                    for (int o = 0; o < regsNeeded; o++)
                    {
                        int idx = i + o;
                        if (idx < _rawData.Length && idx < _prevRawData.Length && _rawData[idx] != _prevRawData[idx])
                        {
                            valueChanged = true;
                            break;
                        }
                    }
                }

                if (valueChanged) _lastChangeTimes[i] = DateTime.Now;

                if (regsNeeded > 1)
                {
                    for (int j = 1; j < regsNeeded; j++)
                    {
                        if (i + j < dataGridView1.Rows.Count)
                        {
                            dataGridView1.Rows[i + j].Visible = false;
                            dataGridView1.Rows[i + j].Cells["Value"].ReadOnly = true;
                            ((DataGridViewCheckBoxCell)dataGridView1.Rows[i + j].Cells["Chart"]).ReadOnly = true;
                        }
                    }
                    i += (regsNeeded - 1);
                }
            }
            dataGridView1.ResumeLayout();
            if (_rawData != null) _prevRawData = (ushort[])_rawData.Clone();
            OnChartDataUpdated(new ChartDataUpdateEventArgs(GetChartData()));
        }
        #endregion

        // Click handlers for 64-bit formats
        private void bigendianToolStripMenuItem6_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Unsigned64_BE);
        private void littleendianToolStripMenuItem6_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Unsigned64_LE);
        private void bigendianByteSwapToolStripMenuItem4_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Unsigned64_BE_BS);
        private void littleendianByteSwapToolStripMenuItem4_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Unsigned64_LE_BS);
        private void bigendianToolStripMenuItem7_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Signed64_BE);
        private void littleendianToolStripMenuItem7_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Signed64_LE);
        private void bigendianByteSwapToolStripMenuItem5_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Signed64_BE_BS);
        private void littleendianByteSwapToolStripMenuItem5_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Signed64_LE_BS);
        private void bigendianToolStripMenuItem8_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Double64_BE);
        private void littleendianToolStripMenuItem8_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Double64_LE);
        private void bigendianByteSwapToolStripMenuItem6_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Float64_BE_BS);
        private void littleendianByteSwapToolStripMenuItem6_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Float64_LE_BS);
        private void bigendianToolStripMenuItem10_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.ASCII64_BE);
        private void littleendianToolStripMenuItem10_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.ASCII64_LE);
        private void bigendianByteSwapToolStripMenuItem8_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.ASCII64_BE_BS);
        private void littleendianByteSwapToolStripMenuItem8_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.ASCII64_LE_BS);
        private void hex64BEToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex64_BE);
        private void hex64LEToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex64_LE);
        private void hex64BEBSToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex64_BE_BS);
        private void hex64LEBSToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex64_LE_BS);
        private void bigendianToolStripMenuItem9_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex64_BE);
        private void littleendianToolStripMenuItem9_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex64_LE);
        private void bigendianByteSwapToolStripMenuItem7_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex64_BE_BS);
        private void littleendianByteSwapToolStripMenuItem7_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Hex64_LE_BS);
        private void boolToolStripMenuItem_Click(object sender, EventArgs e) => ApplyFormatToSelected(DisplayFormat.Bool16);

        private void _fadeTimer_Tick(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0) return;
            DateTime now = DateTime.Now;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow || !row.Visible) continue;
                if (_lastChangeTimes.TryGetValue(row.Index, out DateTime lastChange))
                {
                    double elapsed = (now - lastChange).TotalMilliseconds;
                    if (elapsed < FADE_DURATION_MS)
                    {
                        row.Cells["Value"].Style.BackColor = InterpolateColor(_flashColor, dataGridView1.DefaultCellStyle.BackColor, (float)(elapsed / FADE_DURATION_MS));
                    }
                    else if (row.Cells["Value"].Style.BackColor != dataGridView1.DefaultCellStyle.BackColor)
                    {
                        row.Cells["Value"].Style.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                    }
                }
            }
        }

        private Color InterpolateColor(Color start, Color end, float percentage)
        {
            percentage = Math.Max(0, Math.Min(1, percentage));
            return Color.FromArgb(
                (int)(start.R + (end.R - start.R) * percentage),
                (int)(start.G + (end.G - start.G) * percentage),
                (int)(start.B + (end.B - start.B) * percentage));
        }
    }

    public class ChartDataPoint
    {
        public string SeriesName { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ChartDataUpdateEventArgs : EventArgs
    {
        public List<ChartDataPoint> DataPoints { get; }
        public ChartDataUpdateEventArgs(List<ChartDataPoint> dataPoints) { DataPoints = dataPoints; }
    }

    public class WriteRequestedEventArgs : EventArgs
    {
        public byte SlaveId { get; set; }
        public int FunctionCode { get; set; }
        public ushort StartAddress { get; set; }
        public string ValueString { get; set; }
        public ReadingsTab ReadingsTab { get; set; }
        public ReadingsTab.DisplayFormat Format { get; set; }
        public int RowIndex { get; set; }

        public WriteRequestedEventArgs(int fc, ushort addr, string val, ReadingsTab tab, ReadingsTab.DisplayFormat fmt, int idx)
        {
            FunctionCode = fc; StartAddress = addr; ValueString = val; ReadingsTab = tab; Format = fmt; RowIndex = idx;
        }
    }
}