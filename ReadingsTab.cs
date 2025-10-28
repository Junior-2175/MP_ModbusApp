using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Text; // Dodane dla obsługi ASCII
using System.Collections.Generic; // Dodane dla Dictionary

namespace MP_ModbusApp
{
    public partial class ReadingsTab : UserControl
    {
        private bool _isUpdatingValues = false;
        private ushort[] _rawData = null;

        public enum DisplayFormat
        {
            // 16-bit
            Unsigned16,
            Signed16,
            Hex16,
            Binary16,
            ASCII,

            // 32-bit (BE = Big-Endian, LE = Little-Endian)
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
            Float32_LE_BS,
            ASCII32_BE,
            ASCII32_LE,
            ASCII32_BE_BS,
            ASCII32_LE_BS,
            // NOWE FORMATY HEX 32
            Hex32_BE,
            Hex32_LE,
            Hex32_BE_BS,
            Hex32_LE_BS,


            // 64-bit (analogicznie)
            Unsigned64_BE,
            Signed64_BE,
            Double64_BE, // Real (64-bit)
            Unsigned64_LE,
            Signed64_LE,
            Double64_LE,
            Unsigned64_BE_BS,
            Signed64_BE_BS,
            Float64_BE_BS, // Real (alias dla Double)
            Unsigned64_LE_BS,
            Signed64_LE_BS,
            Float64_LE_BS, // alias dla Double
            ASCII64_BE,
            ASCII64_LE,
            ASCII64_BE_BS,
            ASCII64_LE_BS,
            // NOWE FORMATY HEX 64
            Hex64_BE,
            Hex64_LE,
            Hex64_BE_BS,
            Hex64_LE_BS
        }
        public ReadingsTab()
        {

            InitializeComponent();

            // --- TAGOWANIE ELEMENTÓW MENU DLA CHECKBOXÓW ---
            // (Ten kod jest taki sam jak w poprzedniej odpowiedzi)

            // 16-bit
            this.unsignedToolStripMenuItem.Tag = DisplayFormat.Unsigned16;
            this.signedToolStripMenuItem.Tag = DisplayFormat.Signed16;
            this.binaryToolStripMenuItem.Tag = DisplayFormat.Binary16;
            this.hexToolStripMenuItem.Tag = DisplayFormat.Hex16;
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


            // --- PODŁĄCZENIE WSZYSTKICH HANDLERÓW Z DESIGNERA ---
            // (Ten kod jest taki sam jak w poprzedniej odpowiedzi)

            // 32-bit Unsigned BS
            this.bigendianToolStripMenuItem1.Click += new System.EventHandler(this.unsigned32BEBSToolStripMenuItem_Click);
            this.bigendianToolStripMenuItem1.Tag = DisplayFormat.Unsigned32_BE_BS;
            this.littleendianToolStripMenuItem1.Click += new System.EventHandler(this.unsigned32LEBSToolStripMenuItem_Click);
            this.littleendianToolStripMenuItem1.Tag = DisplayFormat.Unsigned32_LE_BS;

            // 32-bit Signed BS
            this.bigendianByteSwapToolStripMenuItem.Click += new System.EventHandler(this.signed32BEBSToolStripMenuItem_Click);
            this.bigendianByteSwapToolStripMenuItem.Tag = DisplayFormat.Signed32_BE_BS;
            this.littleendianByteSwapToolStripMenuItem1.Click += new System.EventHandler(this.signed32LEBSToolStripMenuItem_Click);
            this.littleendianByteSwapToolStripMenuItem1.Tag = DisplayFormat.Signed32_LE_BS;

            // 32-bit Real BS
            this.bigendianByteSwapToolStripMenuItem1.Click += new System.EventHandler(this.float32BEBSToolStripMenuItem_Click);
            this.bigendianByteSwapToolStripMenuItem1.Tag = DisplayFormat.Float32_BE_BS;
            this.littleendianByteSwapToolStripMenuItem.Click += new System.EventHandler(this.float32LEBSToolStripMenuItem_Click);
            this.littleendianByteSwapToolStripMenuItem.Tag = DisplayFormat.Float32_LE_BS;

            // 32-bit Hex
            this.bigendianToolStripMenuItem4.Click += new System.EventHandler(this.hex32BEToolStripMenuItem_Click);
            this.bigendianToolStripMenuItem4.Tag = DisplayFormat.Hex32_BE;
            this.littleendianToolStripMenuItem5.Click += new System.EventHandler(this.hex32LEToolStripMenuItem_Click);
            this.littleendianToolStripMenuItem5.Tag = DisplayFormat.Hex32_LE;
            this.bigendianByteSwapToolStripMenuItem2.Click += new System.EventHandler(this.hex32BEBSToolStripMenuItem_Click);
            this.bigendianByteSwapToolStripMenuItem2.Tag = DisplayFormat.Hex32_BE_BS;
            this.littleendianByteSwapToolStripMenuItem2.Click += new System.EventHandler(this.hex32LEBSToolStripMenuItem_Click);
            this.littleendianByteSwapToolStripMenuItem2.Tag = DisplayFormat.Hex32_LE_BS;

            // 32-bit ASCII
            this.bigendianToolStripMenuItem5.Click += new System.EventHandler(this.ascii32BEToolStripMenuItem_Click);
            this.bigendianToolStripMenuItem5.Tag = DisplayFormat.ASCII32_BE;
            this.littleendianToolStripMenuItem4.Click += new System.EventHandler(this.ascii32LEToolStripMenuItem_Click);
            this.littleendianToolStripMenuItem4.Tag = DisplayFormat.ASCII32_LE;
            this.bigendianByteSwapToolStripMenuItem3.Click += new System.EventHandler(this.ascii32BEBSToolStripMenuItem_Click);
            this.bigendianByteSwapToolStripMenuItem3.Tag = DisplayFormat.ASCII32_BE_BS;
            this.littleendianByteSwapToolStripMenuItem3.Click += new System.EventHandler(this.ascii32LEBSToolStripMenuItem_Click);
            this.littleendianByteSwapToolStripMenuItem3.Tag = DisplayFormat.ASCII32_LE_BS;

            // 64-bit Hex
            this.bigendianToolStripMenuItem9.Click += new System.EventHandler(this.hex64BEToolStripMenuItem_Click);
            this.bigendianToolStripMenuItem9.Tag = DisplayFormat.Hex64_BE;
            this.littleendianToolStripMenuItem9.Click += new System.EventHandler(this.hex64LEToolStripMenuItem_Click);
            this.littleendianToolStripMenuItem9.Tag = DisplayFormat.Hex64_LE;
            this.bigendianByteSwapToolStripMenuItem7.Click += new System.EventHandler(this.hex64BEBSToolStripMenuItem_Click);
            this.bigendianByteSwapToolStripMenuItem7.Tag = DisplayFormat.Hex64_BE_BS;
            this.littleendianByteSwapToolStripMenuItem7.Click += new System.EventHandler(this.hex64LEBSToolStripMenuItem_Click);
            this.littleendianByteSwapToolStripMenuItem7.Tag = DisplayFormat.Hex64_LE_BS;
        }

