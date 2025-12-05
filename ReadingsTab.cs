using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace MP_ModbusApp
{
    

    public partial class ReadingsTab : UserControl
    {
        public event EventHandler<ChartDataUpdateEventArgs> ChartDataUpdated;

        // NOWY EVENT: Zdarzenie wywoływane po edycji komórki
        public event EventHandler<WriteRequestedEventArgs> WriteValueRequested;

        // Flag to prevent recursive event loops when updating numeric up/down controls
        private bool _isUpdatingValues = false;

        // Local cache of the most recent raw data from the poller
        private ushort[] _rawData = null;

        /// <summary>
        /// Defines all possible display formats for register values.
        /// </summary>
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
            Float32_BE, // Real
            Unsigned32_LE,
            Signed32_LE,
            Float32_LE,
            Unsigned32_BE_BS,
            Signed32_BE_BS,
            Float32_BE_BS, // Real
            Unsigned32_LE_BS,
            Signed32_LE_BS,
            Float32_LE_BS, // alias for Float
            ASCII32_BE,
            ASCII32_LE,
            ASCII32_BE_BS,
            ASCII32_LE_BS,
            // New 32-bit Hex formats
            Hex32_BE,
            Hex32_LE,
            Hex32_BE_BS,
            Hex32_LE_BS,


            // 64-bit (analogous)
            Unsigned64_BE,
            Signed64_BE,
            Double64_BE, // Real (64-bit)
            Unsigned64_LE,
            Signed64_LE,
            Double64_LE,
            Unsigned64_BE_BS,
            Signed64_BE_BS,
            Float64_BE_BS, // Real (alias for Double)
            Unsigned64_LE_BS,
            Signed64_LE_BS,
            Float64_LE_BS, // alias for Double
            ASCII64_BE,
            ASCII64_LE,
            ASCII64_BE_BS,
            ASCII64_LE_BS,
            // New 64-bit Hex formats
            Hex64_BE,
            Hex64_LE,
            Hex64_BE_BS,
            Hex64_LE_BS
        }

        public ReadingsTab()
        {
            InitializeComponent();

            // --- Link context menu items to DisplayFormat enums using the Tag property ---
            this.unsignedToolStripMenuItem.Tag = DisplayFormat.Unsigned16;
            this.signedToolStripMenuItem.Tag = DisplayFormat.Signed16;
            this.binaryToolStripMenuItem.Tag = DisplayFormat.Binary16;
            this.hexToolStripMenuItem.Tag = DisplayFormat.Hex16;
            this.boolToolStripMenuItem.Tag = DisplayFormat.Bool16;
            this.aSCIIToolStripMenuItem.Tag = DisplayFormat.ASCII;

            // 32-bit Unsigned
            this.bigendianToolStripMenuItem.Tag = DisplayFormat.Unsigned32_BE;
            this.littleendianToolStripMenuItem.Tag = DisplayFormat.Unsigned32_LE;
            // 32-bit Signed
            this.bigendianToolStripMenuItem2.Tag = DisplayFormat.Signed32_BE;
            this.littleendianToolStripMenuItem3.Tag = DisplayFormat.Signed32_LE;
            // 32-bit Real
            this.bigendianToolStripMenuItem3.Tag = DisplayFormat.Float32_BE;
            this.littleendianToolStripMenuItem2.Tag = DisplayFormat.Float32_LE;

            // 64-bit Unsigned
            this.bigendianToolStripMenuItem6.Tag = DisplayFormat.Unsigned64_BE;
            this.littleendianToolStripMenuItem6.Tag = DisplayFormat.Unsigned64_LE;
            this.bigendianByteSwapToolStripMenuItem4.Tag = DisplayFormat.Unsigned64_BE_BS;
            this.littleendianByteSwapToolStripMenuItem4.Tag = DisplayFormat.Unsigned64_LE_BS;
            // 64-bit Signed
            this.bigendianToolStripMenuItem7.Tag = DisplayFormat.Signed64_BE;
            this.littleendianToolStripMenuItem7.Tag = DisplayFormat.Signed64_LE;
            this.bigendianByteSwapToolStripMenuItem5.Tag = DisplayFormat.Signed64_BE_BS;
            this.littleendianByteSwapToolStripMenuItem5.Tag = DisplayFormat.Signed64_LE_BS;
            // 64-bit Real (Double)
            this.bigendianToolStripMenuItem8.Tag = DisplayFormat.Double64_BE;
            this.littleendianToolStripMenuItem8.Tag = DisplayFormat.Double64_LE;
            this.bigendianByteSwapToolStripMenuItem6.Tag = DisplayFormat.Float64_BE_BS;
            this.littleendianByteSwapToolStripMenuItem6.Tag = DisplayFormat.Float64_LE_BS;
            // 64-bit ASCII
            this.bigendianToolStripMenuItem10.Tag = DisplayFormat.ASCII64_BE;
            this.littleendianToolStripMenuItem10.Tag = DisplayFormat.ASCII64_LE;
            this.bigendianByteSwapToolStripMenuItem8.Tag = DisplayFormat.ASCII64_BE_BS;
            this.littleendianByteSwapToolStripMenuItem8.Tag = DisplayFormat.ASCII64_LE_BS;


            // --- Connect all event handlers from the designer ---
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

            // --- NOWE: Podpięcie zdarzeń do obsługi wykresu i ZAPISU ---
            this.dataGridView1.CellValueChanged += new DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            this.dataGridView1.CurrentCellDirtyStateChanged += new EventHandler(this.dataGridView1_CurrentCellDirtyStateChanged);
            this.dataGridView1.CellEndEdit += new DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
        }

        private void ReadingsTab_Load(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
            {
                // Default to Holding Registers (4x)
                comboBox1.SelectedIndex = 2;
            }
            dataGridView1.RowCount = (int)numOfRegisters.Value;
            lblTabError.Visible = false;
        }

        // Wymusza natychmiastowe zatwierdzenie edycji komórki (np. po kliknięciu checkboxa)
        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty && dataGridView1.CurrentCell.ColumnIndex == Chart.Index)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        // Wywołuje zdarzenie po zmianie wartości komórki (np. checkboxa)
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Wywołaj aktualizację wykresu tylko po zmianie wartości w kolumnie "Chart"
            if (e.RowIndex >= 0 && e.ColumnIndex == Chart.Index)
            {
                OnChartDataUpdated(new ChartDataUpdateEventArgs(GetChartData()));
            }
        }

        /// <summary>
        /// NOWA METODA: Obsługuje zakończenie edycji komórki 'Value' i wywołuje zdarzenie zapisu.
        /// </summary>
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Sprawdź, czy edytowana komórka znajduje się w kolumnie "Value"
            if (e.RowIndex < 0 || e.ColumnIndex != Value.Index) return;

            int funcCode = GetFunctionCode();

            // Zapis jest dozwolony tylko dla Coils (FC 01, index 0) i Holding Registers (FC 03, index 2)
            if (funcCode != 0 && funcCode != 2)
            {
                MessageBox.Show("Zapis jest dozwolony tylko dla Coils (0x) i Holding Registers (4x).", "Błąd zapisu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Cofnij edycję (prosta metoda: odśwież DataGridView)
                RefreshDisplayValues();
                return;
            }

            // Adres rejestru/Coila, który jest wyświetlany w tym wierszu
            int startAddressForTab = GetStartAddress();
            ushort transactionAddress = (ushort)(startAddressForTab + e.RowIndex);

            string valueString = dataGridView1.Rows[e.RowIndex].Cells[Value.Index].Value?.ToString() ?? "";
            DisplayFormat format = (DisplayFormat)dataGridView1.Rows[e.RowIndex].Cells["DisplayFormatColumn"].Value;
            int regsNeeded = GetRegistersForFormat(format); // Sprawdź, ile rejestrów wymaga format

            if (string.IsNullOrWhiteSpace(valueString)) return;

            // Logika walidacji edycji dla pól wielorejestrowych
            if (regsNeeded > 1 && dataGridView1.Rows[e.RowIndex].Cells["Value"].ReadOnly)
            {
                // To się nie powinno zdarzyć, jeśli flaga ReadOnly została poprawnie ustawiona
                // Jeśli jednak nastąpiła edycja wiersza, który nie jest początkiem grupy
                MessageBox.Show("Edycja jest dozwolona tylko w pierwszym wierszu grupy wielorejestrowej (np. 40001).", "Błąd edycji", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RefreshDisplayValues();
                return;
            }

            // Cofnij wyświetlaną wartość do "---" do czasu następnego odczytu
            dataGridView1.Rows[e.RowIndex].Cells[Value.Index].Value = "---";

            // Wywołaj zdarzenie. SlaveId zostanie dodane w ModbusDevice.
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
            ChartDataUpdated?.Invoke(this, e);
        }

        protected virtual void OnWriteValueRequested(WriteRequestedEventArgs e)
        {
            WriteValueRequested?.Invoke(this, e);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            datagridUpdate();
        }

        /// <summary>
        /// Handles changes to the decimal start register, updating the Hex and 40001-style boxes.
        /// </summary>
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

        /// <summary>
        /// Handles changes to the hex start register, updating the Decimal and 40001-style boxes.
        /// </summary>
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

        /// <summary>
        /// Updates the 40001-style register display based on the function code and start address.
        /// </summary>
        private void updateStartAddressDisplay()
        {
            decimal requestedFuncionNo = 0;
            switch (comboBox1.SelectedIndex)
            {
                case 0: // 01 Coils (0x)
                    requestedFuncionNo = 0;
                    break;
                case 1: // 02 Discrete Inputs (1x)
                    requestedFuncionNo = 1;
                    break;
                case 2: // 03 Holding Registers (4x)
                    requestedFuncionNo = 4;
                    break;
                case 3: // 04 Input Registers (3x)
                    requestedFuncionNo = 3;
                    break;
                default:
                    break;
            }

            // Use 5 or 6 digits based on register number
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

        /// <summary>
        /// Re-initializes the DataGridView when settings (like quantity or start address) change.
        /// It preserves existing register names and display formats.
        /// </summary>
        private void datagridUpdate()
        {
            dataGridView1.SuspendLayout();

            int newRowCount = (int)numOfRegisters.Value;

            // Store existing formats by register number before resizing
            var formats = new Dictionary<int, DisplayFormat>();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells["RegisterNumber"].Value != null)
                {
                    int regNum = 0;
                    string regVal = dataGridView1.Rows[i].Cells["RegisterNumber"].Value.ToString();
                    // Get the first number from "100" or "100 - 103"
                    int.TryParse(regVal.Split(' ')[0], out regNum);

                    if (regNum != 0 && dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value != null)
                    {
                        formats[regNum] = (DisplayFormat)dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value;
                    }
                }
            }

            dataGridView1.RowCount = newRowCount; // Apply the new row count

            // Initialize all rows (new and old)
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].IsNewRow) continue;

                int currentRegNum = i + (int)startRegister.Value;
                dataGridView1.Rows[i].Cells["RegisterNumber"].Value = currentRegNum;
                dataGridView1.Rows[i].Cells["Name"].Value = "Register_" + currentRegNum;
                dataGridView1.Rows[i].Visible = true; // Always reset visibility

                // Restore the old format if it existed, otherwise set default
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

            // Finally, apply all formatting logic (hiding rows, etc.)
            RefreshDisplayValues();
        }

        /// <summary>
        /// Applies the selected DisplayFormat to all selected "Value" cells.
        /// </summary>
        private void ApplyFormatToSelected(DisplayFormat format)
        {
            if (dataGridView1.SelectedCells.Count == 0) return;

            // Check if the format can be applied to all selected cells without going out of bounds
            int regsNeeded = GetRegistersForFormat(format);
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                // Only apply to cells in the "Value" column
                if (cell.ColumnIndex == Value.Index)
                {
                    // Check if the format would read past the end of the grid
                    if (cell.RowIndex + regsNeeded > dataGridView1.Rows.Count)
                    {
                        MessageBox.Show($"Cannot apply format requiring {regsNeeded} registers at row {cell.RowIndex}. Not enough subsequent registers.", "Formatting Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return; // Abort operation
                    }
                }
            }

            // Apply the format
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                if (cell.ColumnIndex == Value.Index)
                {
                    // Set the format for the *first* row
                    dataGridView1.Rows[cell.RowIndex].Cells["DisplayFormatColumn"].Value = format;

                    // Reset the formats for any "hidden" rows that will now be part of this one
                    for (int i = 1; i < regsNeeded; i++)
                    {
                        if (cell.RowIndex + i < dataGridView1.Rows.Count)
                        {
                            dataGridView1.Rows[cell.RowIndex + i].Cells["DisplayFormatColumn"].Value = DisplayFormat.Unsigned16; // Reset
                        }
                    }
                }
            }

            // Refresh the entire grid to show changes (hide rows, update values)
            RefreshDisplayValues();
        }

        /// <summary>
        /// Recursively searches menu items to set the 'Checked' state on the
        /// currently active format and any of its parent menus.
        /// </summary>
        /// <returns>Returns 'true' if this item or any of its children are checked.</returns>
        private bool UpdateMenuChecks(ToolStripItemCollection items, DisplayFormat currentFormat)
        {
            bool anyChildInThisListIsChecked = false;

            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    bool isThisItemTheOne = false;
                    bool isDescendantChecked = false;

                    // 1. Check if this item is the final selection (has a Tag)
                    if (menuItem.Tag is DisplayFormat itemFormat)
                    {
                        if (itemFormat == currentFormat)
                        {
                            isThisItemTheOne = true;
                            anyChildInThisListIsChecked = true; // Signal to parent
                        }
                    }

                    // 2. Recurse if the item has a sub-menu
                    if (menuItem.HasDropDownItems)
                    {
                        // If a descendant is checked, this parent must also be
                        isDescendantChecked = UpdateMenuChecks(menuItem.DropDownItems, currentFormat);
                        if (isDescendantChecked)
                        {
                            anyChildInThisListIsChecked = true; // Signal to parent
                        }
                    }

                    // 3. Set the check state
                    // The item is checked if:
                    // A) It is the exact item the user selected (isThisItemTheOne)
                    // OR
                    // B) It is a parent of the item the user selected (isDescendantChecked)
                    menuItem.Checked = (isThisItemTheOne || isDescendantChecked);
                }
            }

            // Return status to the parent call
            return anyChildInThisListIsChecked;
        }


        /// <summary>
        /// Handles the opening of the context menu.
        /// Sets the checked state and enables/disables items based on selection.
        /// </summary>
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0)
            {
                e.Cancel = true; // Don't show the menu if no cells are selected
                return;
            }

            // --- Set Check Marks ---
            DataGridViewCell firstSelectedCell = null;
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                if (cell.ColumnIndex == Value.Index)
                {
                    firstSelectedCell = cell;
                    break;
                }
            }

            DisplayFormat currentFormat = DisplayFormat.Unsigned16; // Default

            if (firstSelectedCell != null &&
                dataGridView1.Rows[firstSelectedCell.RowIndex].Cells["DisplayFormatColumn"].Value != null)
            {
                currentFormat = (DisplayFormat)dataGridView1.Rows[firstSelectedCell.RowIndex].Cells["DisplayFormatColumn"].Value;
            }

            // Update the check status for all items in the menu
            UpdateMenuChecks(contextMenuStrip1.Items, currentFormat);

            // --- Enable/Disable Items ---
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
                // No "Value" cell is selected, disable all
                toolStripMenuItem2.Enabled = false; // 16-bit
                toolStripMenuItem3.Enabled = false; // 32-bit
                toolStripMenuItem4.Enabled = false; // 64-bit
                return;
            }

            // Enable/disable based on the number of rows available *after* the last selected cell
            int rowsAvailable = dataGridView1.Rows.Count - maxRowIndex;

            toolStripMenuItem2.Enabled = (rowsAvailable >= 1); // 16-bit
            toolStripMenuItem3.Enabled = (rowsAvailable >= 2); // 32-bit
            toolStripMenuItem4.Enabled = (rowsAvailable >= 4); // 64-bit
        }


        // --- 16-bit Format Click Handlers ---
        private void unsignedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Unsigned16);
        }

        private void signedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Signed16);
        }

        private void binaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Binary16);
        }

        private void hexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex16);
        }

        private void aSCIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.ASCII);
        }


        // --- 32-bit Format Click Handlers ---
        private void unsigned32BEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Unsigned32_BE);
        }

        private void unsigned32LEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Unsigned32_LE);
        }

        private void signed32BEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Signed32_BE);
        }

        private void signed32LEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Signed32_LE);
        }

        private void float32BEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Float32_BE);
        }

        private void float32LEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Float32_LE);
        }

        // 32-bit Byte Swap
        private void unsigned32BEBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Unsigned32_BE_BS);
        }

        private void unsigned32LEBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Unsigned32_LE_BS);
        }

        private void signed32BEBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Signed32_BE_BS);
        }

        private void signed32LEBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Signed32_LE_BS);
        }

        private void float32BEBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Float32_BE_BS);
        }

        private void float32LEBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Float32_LE_BS);
        }

        // 32-bit ASCII
        private void ascii32BEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.ASCII32_BE);
        }

        private void ascii32LEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.ASCII32_LE);
        }

        private void ascii32BEBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.ASCII32_BE_BS);
        }

        private void ascii32LEBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.ASCII32_LE_BS);
        }

        // 32-bit Hex
        private void hex32BEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex32_BE);
        }

        private void hex32LEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex32_LE);
        }

        private void hex32BEBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex32_BE_BS);
        }

        private void hex32LEBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex32_LE_BS);
        }


        /// <summary>
        /// Handles changes to the 40001-style register box, updating the decimal/hex boxes.
        /// </summary>
        private void startRegister_1_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingValues) return;
            _isUpdatingValues = true;

            try
            {
                string originalString = (startRegister_1.Value - 1).ToString();
                if (originalString.Length == 0)
                {
                    _isUpdatingValues = false;
                    return;
                }

                string functionString = originalString.Substring(0, 1);
                string registerString = originalString.Substring(1);
                decimal registerStringDecimal = Convert.ToDecimal(registerString);
                string requestedFunctionPrefix = "";

                switch (comboBox1.SelectedIndex)
                {
                    case 0: // 01 Coils (0x)
                        if (startRegister_1.Value > 65536 || startRegister_1.Value < 1)
                        {
                            MessageBox.Show("Invalid Register Number. Register Number must be 1-65536");
                            _isUpdatingValues = false;
                            return;
                        }
                        else
                        {
                            // For coils, the address is just the value
                            registerStringDecimal = startRegister_1.Value - 1;
                        }
                        break;

                    case 1: // 02 Discrete Inputs (1x)
                        requestedFunctionPrefix = "1";
                        break;
                    case 2: // 03 Holding Registers (4x)
                        requestedFunctionPrefix = "4";
                        break;
                    case 3: // 04 Input Registers (3x)
                        requestedFunctionPrefix = "3";
                        break;
                    default:
                        break;
                }

                // Validate the prefix for non-coil types
                if (functionString != requestedFunctionPrefix && comboBox1.SelectedIndex != 0 || (registerStringDecimal < 0 || registerStringDecimal > 65535))
                {
                    MessageBox.Show("Invalid Register Number for the selected Function Code. Adjusting to match Function Code.");
                    // Don't return, just let it correct the value
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

        #region Public Interface (for ModbusDevice form)

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

        /// <summary>
        /// Sets the tab's configuration when loading a device from the database.
        /// </summary>
        public void SetConfiguration(int funcCode, int startAddr, int quantity)
        {
            _isUpdatingValues = true;
            try
            {
                comboBox1.SelectedIndex = funcCode;
                startRegister.Value = startAddr;
                numOfRegisters.Value = quantity;
                datagridUpdate(); // This will also update the hex/40001 boxes
            }
            finally
            {
                _isUpdatingValues = false;
            }
        }

        /// <summary>
        /// Populates the register names and display formats from the database.
        /// </summary>
        public void SetRegisterDefinitions(List<Tuple<int, string, string>> registers)
        {
            foreach (var regDef in registers) // (int RegNum, string RegName, string RegFormat)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow || row.Cells["RegisterNumber"].Value == null) continue;

                    // The "RegisterNumber" cell value might be an int (e.g., 100) or a string (e.g., "100 - 103")
                    if (row.Cells["RegisterNumber"].Value is int regNumInCell)
                    {
                        if (regNumInCell == regDef.Item1) // Item1 = RegisterNumber
                        {
                            row.Cells["Name"].Value = regDef.Item2; // Item2 = RegisterName

                            // Set the display format
                            if (Enum.TryParse<DisplayFormat>(regDef.Item3, out var displayFormat)) // Item3 = DisplayFormat (string)
                            {
                                row.Cells["DisplayFormatColumn"].Value = displayFormat;
                            }
                            else
                            {
                                row.Cells["DisplayFormatColumn"].Value = DisplayFormat.Unsigned16; // Default
                            }
                            break; // Found matching row, move to next register
                        }
                    }
                }
            }

            // After setting ALL formats, call RefreshDisplayValues() ONCE
            // to consolidate multi-row formats and hide rows correctly.
            RefreshDisplayValues();
        }

        /// <summary>
        /// Clears all displayed values in the grid, showing "---".
        /// This is used when a non-communication error (like Illegal Data Address)
        /// occurs, to prevent showing stale data. Thread-safe.
        /// </summary>
        public void ClearDisplayValues()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearDisplayValues));
                return;
            }

            _rawData = null;
            RefreshDisplayValues();
        }


        /// <summary>
        /// Displays an error message on this specific tab. Thread-safe.
        /// </summary>
        public void ShowTabError(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowTabError(message)));
                return;
            }
            lblTabError.Text = message;
            lblTabError.Visible = true;
        }

        /// <summary>
        /// Clears the error message on this tab. Thread-safe.
        /// </summary>
        public void ClearTabError()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearTabError));
                return;
            }
            lblTabError.Visible = false;
        }

        /// <summary>
        /// Main entry point for new data from the polling loop. Thread-safe.
        /// </summary>
        public void UpdateValues(ushort[] data)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateValues(data)));
                return;
            }

            if (data == null) return;

            _rawData = data; // Store the raw data

            // Refresh the grid display using the new data
            RefreshDisplayValues();

        }

        /// <summary>
        /// Zwraca listę aktywnych punktów danych do wykreślenia.
        /// Wymaga publicznego dostępu dla ModbusDevice.
        /// </summary>
        public List<ChartDataPoint> GetChartData()
        {
            if (_rawData == null) return new List<ChartDataPoint>();

            var chartData = new List<ChartDataPoint>();
            DateTime now = DateTime.Now;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].IsNewRow || !dataGridView1.Rows[i].Visible) continue;

                // Użyj domyślnej wartości false, jeśli komórka jest DBNull
                bool isChartChecked = (bool)(dataGridView1.Rows[i].Cells["Chart"].Value ?? false);
                DisplayFormat format = (DisplayFormat)dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value;
                int regsNeeded = GetRegistersForFormat(format);

                if (isChartChecked && IsNumericFormat(format))
                {
                    string stringValue = FormatValue(i);

                    // Bezpieczne parsowanie wartości. Używamy InvariantCulture, aby uniknąć problemów z przecinkami/kropkami.
                    if (double.TryParse(stringValue.Replace("0x", "").Replace(" ", ""),
                                       System.Globalization.NumberStyles.Any,
                                       System.Globalization.CultureInfo.InvariantCulture,
                                       out double value))
                    {
                        string seriesName = dataGridView1.Rows[i].Cells["Name"].Value?.ToString() ?? "Register";

                        // Dodanie nazwy nadrzędnej zakładki/grupy do nazwy serii
                        string fullSeriesName = this.Parent?.Text + " - " + seriesName;

                        chartData.Add(new ChartDataPoint
                        {
                            SeriesName = fullSeriesName,
                            Value = value,
                            Timestamp = now
                        });
                    }
                }

                // Pomiń ukryte wiersze (część większej wartości)
                if (regsNeeded > 1)
                {
                    i += (regsNeeded - 1);
                }
            }

            return chartData;
        }

        #endregion

        #region Formatting Logic

        /// <summary>
        /// Checks if a DisplayFormat represents a numeric type that can be charted.
        /// Non-numeric types are Hex, Binary, ASCII, and Bool.
        /// </summary>
        private bool IsNumericFormat(DisplayFormat format)
        {
            string fmtStr = format.ToString();

            // Exclude all formats that contain string/binary representation or boolean
            if (fmtStr.Contains("Hex") || fmtStr.Contains("Binary") || fmtStr.Contains("ASCII") || format == DisplayFormat.Bool16)
            {
                return false;
            }

            // All remaining formats (Signed, Unsigned, Float, Double) are numeric
            return true;
        }

        /// <summary>
        /// Returns the number of 16-bit registers required for a given format.
        /// </summary>
        public int GetRegistersForFormat(DisplayFormat format) // Zmieniono na PUBLIC
        {
            string fmtStr = format.ToString();

            // 64-bit formats (Double, Long, ASCII64, Hex64) require 4 registers
            if (fmtStr.Contains("64"))
                return 4;

            // 32-bit formats (Float, Int, UInt, ASCII32, Hex32) require 2 registers
            if (fmtStr.Contains("32"))
                return 2;

            // 16-bit formats (or 16-bit ASCII) require 1 register
            return 1;
        }

        /// <summary>
        /// Builds a byte array from raw Modbus registers (ushort)
        /// respecting Big/Little-Endian and Byte-Swap settings.
        /// </summary>
        /// <param name="rowIndex">The starting index in the _rawData array</param>
        /// <param name="numRegisters">Number of registers to read (e.g., 2 for 32-bit, 4 for 64-bit)</param>
        /// <param name="isLE_Format">True if the format is Little-Endian (register order is swapped)</param>
        /// <param name="isBS_Format">True if the format requires byte-swapping within each register</param>
        /// <returns>A byte array in Big-Endian order (MSB first)</returns>
        private byte[] BuildByteArray(int rowIndex, int numRegisters, bool isLE_Format, bool isBS_Format)
        {
            byte[] bytes = new byte[numRegisters * 2];

            // 1. Get registers and apply byte-swapping (BS) if needed
            ushort[] regs = new ushort[numRegisters];
            for (int i = 0; i < numRegisters; i++)
            {
                ushort raw = _rawData[rowIndex + i];
                // (Hi, Lo) -> (Lo, Hi) or vice-versa
                regs[i] = isBS_Format ? (ushort)((raw << 8) | (raw >> 8)) : raw;
            }

            // 2. Arrange registers in the correct order (BE or LE)
            for (int i = 0; i < numRegisters; i++)
            {
                // For BE (e.g., 64-bit): 0, 1, 2, 3
                // For LE (e.g., 64-bit): 3, 2, 1, 0 (reversed WORD/REGISTER order)
                int regIndex = isLE_Format ? (numRegisters - 1 - i) : i;

                // 3. Split each register into bytes (Hi, Lo)
                // BitConverter.GetBytes on a PC (Little Endian) will return [Lo, Hi]
                byte[] regBytes = BitConverter.GetBytes(regs[regIndex]);

                // We assemble the final array in Big-Endian (MSB first) order
                bytes[i * 2] = regBytes[1]; // Hi Byte
                bytes[i * 2 + 1] = regBytes[0]; // Lo Byte
            }

            return bytes;
        }


        /// <summary>
        /// Formats the final display string for a given row based on its DisplayFormat.
        /// </summary>
        private string FormatValue(int rowIndex)
        {
            if (_rawData == null || rowIndex >= _rawData.Length)
                return "---";

            DisplayFormat format = (DisplayFormat)dataGridView1.Rows[rowIndex].Cells["DisplayFormatColumn"].Value;
            int regsNeeded = GetRegistersForFormat(format);
            string fmtStr = format.ToString();

            // Check if we have enough data to format
            if (rowIndex + regsNeeded > _rawData.Length)
                return "ERROR: NOT ENOUGH DATA";

            try
            {
                // --- 16-bit Logic ---
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
                            byte[] asciiBytes = BitConverter.GetBytes(val);
                            // Swap bytes for readable ASCII (Hi, Lo) -> (Lo, Hi)
                            if (BitConverter.IsLittleEndian)
                                return $"{(char)asciiBytes[0]}{(char)asciiBytes[1]}";
                            else
                                return $"{(char)asciiBytes[1]}{(char)asciiBytes[0]}";
                        case DisplayFormat.Unsigned16:
                        default:
                            return val.ToString();
                    }
                }

                bool isLE_Format = fmtStr.Contains("_LE");
                bool isBS_Format = fmtStr.Contains("_BS");
                bool isAscii = fmtStr.Contains("ASCII");
                bool isHex = fmtStr.Contains("Hex");

                // --- 32-bit Logic ---
                if (regsNeeded == 2)
                {
                    byte[] bytes = BuildByteArray(rowIndex, 2, isLE_Format, isBS_Format);

                    // BuildByteArray always returns in Big-Endian (MSB...LSB).
                    // BitConverter on a PC (Little Endian) expects LSB...MSB.
                    // Therefore, we MUST reverse the byte array for BitConverter to work correctly.
                    // We skip this for ASCII and Hex, which we want to display as-is (MSB first).
                    if (!isAscii && !isHex)
                    {
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(bytes);
                    }

                    switch (format)
                    {
                        // 32-bit Numeric
                        case DisplayFormat.Unsigned32_BE:
                        case DisplayFormat.Unsigned32_LE:
                        case DisplayFormat.Unsigned32_BE_BS:
                        case DisplayFormat.Unsigned32_LE_BS:
                            return BitConverter.ToUInt32(bytes, 0).ToString();

                        case DisplayFormat.Signed32_BE:
                        case DisplayFormat.Signed32_LE:
                        case DisplayFormat.Signed32_BE_BS:
                        case DisplayFormat.Signed32_LE_BS:
                            return BitConverter.ToInt32(bytes, 0).ToString();

                        case DisplayFormat.Float32_BE:
                        case DisplayFormat.Float32_LE:
                        case DisplayFormat.Float32_BE_BS:
                        case DisplayFormat.Float32_LE_BS:
                            return BitConverter.ToSingle(bytes, 0).ToString("F3", System.Globalization.CultureInfo.InvariantCulture); // 3 decimal places

                        // 32-bit ASCII (bytes are already MSB...LSB)
                        case DisplayFormat.ASCII32_BE:
                        case DisplayFormat.ASCII32_LE:
                        case DisplayFormat.ASCII32_BE_BS:
                        case DisplayFormat.ASCII32_LE_BS:
                            return Encoding.ASCII.GetString(bytes).Replace("\0", " "); // Clean null chars

                        // 32-bit Hex (bytes are already MSB...LSB)
                        case DisplayFormat.Hex32_BE:
                        case DisplayFormat.Hex32_LE:
                        case DisplayFormat.Hex32_BE_BS:
                        case DisplayFormat.Hex32_LE_BS:
                            return "0x" + BitConverter.ToString(bytes).Replace("-", "");
                    }
                }

                // --- 64-bit Logic ---
                if (regsNeeded == 4)
                {
                    byte[] bytes = BuildByteArray(rowIndex, 4, isLE_Format, isBS_Format);

                    // Same logic as 32-bit: Reverse for BitConverter if on a Little-Endian machine.
                    if (!isAscii && !isHex)
                    {
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(bytes);
                    }

                    switch (format)
                    {
                        // 64-bit Numeric (Unsigned)
                        case DisplayFormat.Unsigned64_BE:
                        case DisplayFormat.Unsigned64_LE:
                        case DisplayFormat.Unsigned64_BE_BS:
                        case DisplayFormat.Unsigned64_LE_BS:
                            return BitConverter.ToUInt64(bytes, 0).ToString();

                        // 64-bit Numeric (Signed)
                        case DisplayFormat.Signed64_BE:
                        case DisplayFormat.Signed64_LE:
                        case DisplayFormat.Signed64_BE_BS:
                        case DisplayFormat.Signed64_LE_BS:
                            return BitConverter.ToInt64(bytes, 0).ToString();

                        // 64-bit Numeric (Float/Double)
                        case DisplayFormat.Double64_BE:
                        case DisplayFormat.Double64_LE:
                        case DisplayFormat.Float64_BE_BS: // Alias for Double
                        case DisplayFormat.Float64_LE_BS: // Alias for Double
                            return BitConverter.ToDouble(bytes, 0).ToString("F5", System.Globalization.CultureInfo.InvariantCulture); // 5 decimal places

                        // 64-bit ASCII (bytes are already MSB...LSB)
                        case DisplayFormat.ASCII64_BE:
                        case DisplayFormat.ASCII64_LE:
                        case DisplayFormat.ASCII64_BE_BS:
                        case DisplayFormat.ASCII64_LE_BS:
                            return Encoding.ASCII.GetString(bytes).Replace("\0", " "); // Clean null chars

                        // 64-bit Hex
                        case DisplayFormat.Hex64_BE:
                        case DisplayFormat.Hex64_LE:
                        case DisplayFormat.Hex64_BE_BS:
                        case DisplayFormat.Hex64_LE_BS:
                            return "0x" + BitConverter.ToString(bytes).Replace("-", "");
                    }
                }
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }

            return "---"; // Fallback
        }


        /// <summary>
        /// Refreshes the entire grid display. This function applies formatting,
        /// hides rows for multi-register formats, and updates register number ranges.
        /// </summary>
        public void RefreshDisplayValues()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshDisplayValues));
                return;
            }

            dataGridView1.SuspendLayout(); // Suspend layout for performance

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].IsNewRow) continue;

                // Step 1: Ensure row is visible (in case format changed)
                dataGridView1.Rows[i].Visible = true;

                // Step 2: Get the format and number of registers needed
                if (dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value == null)
                {
                    dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value = DisplayFormat.Unsigned16;
                }
                DisplayFormat format = (DisplayFormat)dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value;
                int regsNeeded = GetRegistersForFormat(format);

                // --- LOGIKA CHARTINGOWA: Wyłączanie dla typów nienumerycznych ---
                bool isNumeric = IsNumericFormat(format);
                DataGridViewCheckBoxCell chartCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["Chart"];

                chartCell.ReadOnly = !isNumeric;

                if (!isNumeric && (bool)(chartCell.Value ?? false))
                {
                    chartCell.Value = false;
                }

                // Ustaw wizualny stan wyłączonego checkboxa
                chartCell.Style.BackColor = isNumeric ? dataGridView1.DefaultCellStyle.BackColor : SystemColors.ControlLight;
                // --- KONIEC LOGIKI CHARTINGOWEJ ---

                // Umożliwia edycję tylko dla Coils (FC 01, index 0) i Holding Registers (FC 03, index 2)
                bool isCoilOrHolding = (GetFunctionCode() == 0 || GetFunctionCode() == 2);

                // Nowe: Zezwalamy na edycję tylko wiersza "głównego" (pierwszego w grupie)
                bool isEditable = isCoilOrHolding && (i + regsNeeded <= dataGridView1.Rows.Count);
                dataGridView1.Rows[i].Cells["Value"].ReadOnly = !isEditable;

                // Step 3: Format the "RegisterNumber" cell
                int baseRegNum = i + (int)startRegister.Value;
                if (regsNeeded == 1)
                {
                    dataGridView1.Rows[i].Cells["RegisterNumber"].Value = baseRegNum;
                }
                else
                {
                    int endRegNum = baseRegNum + regsNeeded - 1;
                    dataGridView1.Rows[i].Cells["RegisterNumber"].Value = $"{baseRegNum} - {endRegNum}";
                }

                // Step 4: Format the "Value" cell
                dataGridView1.Rows[i].Cells["Value"].Value = FormatValue(i);

                // Step 5: Hide subsequent rows that are part of this multi-register value
                if (regsNeeded > 1)
                {
                    for (int j = 1; j < regsNeeded; j++)
                    {
                        if (i + j < dataGridView1.Rows.Count)
                        {
                            dataGridView1.Rows[i + j].Visible = false;
                            dataGridView1.Rows[i + j].Cells["Value"].ReadOnly = true; // Ukryte wiersze nie są edytowalne

                            // Także jawne wyłączenie/odznaczenie wykresu dla ukrytych wierszy
                            DataGridViewCheckBoxCell hiddenChartCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i + j].Cells["Chart"];
                            hiddenChartCell.ReadOnly = true;
                            hiddenChartCell.Value = false;
                            hiddenChartCell.Style.BackColor = SystemColors.ControlLight;
                        }
                    }
                    i += (regsNeeded - 1); // Skip the rows we just hid
                }
            }
            dataGridView1.ResumeLayout();

            // NOWE: Wywołaj zdarzenie po pełnym odświeżeniu wartości
            OnChartDataUpdated(new ChartDataUpdateEventArgs(GetChartData()));
        }

        #endregion

        // --- 64-bit Format Click Handlers ---

        // 64-bit Unsigned
        private void bigendianToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Unsigned64_BE);
        }

        private void littleendianToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Unsigned64_LE);
        }

        private void bigendianByteSwapToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Unsigned64_BE_BS);
        }

        private void littleendianByteSwapToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Unsigned64_LE_BS);
        }

        // 64-bit Signed
        private void bigendianToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Signed64_BE);
        }

        private void littleendianToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Signed64_LE);
        }

        private void bigendianByteSwapToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Signed64_BE_BS);
        }

        private void littleendianByteSwapToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Signed64_LE_BS);
        }

        // 64-bit Real (Double)
        private void bigendianToolStripMenuItem8_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Double64_BE);
        }

        private void littleendianToolStripMenuItem8_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Double64_LE);
        }

        private void bigendianByteSwapToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Float64_BE_BS);
        }

        private void littleendianByteSwapToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Float64_LE_BS);
        }

        // 64-bit ASCII
        private void bigendianToolStripMenuItem10_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.ASCII64_BE);
        }

        private void littleendianToolStripMenuItem10_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.ASCII64_LE);
        }

        private void bigendianByteSwapToolStripMenuItem8_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.ASCII64_BE_BS);
        }

        private void littleendianByteSwapToolStripMenuItem8_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.ASCII64_LE_BS);
        }


        // 64-bit Hex
        private void hex64BEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex64_BE);
        }

        private void hex64LEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex64_LE);
        }

        private void hex64BEBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex64_BE_BS);
        }

        private void hex64LEBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex64_LE_BS);
        }

        // --- Empty designer-generated handlers (now connected to Hex64) ---
        private void bigendianToolStripMenuItem9_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex64_BE);
        }

        private void littleendianToolStripMenuItem9_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex64_LE);
        }

        private void bigendianByteSwapToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex64_BE_BS);
        }

        private void littleendianByteSwapToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Hex64_LE_BS);
        }

        private void boolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFormatToSelected(DisplayFormat.Bool16);
        }
    }


    // KLASY POMOCNICZE WSPÓLNE DLA CAŁEJ PRZESTRZENI NAZW
    public class ChartDataPoint
    {
        public string SeriesName { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ChartDataUpdateEventArgs : EventArgs
    {
        public List<ChartDataPoint> DataPoints { get; }
        public ChartDataUpdateEventArgs(List<ChartDataPoint> dataPoints)
        {
            DataPoints = dataPoints;
        }
    }

    /// <summary>
    /// Argumenty zdarzenia żądania zapisu z ReadingsTab.
    /// </summary>
    public class WriteRequestedEventArgs : EventArgs
    {
        public byte SlaveId { get; set; }
        // 0=Coil (Read FC 01), 2=Holding Register (Read FC 03)
        public int FunctionCode { get; set; }
        public ushort StartAddress { get; set; } // Adres Modbus wiersza
        public string ValueString { get; set; }
        public ReadingsTab ReadingsTab { get; set; }
        public ReadingsTab.DisplayFormat Format { get; set; } // Format danych
        public int RowIndex { get; set; } // Index w DataGridView

        public WriteRequestedEventArgs(int functionCode, ushort startAddress, string valueString, ReadingsTab tab, ReadingsTab.DisplayFormat format, int rowIndex)
        {
            FunctionCode = functionCode;
            StartAddress = startAddress;
            ValueString = valueString;
            ReadingsTab = tab;
            Format = format;
            RowIndex = rowIndex;
        }
    }
    // KONIEC KLAS POMOCNICZYCH
}