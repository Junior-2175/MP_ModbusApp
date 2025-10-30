using Microsoft.Data.Sqlite;
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

        private MainWindow _mainWindow;
        //private IModbusMaster _modbusMaster;
        private MP_modbus.IMyModbusMaster _modbusMaster;
        private bool _isPolling = false;


        public event EventHandler DeviceSaved;

        public string DeviceName
        {
            get => this.Text;
            set => this.Text = value;
        }

        public int SlaveId
        {
            get => (int)slaveId.Value;
            set => slaveId.Value = value;
        }

        public TabControl DeviceTabControl => tabPanel1;

        private TabPage tabToRename = null;
        private int tabNo = 0;
        public ModbusDevice()
        {
            InitializeComponent();
            addToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+ + ";
            removeToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+ - ";
            newReadingsToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+ + ";

            // Podłączenie przycisku "Pause"
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
        }

        private async void ModbusGroup_Load(object sender, EventArgs e)
        {
            if (this.MdiParent is MainWindow mw)
            {
                _mainWindow = mw;
            }

            lblDeviceStatus.Visible = false;
            tabNo = tabPanel1.TabPages.Count;
            if (tabPanel1.TabPages.Count == 0)
            {
                TabPage newTab = new TabPage();
                tabNo = tabNo + 1;
                tabPanel1.SelectedTab = newTab;
                newTab.Text = "Readings " + tabNo;
                newTab.Controls.Add(new ReadingsTab() { Dock = DockStyle.Fill });
                tabPanel1.TabPages.Add(newTab);
            }
            txtRenameTab.Visible = false;

            // Automatyczne uruchomienie pollingu
            await Task.Delay(100);
            startToolStripMenuItem_Click(sender, e);
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
                        contextMenuStrip1.Show(tabPanel1, e.Location);
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

        private void ModbusDevice_Resize(object sender, EventArgs e)
        {
            CancelRename();
        }

        private void newReadingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage newTab = new TabPage();
            tabNo = tabNo + 1;
            tabPanel1.SelectedTab = newTab;
            newTab.Text = "Readings " + tabNo;
            newTab.Controls.Add(new ReadingsTab() { Dock = DockStyle.Fill });
            tabPanel1.TabPages.Add(newTab);
        }

        private void tabPanel1_DoubleClick(object sender, EventArgs e)
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

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string oldName = this.Text;
            string newName;

            using (RenameForm renameDialog = new RenameForm())
            {
                renameDialog.newName = oldName;
                if (renameDialog.ShowDialog() == DialogResult.OK)
                {
                    newName = renameDialog.newName;
                }
                else
                {
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Please provide a device name to save.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Focus();
                return;
            }

            this.Text = newName;

            try
            {
                SaveDeviceConfiguration();
                MessageBox.Show("Device saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DeviceSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the device: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        private void SaveDeviceConfiguration()
        {
            using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    long deviceId;

                    var checkCmd = connection.CreateCommand();
                    checkCmd.Transaction = transaction;
                    checkCmd.CommandText = "SELECT DeviceId FROM Devices WHERE DeviceName = $name;";
                    checkCmd.Parameters.AddWithValue("$name", this.Text);

                    object existingIdResult = checkCmd.ExecuteScalar();

                    if (existingIdResult != null)
                    {
                        deviceId = (long)existingIdResult;

                        var deleteGroupsCmd = connection.CreateCommand();
                        deleteGroupsCmd.Transaction = transaction;
                        deleteGroupsCmd.CommandText = "DELETE FROM ReadingGroups WHERE DeviceId = $deviceId;";
                        deleteGroupsCmd.Parameters.AddWithValue("$deviceId", deviceId);
                        deleteGroupsCmd.ExecuteNonQuery();

                        var updateDeviceCmd = connection.CreateCommand();
                        updateDeviceCmd.Transaction = transaction;
                        updateDeviceCmd.CommandText = "UPDATE Devices SET SlaveId = $slaveId WHERE DeviceId = $deviceId;";
                        updateDeviceCmd.Parameters.AddWithValue("$slaveId", (int)slaveId.Value);
                        updateDeviceCmd.Parameters.AddWithValue("$deviceId", deviceId);
                        updateDeviceCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var insertDeviceCmd = connection.CreateCommand();
                        insertDeviceCmd.Transaction = transaction;
                        insertDeviceCmd.CommandText = "INSERT INTO Devices (DeviceName, SlaveId) VALUES ($name, $slaveId) RETURNING DeviceId;";
                        insertDeviceCmd.Parameters.AddWithValue("$name", this.Text);
                        insertDeviceCmd.Parameters.AddWithValue("$slaveId", (int)slaveId.Value);

                        deviceId = (long)insertDeviceCmd.ExecuteScalar();
                    }

                    foreach (TabPage tabPage in tabPanel1.TabPages)
                    {
                        if (tabPage.Controls[0] is ReadingsTab readingsTab)
                        {
                            var groupCmd = connection.CreateCommand();
                            groupCmd.Transaction = transaction;
                            groupCmd.CommandText = @"
                        INSERT INTO ReadingGroups (DeviceId, GroupName, FunctionCode, StartAddress, Quantity)
                        VALUES ($deviceId, $groupName, $funcCode, $startAddr, $quantity)
                        RETURNING GroupId;";

                            groupCmd.Parameters.AddWithValue("$deviceId", deviceId);
                            groupCmd.Parameters.AddWithValue("$groupName", tabPage.Text);
                            groupCmd.Parameters.AddWithValue("$funcCode", readingsTab.GetFunctionCode());
                            groupCmd.Parameters.AddWithValue("$startAddr", readingsTab.GetStartAddress());
                            groupCmd.Parameters.AddWithValue("$quantity", readingsTab.GetQuantity());

                            long groupId = (long)groupCmd.ExecuteScalar();

                            int startAddrForThisTab = readingsTab.GetStartAddress();

                            foreach (DataGridViewRow row in readingsTab.GetDataGridViewRows())
                            {
                                if (row.IsNewRow) continue;
                                int registerNumber = startAddrForThisTab + row.Index;

                                var regCmd = connection.CreateCommand();
                                regCmd.Transaction = transaction;
                                regCmd.CommandText = @"
                            INSERT INTO RegisterDefinitions (GroupId, RegisterNumber, RegisterName, DisplayFormatColumn)
                            VALUES ($groupId, $regNum, $regName, $displayFormat);";

                                regCmd.Parameters.AddWithValue("$groupId", groupId);
                                regCmd.Parameters.AddWithValue("$regNum", registerNumber);
                                regCmd.Parameters.AddWithValue("$regName", row.Cells["Name"].Value ?? string.Empty);

                                var displayFormat = row.Cells["DisplayFormatColumn"].Value?.ToString() ?? "Unsigned16";
                                regCmd.Parameters.AddWithValue("$displayFormat", displayFormat);
                                regCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();
                }
            }
        }
        private async void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _modbusMaster = _mainWindow?.ModbusMaster;
            if (_modbusMaster == null)
            {
                ShowDeviceError("Not connected in Setup tab!");
                return;
            }

            if (_isPolling) return;
            _isPolling = true;

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;

            await PollDeviceLoop();
        }
        private async Task PollDeviceLoop()
        {
            while (_isPolling)
            {
                await PollDeviceOnce();

                if (!_isPolling) break;

                int pollDelay = _mainWindow.GetPollDelay();

                await Task.Delay(pollDelay);
            }
        }

        /// <summary>
        /// Publiczna metoda do zatrzymywania pollingu, wywoływana z zewnątrz (np. przez MainWindow).
        /// Jest bezpieczna wątkowo.
        /// </summary>
        public void StopPolling()
        {
            // Ustaw flagę zatrzymania
            _isPolling = false;

            // Aktualizacja UI musi być w wątku UI
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    startToolStripMenuItem.Enabled = true;
                    stopToolStripMenuItem.Enabled = false;
                }));
            }
            else
            {
                startToolStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = false;
            }
        }

        // Ta funkcja jest podłączona do przycisku "Pause"
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Po prostu wywołuje nową, publiczną, bezpieczną wątkowo metodę
            this.StopPolling();
        }

        private void ShowDeviceError(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowDeviceError(message)));
                return;
            }
            lblDeviceStatus.Visible = true;
            lblDeviceStatus.Text = $"Device Error: {message}";
            lblDeviceStatus.ForeColor = Color.Red;
        }

        private void ClearDeviceError()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearDeviceError));
                return;
            }
            lblDeviceStatus.Visible = false;
            // --- ZAKTUALIZOWANY KOD: Lepszy komunikat o stanie ---
            lblDeviceStatus.Text = _isPolling ? "Polling OK" : "Polling Paused"; // Pokaż "Paused" zamiast pustego
            // --- KONIEC ZAKTUALIZOWANEGO KODU ---
            lblDeviceStatus.ForeColor = Color.Black; // Użyj czarnego dla "Paused"
        }


        // --- NOWY KOD: Metoda pomocnicza do formatowania błędów Modbus ---
        /// <summary>
        /// Formatuje wyjątek SlaveException na czytelny komunikat błędu.
        /// </summary>
        private string GetModbusErrorMessage(MP_modbus.MyModbusSlaveException ex)
        {
            string errorName;
            switch (ex.SlaveExceptionCode)
            {
                case 1: errorName = "Illegal Function"; break;
                case 2: errorName = "Illegal Data Address"; break;
                case 3: errorName = "Illegal Data Value"; break;
                case 4: errorName = "Slave Device Failure"; break;
                case 5: errorName = "Acknowledge"; break;
                case 6: errorName = "Slave Device Busy"; break;
                // Można dodać więcej kodów zgodnie ze specyfikacją Modbus
                case 7: errorName = "Negative Acknowledge"; break;
                case 8: errorName = "Memory Parity Error"; break;
                case 10: errorName = "Gateway Path Unavailable"; break; // 0x0A
                case 11: errorName = "Gateway Target Device Failed to Respond"; break; // 0x0B
                default: errorName = $"Unknown Exception ({ex.SlaveExceptionCode})"; break;
            }
            // Kod funkcji w wyjątku to oryginalny kod + 0x80 (128)
            byte originalFunctionCode = (byte)(ex.FunctionCode - 128);
            return $"Modbus Error (FC:{originalFunctionCode}, Code:{ex.SlaveExceptionCode}) - {errorName}";
        }
        // --- KONIEC NOWEGO KODU ---


        private async Task PollDeviceOnce()
        {
            if (!_isPolling || _modbusMaster == null) return;

            byte slaveId = (byte)this.SlaveId;

            foreach (TabPage tabPage in tabPanel1.TabPages)
            {
                // --- NOWY KOD: Sprawdzenie _isPolling przed przetworzeniem zakładki ---
                // Jeśli użytkownik kliknął "Pause" podczas przetwarzania poprzedniej zakładki,
                // nie chcemy kontynuować z kolejnymi.
                if (!_isPolling) break;
                // --- KONIEC NOWEGO KODU ---

                if (tabPage.Controls[0] is not ReadingsTab readingsTab) continue;

                int funcCode = 0;
                ushort startAddr = 0;
                ushort quantity = 0;
                try
                {
                    funcCode = readingsTab.GetFunctionCode();
                    startAddr = (ushort)readingsTab.GetStartAddress();
                    quantity = (ushort)readingsTab.GetQuantity();

                    ushort[] data;
                    switch (funcCode)
                    {
                        case 0: // 01 Coils
                            bool[] coils = await _modbusMaster.ReadCoilsAsync(slaveId, startAddr, quantity);
                            data = coils.Select(c => c ? (ushort)1 : (ushort)0).ToArray();
                            break;
                        case 1: // 02 Discrete Inputs
                            bool[] inputs = await _modbusMaster.ReadInputsAsync(slaveId, startAddr, quantity);
                            data = inputs.Select(i => i ? (ushort)1 : (ushort)0).ToArray();
                            break;
                        // --- ZAKTUALIZOWANY KOD: Poprawione mapowanie ComboBox na Function Code ---
                        // 03 Holding Registers (4x) to indeks 2 w ComboBox
                        // 04 Input Registers (3x) to indeks 3 w ComboBox
                        case 2: // 03 Holding Registers (Indeks ComboBox = 2)
                            data = await _modbusMaster.ReadHoldingRegistersAsync(slaveId, startAddr, quantity);
                            break;
                        case 3: // 04 Input Registers (Indeks ComboBox = 3)
                            data = await _modbusMaster.ReadInputRegistersAsync(slaveId, startAddr, quantity);
                            break;
                        // --- KONIEC ZAKTUALIZOWANEGO KODU ---
                        default:
                            // To nie powinno się zdarzyć z ComboBox, ale na wszelki wypadek
                            readingsTab.ShowTabError($"Unsupported Function Code Index: {funcCode}");
                            continue; // Przejdź do następnej zakładki
                    }

                    readingsTab.UpdateValues(data);

                    readingsTab.ClearTabError(); // Wyczyść błąd, jeśli odczyt się udał
                    ClearDeviceError(); // Wyczyść błąd na poziomie urządzenia
                }
                //catch (NModbus.SlaveException modbusEx) // Błąd Modbus (np. zły adres)
                catch (MP_modbus.MyModbusSlaveException modbusEx) // Nowy kod
                {
                    // --- ZAKTUALIZOWANY KOD: Użycie nowej funkcji formatującej ---
                    //string userFriendlyError = GetModbusErrorMessage(modbusEx);
                    //readingsTab.ShowTabError(userFriendlyError);
                    // Nie pokazujemy tego błędu na poziomie całego urządzenia (ShowDeviceError)
                    // ani nie zatrzymujemy pollingu, bo może dotyczyć tylko jednej zakładki.
                    // --- KONIEC ZAKTUALIZOWANEGO KODU ---
                    string userFriendlyError = GetModbusErrorMessage(modbusEx);
                    readingsTab.ShowTabError(userFriendlyError);

                    // Logowanie w NModbusLogger nadal działa i pokazuje szczegóły ramki HEX
                }
                catch (Exception ex) // Błąd Komunikacji (np. Timeout, rozłączenie)
                {
                    // Loguj błąd komunikacji (ważne, bo logger NModbus go nie widzi)
                    LogFrame("Error", $"Comms Error: {ex.GetType().Name} - {ex.Message}", ex.Message);

                    // Pokaż błąd w zakładce i na poziomie urządzenia
                    readingsTab.ShowTabError($"Comms Error: {ex.Message}");
                    ShowDeviceError(ex.Message); // Pokaż błąd na poziomie urządzenia

                    // Bezpiecznie zatrzymuje pętlę i aktualizuje UI
                    this.StopPolling();

                    break; // Przerwij pętlę po zakładkach, bo błąd dotyczy całego połączenia
                }
            } // Koniec pętli foreach po zakładkach

            // --- NOWY KOD: Jeśli pętla się zakończyła, a polling jest wciąż aktywny, wyczyść błąd urządzenia ---
            // To obsłuży przypadek, gdy ostatnia zakładka miała błąd SlaveException,
            // ale poprzednie odczyty były OK - nie chcemy wtedy widzieć błędu na poziomie urządzenia.
            if (_isPolling)
            {
                ClearDeviceError();
            }
            // --- KONIEC NOWEGO KODU ---
        }

        private void LogFrame(string direction, string dataFrame, string error = "")
        {
            var logEntry = new ModbusFrameLog
            {
                Timestamp = DateTime.Now,
                Direction = direction,
                DataFrame = dataFrame,
                ErrorDescription = error
            };
            _mainWindow?.LogCommunicationEvent(logEntry);
        }

        private void ModbusDevice_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.StopPolling();
        }
    }
}