        private void ReadingsTab_Load(object sender, EventArgs e)
        {
            //comboBox1.SelectedIndex = 2;
            dataGridView1.RowCount = (int)numOfRegisters.Value;
            dataGridView1.RowTemplate.Height = 10;
            lblTabError.Visible = false;
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
            dataGridView1.SuspendLayout();

            int newRowCount = (int)numOfRegisters.Value;

            // Zapisz formaty przed zmianą rozmiaru (opcjonalne, ale dobre)
            var formats = new Dictionary<int, DisplayFormat>();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells["RegisterNumber"].Value != null)
                {
                    int regNum = 0;
                    string regVal = dataGridView1.Rows[i].Cells["RegisterNumber"].Value.ToString();
                    // Pobierz pierwszy numer z "100" lub "100 - 103"
                    int.TryParse(regVal.Split(' ')[0], out regNum);

                    if (regNum != 0 && dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value != null)
                    {
                        formats[regNum] = (DisplayFormat)dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value;
                    }
                }
            }

            dataGridView1.RowCount = newRowCount; // Zastosuj nową liczbę wierszy

            // Zainicjuj wszystkie wiersze (nowe i stare)
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].IsNewRow) continue;

                int currentRegNum = i + (int)startRegister.Value;
                dataGridView1.Rows[i].Cells["RegisterNumber"].Value = currentRegNum;
                dataGridView1.Rows[i].Cells["Name"].Value = "Register_" + currentRegNum;
                dataGridView1.Rows[i].Visible = true; // Zawsze resetuj widoczność

                // Przywróć stary format, jeśli istnieje, lub ustaw domyślny
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

            // Na koniec, zastosuj całą logikę formatowania i ukrywania
            RefreshDisplayValues();
        }

        /// <summary>
        /// Funkcja pomocnicza do stosowania formatu na wszystkich zaznaczonych komórkach.
        /// </summary>
        private void ApplyFormatToSelected(DisplayFormat format)
        {
            if (dataGridView1.SelectedCells.Count == 0) return;

            // Sprawdź, czy można zastosować format (Feature 3)
            int regsNeeded = GetRegistersForFormat(format);
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                // Stosuj tylko do komórek w kolumnie "Value"
                if (cell.ColumnIndex == Value.Index)
                {
                    // Sprawdź, czy format nie wyjdzie poza siatkę
                    if (cell.RowIndex + regsNeeded > dataGridView1.Rows.Count)
                    {
                        MessageBox.Show($"Nie można zastosować formatu wymagającego {regsNeeded} rejestrów w wierszu {cell.RowIndex}. Niewystarczająca liczba kolejnych rejestrów.", "Błąd formatowania", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return; // Przerwij operację
                    }
                }
            }

            // Zastosuj format
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                if (cell.ColumnIndex == Value.Index)
                {
                    // Ustaw format dla *pierwszego* wiersza
                    dataGridView1.Rows[cell.RowIndex].Cells["DisplayFormatColumn"].Value = format;

                    // Zresetuj formaty dla "ukrytych" wierszy, które teraz będą częścią tego
                    for (int i = 1; i < regsNeeded; i++)
                    {
                        if (cell.RowIndex + i < dataGridView1.Rows.Count)
                        {
                            dataGridView1.Rows[cell.RowIndex + i].Cells["DisplayFormatColumn"].Value = DisplayFormat.Unsigned16; // Reset
                        }
                    }
                }
            }

            // Odśwież całą siatkę, aby pokazać zmiany (ukryć wiersze itp.)
            RefreshDisplayValues();
        }

        // --- ZAKTUALIZOWANY KOD ---
        /// <summary>
        /// Rekursywnie przeszukuje elementy menu i ustawia 'Checked' na tym,
        /// który pasuje do bieżącego formatu komórki, ORAZ na jego rodzicach.
        /// </summary>
        /// <returns>Zwraca 'true', jeśli ten element lub jego potomek jest zaznaczony.</returns>
        private bool UpdateMenuChecks(ToolStripItemCollection items, DisplayFormat currentFormat)
        {
            bool anyChildInThisListIsChecked = false;

            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    bool isThisItemTheOne = false;
                    bool isDescendantChecked = false;

                    // 1. Sprawdź, czy ten element jest finalnym wyborem (ma Tag)
                    if (menuItem.Tag is DisplayFormat itemFormat)
                    {
                        if (itemFormat == currentFormat)
                        {
                            isThisItemTheOne = true;
                            anyChildInThisListIsChecked = true; // Sygnał dla rodzica
                        }
                    }

                    // 2. Rekurencja, jeśli element ma pod-menu
                    if (menuItem.HasDropDownItems)
                    {
                        // Jeśli potomek jest zaznaczony, ten rodzic też musi być
                        isDescendantChecked = UpdateMenuChecks(menuItem.DropDownItems, currentFormat);
                        if (isDescendantChecked)
                        {
                            anyChildInThisListIsChecked = true; // Sygnał dla rodzica
                        }
                    }

                    // 3. Ustaw stan zaznaczenia
                    // Element jest zaznaczony, jeśli:
                    // A) Jest to dokładnie ten element, który wybrał użytkownik (isThisItemTheOne)
                    // LUB
                    // B) Jest rodzicem dla elementu, który wybrał użytkownik (isDescendantChecked)
                    menuItem.Checked = (isThisItemTheOne || isDescendantChecked);
                }
            }

            // Zwróć status do wywołania nadrzędnego
            return anyChildInThisListIsChecked;
        }
        // --- KONIEC ZAKTUALIZOWANEGO KODU ---


        /// <summary>
        /// Obsługa zdarzenia 'Opening' menu kontekstowego (Feature 3)
        /// </summary>
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0)
            {
                e.Cancel = true; // Nie pokazuj menu, jeśli nic nie zaznaczono
                return;
            }

            // --- Logika zaznaczania (taka sama jak w poprzedniej odpowiedzi) ---
            DataGridViewCell firstSelectedCell = null;
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                if (cell.ColumnIndex == Value.Index)
                {
                    firstSelectedCell = cell;
                    break;
                }
            }

            DisplayFormat currentFormat = DisplayFormat.Unsigned16; // Domyślnie

            if (firstSelectedCell != null &&
                dataGridView1.Rows[firstSelectedCell.RowIndex].Cells["DisplayFormatColumn"].Value != null)
            {
                currentFormat = (DisplayFormat)dataGridView1.Rows[firstSelectedCell.RowIndex].Cells["DisplayFormatColumn"].Value;
            }

            // Zaktualizuj "check" status dla wszystkich itemów w menu
            // Wywołanie jest takie samo, ale teraz funkcja robi więcej
            UpdateMenuChecks(contextMenuStrip1.Items, currentFormat);

            // --- Logika włączania/wyłączania (taka sama jak w poprzedniej odpowiedzi) ---
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
                toolStripMenuItem2.Enabled = false; // 16-bit
                toolStripMenuItem3.Enabled = false; // 32-bit
                toolStripMenuItem4.Enabled = false; // 64-bit
                return;
            }

            int rowsAvailable = dataGridView1.Rows.Count - maxRowIndex;

            toolStripMenuItem2.Enabled = (rowsAvailable >= 1); // 16-bit
            toolStripMenuItem3.Enabled = (rowsAvailable >= 2); // 32-bit
            toolStripMenuItem4.Enabled = (rowsAvailable >= 4); // 64-bit
        }


        // ZASTĄP puste metody Click dla menu 16-bit
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


        // Metody Click dla menu 32-bit.
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


        // ... i analogicznie dla 64-bit ...

        private void startRegister_1_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingValues) return;
            _isUpdatingValues = true;

            try
            {
                string originalString = (startRegister_1.Value - 1).ToString();
                string functionString = originalString.Substring(0, 1);
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
                if (functionString != requestedFuncionNo && comboBox1.SelectedIndex != 0 || (registerStringDouble < 0 && registerStringDouble > 65535))
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
            finally
            {
                _isUpdatingValues = false;
            }
        }

        public void SetRegisterDefinitions(List<Tuple<int, string, string>> registers)
        {
            foreach (var regDef in registers) // (int RegNum, string RegName, string RegFormat)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow || row.Cells["RegisterNumber"].Value == null) continue;

                    // Komórka "RegisterNumber" jest typu 'int' zaraz po 'datagridUpdate()'
                    if (row.Cells["RegisterNumber"].Value is int regNumInCell)
                    {
                        if (regNumInCell == regDef.Item1) // Item1 = RegisterNumber
                        {
                            row.Cells["Name"].Value = regDef.Item2; // Item2 = RegisterName

                            // NOWY KOD: Ustaw DisplayFormat
                            // Próbujemy sparsować string z bazy (np. "Float32_BE") z powrotem na enum
                            if (Enum.TryParse<DisplayFormat>(regDef.Item3, out var displayFormat)) // Item3 = DisplayFormat (string)
                            {
                                row.Cells["DisplayFormatColumn"].Value = displayFormat;
                            }
                            else
                            {
                                // Wartość domyślna, jeśli format z bazy jest nieprawidłowy
                                row.Cells["DisplayFormatColumn"].Value = DisplayFormat.Unsigned16;
                            }
                            break; // Znaleziono pasujący wiersz, przejdź do następnego rejestru
                        }
                    }
                }
            }

            // Po ustawieniu WSZYSTKICH formatów, wywołujemy RefreshDisplayValues() JEDEN RAZ.
            // To skonsoliduje wiersze (np. 32-bitowe) i ukryje te, które trzeba.
            RefreshDisplayValues();
        }



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

        public void ClearTabError()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearTabError));
                return;
            }
            lblTabError.Visible = false;
        }

        // Dodaj tę metodę w pliku ReadingsTab.cs
        public void UpdateValues(ushort[] data)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateValues(data)));
                return;
            }

            if (data == null) return;

            _rawData = data; // Przechowaj surowe dane

            // Odśwież wyświetlanie w siatce
            RefreshDisplayValues();
        }

        /// <summary>
        /// Zwraca liczbę 16-bitowych rejestrów wymaganą przez dany format.
        /// (ZAKTUALIZOWANA WERSJA)
        /// </summary>
        private int GetRegistersForFormat(DisplayFormat format)
        {
            string fmtStr = format.ToString();

            // Format 64-bitowy (Double, Long, ASCII64, Hex64) wymaga 4 rejestrów
            if (fmtStr.Contains("64"))
                return 4;

            // Format 32-bitowy (Float, Int, UInt, ASCII32, Hex32) wymaga 2 rejestrów
            if (fmtStr.Contains("32"))
                return 2;

            // Domyślnie format 16-bitowy (lub ASCII 16-bit) wymaga 1 rejestru
            return 1;
        }

        /// <summary>
        /// Buduje tablicę bajtów z surowych rejestrów Modbus (ushort)
        /// uwzględniając kolejność (LE/BE) i zamianę bajtów (BS).
        /// (NOWA METODA POMOCNICZA)
        /// </summary>
        /// <param name="rowIndex">Indeks startowy w _rawData</param>
        /// <param name="numRegisters">Liczba rejestrów (2 dla 32-bit, 4 dla 64-bit)</param>
        /// <param name="isLE_Format">Czy format jest Little-Endian (zamieniona kolejność rejestrów)</param>
        /// <param name="isBS_Format">Czy format wymaga zamiany bajtów wewnątrz każdego rejestru</param>
        /// <returns>Tablica bajtów gotowa do konwersji (w kolejności Big-Endian)</returns>
        private byte[] BuildByteArray(int rowIndex, int numRegisters, bool isLE_Format, bool isBS_Format)
        {
            byte[] bytes = new byte[numRegisters * 2];

            // 1. Pobierz rejestry i zastosuj zamianę bajtów (BS) jeśli trzeba
            ushort[] regs = new ushort[numRegisters];
            for (int i = 0; i < numRegisters; i++)
            {
                ushort raw = _rawData[rowIndex + i];
                regs[i] = isBS_Format ? (ushort)((raw << 8) | (raw >> 8)) : raw;
            }

            // 2. Ułóż rejestry w odpowiedniej kolejności (BE lub LE)
            for (int i = 0; i < numRegisters; i++)
            {
                // Dla BE: 0, 1, 2, 3
                // Dla LE: 3, 2, 1, 0 (odwrócona kolejność SŁÓW/REJESTRÓW)
                int regIndex = isLE_Format ? (numRegisters - 1 - i) : i;

                // 3. Rozbij każdy rejestr na bajty (Hi, Lo)
                byte[] regBytes = BitConverter.GetBytes(regs[regIndex]); // Na PC (LE) da to [Lo, Hi]

                // Składamy bajty w kolejności Big-Endian (zawsze Hi, Lo)
                bytes[i * 2] = regBytes[1]; // Hi Byte
                bytes[i * 2 + 1] = regBytes[0]; // Lo Byte
            }

            return bytes;
        }


        /// <summary>
        /// Formatuje wartość dla danego wiersza na podstawie surowych danych i formatu wiersza.
        /// (KOMPLETNA POPRAWIONA WERSJA)
        /// </summary>
        private string FormatValue(int rowIndex)
        {
            if (_rawData == null || rowIndex >= _rawData.Length)
                return "---";

            DisplayFormat format = (DisplayFormat)dataGridView1.Rows[rowIndex].Cells["DisplayFormatColumn"].Value;
            int regsNeeded = GetRegistersForFormat(format);
            string fmtStr = format.ToString();

            // Zabezpieczenie (Feature 3)
            if (rowIndex + regsNeeded > _rawData.Length)
                return "BŁĄD: ZBYT MAŁO DANYCH";

            try
            {
                // --- Logika 16-bit ---
                if (regsNeeded == 1)
                {
                    ushort val = _rawData[rowIndex];
                    switch (format)
                    {
                        case DisplayFormat.Signed16: return ((short)val).ToString();
                        case DisplayFormat.Hex16: return $"0x{val:X4}";
                        case DisplayFormat.Binary16: return Convert.ToString(val, 2).PadLeft(16, '0');
                        case DisplayFormat.ASCII:
                            byte[] asciiBytes = BitConverter.GetBytes(val);
                            // Zamień bajty, jeśli trzeba (Hi, Lo) -> (Lo, Hi) dla czytelnego ASCII
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

                // --- Logika 32-bit ---
                if (regsNeeded == 2)
                {
                    byte[] bytes = BuildByteArray(rowIndex, 2, isLE_Format, isBS_Format);

                    // *** KLUCZOWA POPRAWKA LOGIKI ***
                    // BuildByteArray zawsze zwraca tablicę w kolejności Big Endian (MSB...LSB).
                    // BitConverter na PC (Little Endian) oczekuje tablicy w kolejności Little Endian (LSB...MSB).
                    // Dlatego *musimy* odwrócić tablicę, jeśli działamy na maszynie Little Endian.
                    // (Ignorujemy to dla ASCII i Hex, które chcemy wyświetlać w kolejności Big Endian).
                    if (!isAscii && !isHex)
                    {
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(bytes);
                    }

                    switch (format)
                    {
                        // 32-bit Numeryczne
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
                            return BitConverter.ToSingle(bytes, 0).ToString("F3"); // "F3" = 3 miejsca po przecinku

                        // 32-bit ASCII (dostarczone jako [Hi, Lo, Hi, Lo])
                        case DisplayFormat.ASCII32_BE:
                        case DisplayFormat.ASCII32_LE:
                        case DisplayFormat.ASCII32_BE_BS:
                        case DisplayFormat.ASCII32_LE_BS:
                            return Encoding.ASCII.GetString(bytes).Replace("\0", " "); // Czyścimy znaki null

                        // 32-bit Hex (dostarczone jako [Hi, Lo, Hi, Lo])
                        case DisplayFormat.Hex32_BE:
                        case DisplayFormat.Hex32_LE:
                        case DisplayFormat.Hex32_BE_BS:
                        case DisplayFormat.Hex32_LE_BS:
                            return "0x" + BitConverter.ToString(bytes).Replace("-", "");
                    }
                }

                // --- Logika 64-bit ---
                if (regsNeeded == 4)
                {
                    byte[] bytes = BuildByteArray(rowIndex, 4, isLE_Format, isBS_Format);

                    // *** KLUCZOWA POPRAWKA LOGIKI ***
                    // Ta sama logika co dla 32-bit.
                    if (!isAscii && !isHex)
                    {
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(bytes);
                    }

                    switch (format)
                    {
                        // 64-bit Numeryczne (Unsigned)
                        case DisplayFormat.Unsigned64_BE:
                        case DisplayFormat.Unsigned64_LE:
                        case DisplayFormat.Unsigned64_BE_BS:
                        case DisplayFormat.Unsigned64_LE_BS:
                            return BitConverter.ToUInt64(bytes, 0).ToString();

                        // 64-bit Numeryczne (Signed)
                        case DisplayFormat.Signed64_BE:
                        case DisplayFormat.Signed64_LE:
                        case DisplayFormat.Signed64_BE_BS:
                        case DisplayFormat.Signed64_LE_BS:
                            return BitConverter.ToInt64(bytes, 0).ToString();

                        // 64-bit Numeryczne (Float/Double)
                        case DisplayFormat.Double64_BE:
                        case DisplayFormat.Double64_LE:
                        case DisplayFormat.Float64_BE_BS:
                        case DisplayFormat.Float64_LE_BS:
                            return BitConverter.ToDouble(bytes, 0).ToString("F5"); // "F5" = 5 miejsc po przecinku

                        // 64-bit ASCII (nie wymaga odwracania)
                        case DisplayFormat.ASCII64_BE:
                        case DisplayFormat.ASCII64_LE:
                        case DisplayFormat.ASCII64_BE_BS:
                        case DisplayFormat.ASCII64_LE_BS:
                            return Encoding.ASCII.GetString(bytes).Replace("\0", " "); // Czyścimy znaki null

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
                return $"BŁĄD: {ex.Message}";
            }

            return "---";
        }


        /// <summary>
        /// Główna funkcja odświeżająca. Stosuje formaty i ukrywa wiersze. (Feature 1)
        /// </summary>
        public void RefreshDisplayValues()
        {
            if (_rawData == null) return;
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshDisplayValues));
                return;
            }

            dataGridView1.SuspendLayout(); // Poprawa wydajności

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].IsNewRow) continue;

                // Krok 1: Upewnij się, że wiersz jest widoczny (na wypadek zmiany formatu)
                dataGridView1.Rows[i].Visible = true;

                // Krok 2: Pobierz format i liczbę potrzebnych rejestrów
                if (dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value == null)
                {
                    dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value = DisplayFormat.Unsigned16;
                }
                DisplayFormat format = (DisplayFormat)dataGridView1.Rows[i].Cells["DisplayFormatColumn"].Value;
                int regsNeeded = GetRegistersForFormat(format);

                // Krok 3: Sformatuj komórkę "RegisterNumber" (Feature 1)
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

                // Krok 4: Sformatuj komórkę "Value"
                dataGridView1.Rows[i].Cells["Value"].Value = FormatValue(i);

                // Krok 5: Ukryj kolejne wiersze (Feature 1)
                if (regsNeeded > 1)
                {
                    for (int j = 1; j < regsNeeded; j++)
                    {
                        if (i + j < dataGridView1.Rows.Count)
                        {
                            dataGridView1.Rows[i + j].Visible = false;
                        }
                    }
                    i += (regsNeeded - 1); // Pomiń wiersze, które właśnie ukryliśmy
                }
            }
            dataGridView1.ResumeLayout();
        }

        // --- WYPEŁNIONE HANDLERY DLA 64-BIT (I INNYCH BRAKUJĄCYCH) ---

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

        // Puste handlery z designera (teraz podłączone do Hex64)
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

    }
}