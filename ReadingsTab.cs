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

            // 64-bit (analogicznie)
            Unsigned64_BE,
            Signed64_BE,
            Double64_BE, // Real (64-bit)
            Unsigned64_LE,
            Signed64_LE,
            Double64_LE,
            Unsigned64_BE_BS,
            Signed64_BE_BS,
            Float64_BE_BS, // Real
            Unsigned64_LE_BS,
            Signed64_LE_BS,
            Float64_LE_BS,
            ASCII64_BE,
            ASCII64_LE,
            ASCII64_BE_BS,
            ASCII64_LE_BS
            // Możesz dodać więcej kombinacji, np. "Byte Swap"
        }
        public ReadingsTab()
        {

            InitializeComponent();
        }

        private void ReadingsTab_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 2;
            dataGridView1.RowCount = (int)numOfRegisters.Value;

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

            // Znajdź maksymalny indeks wiersza wśród zaznaczonych komórek "Value"
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
                // Nie zaznaczono żadnej komórki "Value"
                toolStripMenuItem2.Enabled = false; // 16-bit
                toolStripMenuItem3.Enabled = false; // 32-bit
                toolStripMenuItem4.Enabled = false; // 64-bit
                return;
            }

            int rowsAvailable = dataGridView1.Rows.Count - maxRowIndex;

            // Włącz/wyłącz opcje menu na podstawie dostępnego miejsca
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

        // Dodaj nowe metody Click dla menu 32-bit i 64-bit.
        // Musisz je podłączyć w `ReadingsTab.Designer.cs`!
        // Np. this.bigendianToolStripMenuItem.Click += new System.EventHandler(this.unsigned32BEToolStripMenuItem_Click);

        // Przykłady (musisz je dodać i podłączyć w Designerze):
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

        public void SetRegisterDefinitions(List<Tuple<int, string>> registers)
        {
            foreach (var regDef in registers)
            {
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
        /// </summary>
        private int GetRegistersForFormat(DisplayFormat format)
        {
            switch (format)
            {
                case DisplayFormat.Unsigned32_BE:
                case DisplayFormat.Signed32_BE:
                case DisplayFormat.Float32_BE:
                case DisplayFormat.Unsigned32_LE:
                case DisplayFormat.Signed32_LE:
                case DisplayFormat.Float32_LE:
                    return 2;

                case DisplayFormat.Unsigned64_BE:
                case DisplayFormat.Signed64_BE:
                case DisplayFormat.Double64_BE:
                case DisplayFormat.Unsigned64_LE:
                case DisplayFormat.Signed64_LE:
                case DisplayFormat.Double64_LE:
                    return 4;

                default: // Wszystkie formaty 16-bitowe
                    return 1;
            }
        }

        /// <summary>
        /// Formatuje wartość dla danego wiersza na podstawie surowych danych i formatu wiersza.
        /// </summary>
        private string FormatValue(int rowIndex)
        {
            if (_rawData == null || rowIndex >= _rawData.Length)
                return "---";

            DisplayFormat format = (DisplayFormat)dataGridView1.Rows[rowIndex].Cells["DisplayFormatColumn"].Value;
            int regsNeeded = GetRegistersForFormat(format);

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
                        case DisplayFormat.ASCII: return new string(new[] { (char)val }); // Uproszczone
                        case DisplayFormat.Unsigned16:
                        default:
                            return val.ToString();
                    }
                }

                // --- Logika 32-bit ---
                if (regsNeeded == 2)
                {
                    // Kolejność bajtów dla 32-bit BE (Big-Endian): [Reg1_Hi, Reg1_Lo, Reg2_Hi, Reg2_Lo]
                    // Kolejność bajtów dla 32-bit LE (Little-Endian): [Reg2_Hi, Reg2_Lo, Reg1_Hi, Reg1_Lo]
                    // NModbus dostarcza ushorty w kolejności hosta (zazwyczaj LE).
                    // Musimy je poprawnie złożyć.

                    byte[] bytes = new byte[4];
                    bool isLE_Format = format.ToString().Contains("_LE");

                    byte[] reg1Bytes = BitConverter.GetBytes(_rawData[rowIndex]);
                    byte[] reg2Bytes = BitConverter.GetBytes(_rawData[rowIndex + 1]);

                    // Standardowa kolejność Modbus (Big-Endian dla rejestrów)
                    // Rejestr 1 (indeks 0) to wyższa część, Rejestr 2 (indeks 1) to niższa
                    // Musimy zamienić bajty wewnątrz ushort (Host LE -> Network BE)

                    // [Reg1_Hi, Reg1_Lo] = [reg1Bytes[1], reg1Bytes[0]]
                    // [Reg2_Hi, Reg2_Lo] = [reg2Bytes[1], reg2Bytes[0]]

                    if (!isLE_Format) // Big Endian (np. Float32_BE)
                    {
                        bytes[0] = reg1Bytes[1]; // Reg1_Hi
                        bytes[1] = reg1Bytes[0]; // Reg1_Lo
                        bytes[2] = reg2Bytes[1]; // Reg2_Hi
                        bytes[3] = reg2Bytes[0]; // Reg2_Lo
                    }
                    else // Little Endian (np. Float32_LE) - zamieniona kolejność rejestrów
                    {
                        bytes[0] = reg2Bytes[1]; // Reg2_Hi
                        bytes[1] = reg2Bytes[0]; // Reg2_Lo
                        bytes[2] = reg1Bytes[1]; // Reg1_Hi
                        bytes[3] = reg1Bytes[0]; // Reg1_Lo
                    }

                    // Nasz PC jest Little-Endian, więc jeśli format jest Big-Endian, musimy odwrócić
                    if (BitConverter.IsLittleEndian && !isLE_Format)
                        Array.Reverse(bytes);
                    // Jeśli nasz PC jest LE i format jest LE, nic nie robimy (bajty już są w kolejności LE)

                    switch (format)
                    {
                        case DisplayFormat.Unsigned32_BE:
                        case DisplayFormat.Unsigned32_LE:
                            return BitConverter.ToUInt32(bytes, 0).ToString();
                        case DisplayFormat.Signed32_BE:
                        case DisplayFormat.Signed32_LE:
                            return BitConverter.ToInt32(bytes, 0).ToString();
                        case DisplayFormat.Float32_BE:
                        case DisplayFormat.Float32_LE:
                            return BitConverter.ToSingle(bytes, 0).ToString("F3");
                    }
                }

                // --- Logika 64-bit (Analogicznie) ---
                if (regsNeeded == 4)
                {
                    // Implementacja analogiczna do 32-bit, ale składająca 8 bajtów z 4 rejestrów
                    return "Wartość 64-bit (TODO)";
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

        private void bigendianToolStripMenuItem6_Click(object sender, EventArgs e)
        {

        }

        private void littleendianToolStripMenuItem6_Click(object sender, EventArgs e)
        {

        }

        private void bigendianByteSwapToolStripMenuItem4_Click(object sender, EventArgs e)
        {

        }

        private void littleendianByteSwapToolStripMenuItem4_Click(object sender, EventArgs e)
        {

        }

        private void bigendianToolStripMenuItem7_Click(object sender, EventArgs e)
        {

        }

        private void littleendianToolStripMenuItem7_Click(object sender, EventArgs e)
        {

        }

        private void bigendianByteSwapToolStripMenuItem5_Click(object sender, EventArgs e)
        {

        }

        private void littleendianByteSwapToolStripMenuItem5_Click(object sender, EventArgs e)
        {

        }

        private void bigendianToolStripMenuItem8_Click(object sender, EventArgs e)
        {

        }

        private void littleendianToolStripMenuItem8_Click(object sender, EventArgs e)
        {

        }

        private void bigendianByteSwapToolStripMenuItem6_Click(object sender, EventArgs e)
        {

        }

        private void littleendianByteSwapToolStripMenuItem6_Click(object sender, EventArgs e)
        {

        }

        private void bigendianToolStripMenuItem9_Click(object sender, EventArgs e)
        {

        }

        private void littleendianToolStripMenuItem9_Click(object sender, EventArgs e)
        {

        }

        private void bigendianByteSwapToolStripMenuItem7_Click(object sender, EventArgs e)
        {

        }

        private void littleendianByteSwapToolStripMenuItem7_Click(object sender, EventArgs e)
        {

        }

        private void bigendianToolStripMenuItem10_Click(object sender, EventArgs e)
        {

        }

        private void littleendianToolStripMenuItem10_Click(object sender, EventArgs e)
        {

        }

        private void bigendianByteSwapToolStripMenuItem8_Click(object sender, EventArgs e)
        {

        }

        private void littleendianByteSwapToolStripMenuItem8_Click(object sender, EventArgs e)
        {

        }
    }
}