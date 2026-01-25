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
        private MP_modbus.IMyModbusMaster _modbusMaster;
        private bool _isPolling = false;
        private int _consecutiveErrorCount = 0;
        private int _rxCounter = 0;
        private int _txCounter = 0;
        private int _errorCounter = 0;

        public event EventHandler DeviceSaved;

        // --- NOWE POLA DLA WYKRESU ---
        private TabPage chartTabPage = null;
        private ChartTab chartControl = null;

        // POPRAWKA CS0123/CS0426: Użycie bezpośredniej nazwy klasy argumentów
        public void ReadingsTab_ChartDataUpdated(object sender, ChartDataUpdateEventArgs e)
        {
            UpdateChartDataFromAllTabs();
        }
        // --- KONIEC NOWYCH PÓL ---

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

            // Connect the "Pause" button event
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);

            // NOWE: Podpięcie zdarzenia do obsługi kliknięcia "Chart"
            this.chartToolStripMenuItem.Click += new System.EventHandler(this.chartToolStripMenuItem_Click);
        }

        /// <summary>
        /// Handles the form load event. Initializes tabs and starts polling.
        /// </summary>
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

                ReadingsTab readingsTab = new ReadingsTab() { Dock = DockStyle.Fill };
                readingsTab.ChartDataUpdated += ReadingsTab_ChartDataUpdated; // Podpięcie zdarzenia
                readingsTab.WriteValueRequested += ReadingsTab_WriteValueRequested; // NOWE: Podpięcie zdarzenia zapisu
                newTab.Controls.Add(readingsTab);

                tabPanel1.TabPages.Add(newTab);
            }

            // Usunięto odwołanie do txtRenameTab, ponieważ używamy teraz okna dialogowego

            // --- Podpięcie zdarzeń dla załadowanych zakładek (jeśli były ładowane z DB) ---
            foreach (TabPage page in tabPanel1.TabPages)
            {
                if (page.Controls.Count > 0 && page.Controls[0] is ReadingsTab tab)
                {
                    tab.ChartDataUpdated += ReadingsTab_ChartDataUpdated;
                    tab.WriteValueRequested += ReadingsTab_WriteValueRequested; // NOWE: Podpięcie zdarzenia zapisu
                }
            }
            // --------------------------------------------------------------------------------

            // Automatically start polling after a short delay
            await Task.Delay(100);
            startToolStripMenuItem_Click(sender, e);
        }

        /// <summary>
        /// Handles the WriteValueRequested event from any ReadingsTab and executes the Modbus write.
        /// Używa FC 05 (Write Single Coil) i FC 06/16 (Write Single/Multiple Registers).
        /// </summary>
        public async void ReadingsTab_WriteValueRequested(object sender, WriteRequestedEventArgs e)
        {
            if (_modbusMaster == null)
            {
                ShowDeviceError("Port nie jest podłączony! Nie można wykonać zapisu.");
                e.ReadingsTab.ShowTabError("Brak połączenia.");
                return;
            }

            // 1. Wstrzymaj polling, aby uniknąć kolizji
            bool wasPolling = _isPolling;
            StopPolling();

            // Ustaw nazwę urządzenia do logowania
            if (_modbusMaster.Transport is MP_modbus.ModbusTransportBase transport)
            {
                transport.LoggingDeviceName = this.DeviceName + " (" + this.slaveId.Value.ToString() + ")";
            }

            try
            {
                byte slaveId = (byte)this.SlaveId;
                ushort address = e.StartAddress;
                string rawValue = e.ValueString;
                int regsNeeded = e.ReadingsTab.GetRegistersForFormat(e.Format);

                if (e.FunctionCode == 0) // Coils (FC 05)
                {
                    bool valueToWrite = false;

                    // Logika konwersji wartości boolowskiej (dla formatu Bool16/default)
                    if (e.Format == ReadingsTab.DisplayFormat.Bool16)
                    {
                        if (!bool.TryParse(rawValue, out valueToWrite))
                        {
                            throw new ArgumentException($"Nieprawidłowa wartość logiczna dla formatu Bool16 (oczekiwano True/False): {rawValue}");
                        }
                    }
                    else // Domyślna konwersja (np. 0/1) dla Coil
                    {
                        if (rawValue.Trim() == "0") valueToWrite = false;
                        else if (rawValue.Trim() == "1") valueToWrite = true;
                        else if (int.TryParse(rawValue, out int intVal)) valueToWrite = (intVal != 0);
                        else throw new ArgumentException($"Nieprawidłowa wartość dla Coila (oczekiwano True/False/0/1): {rawValue}");
                    }

                    // Użycie FC 05 (Write Single Coil)
                    await _modbusMaster.WriteSingleCoilAsync(slaveId, address, valueToWrite);
                    LogFrame("TX_Write", $"Zapis Coil {address} = {valueToWrite} (FC 05)", "Write OK");
                }
                else if (e.FunctionCode == 2) // Holding Registers (FC 06/16)
                {
                    ushort[] valuesToWrite;

                    if (regsNeeded == 1) // 16-bit, używamy FC 06
                    {
                        ushort valueToWrite;

                        if (e.Format == ReadingsTab.DisplayFormat.Unsigned16)
                        {
                            if (!ushort.TryParse(rawValue, out valueToWrite))
                                throw new ArgumentException($"Nieprawidłowa wartość 16-bit Unsigned (oczekiwano 0-65535): {rawValue}");
                        }
                        else if (e.Format == ReadingsTab.DisplayFormat.Signed16)
                        {
                            if (!short.TryParse(rawValue, out short signedValue))
                                throw new ArgumentException($"Nieprawidłowa wartość 16-bit Signed: {rawValue}");
                            valueToWrite = (ushort)signedValue;
                        }
                        else if (e.Format == ReadingsTab.DisplayFormat.Hex16)
                        {
                            string cleanHex = rawValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? rawValue.Substring(2) : rawValue;
                            if (!ushort.TryParse(cleanHex, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out valueToWrite))
                                throw new ArgumentException($"Nieprawidłowa wartość Hex: {rawValue}");
                        }
                        else if (e.Format == ReadingsTab.DisplayFormat.ASCII)
                        {
                            if (rawValue.Length != 2) throw new ArgumentException($"Dla formatu ASCII 16-bit oczekiwano 2 znaków: {rawValue}");
                            byte byte1 = (byte)rawValue[0];
                            byte byte2 = (byte)rawValue[1];
                            valueToWrite = (ushort)((byte1 << 8) | byte2);
                        }
                        else
                        {
                            throw new NotSupportedException($"Wewnętrzny błąd logiki zapisu dla formatu: {e.Format}.");
                        }

                        // Użycie FC 06 (Write Single Register)
                        await _modbusMaster.WriteSingleRegisterAsync(slaveId, address, valueToWrite);
                        LogFrame("TX_Write", $"Zapis Rejestru {address} = {valueToWrite} (FC 06)", "Write OK");
                    }
                    else // 32-bit (2 rejestry) lub 64-bit (4 rejestry), używamy FC 16
                    {
                        string fmtStr = e.Format.ToString();

                        if (fmtStr.Contains("Unsigned") && regsNeeded == 2) // 32-bit Unsigned (UInt32)
                        {
                            if (!uint.TryParse(rawValue, out uint uIntValue)) throw new ArgumentException($"Nieprawidłowa wartość UInt32: {rawValue}");
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertValueToRegisters(uIntValue, e.Format);
                        }
                        else if (fmtStr.Contains("Signed") && regsNeeded == 2) // 32-bit Signed (Int32)
                        {
                            if (!int.TryParse(rawValue, out int intValue)) throw new ArgumentException($"Nieprawidłowa wartość Int32: {rawValue}");
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertValueToRegisters(intValue, e.Format);
                        }
                        else if (fmtStr.Contains("Float32")) // 32-bit Float (Real)
                        {
                            if (!float.TryParse(rawValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float floatValue))
                                throw new ArgumentException($"Nieprawidłowa wartość zmiennoprzecinkowa Float32: {rawValue}");
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertValueToRegisters(floatValue, e.Format);
                        }
                        else if (fmtStr.Contains("Unsigned") && regsNeeded == 4) // 64-bit Unsigned (ULong)
                        {
                            if (!ulong.TryParse(rawValue, out ulong ulongValue)) throw new ArgumentException($"Nieprawidłowa wartość ULong64: {rawValue}");
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertValueToRegisters(ulongValue, e.Format);
                        }
                        else if (fmtStr.Contains("Signed") && regsNeeded == 4) // 64-bit Signed (Long)
                        {
                            if (!long.TryParse(rawValue, out long longValue)) throw new ArgumentException($"Nieprawidłowa wartość Long64: {rawValue}");
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertValueToRegisters(longValue, e.Format);
                        }
                        else if (fmtStr.Contains("Double64") || fmtStr.Contains("Float64")) // 64-bit Double (Real)
                        {
                            if (!double.TryParse(rawValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double doubleValue))
                                throw new ArgumentException($"Nieprawidłowa wartość zmiennoprzecinkowa Double64: {rawValue}");
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertValueToRegisters(doubleValue, e.Format);
                        }
                        else if (fmtStr.Contains("ASCII")) // ASCII Multi-register
                        {
                            valuesToWrite = MP_modbus.ModbusUtils.ConvertAsciiToRegisters(rawValue, e.Format);
                        }
                        else
                        {
                            throw new NotSupportedException($"Zapis formatu {e.Format} ({regsNeeded * 16}-bit) nie jest obsługiwany przez obecną logikę konwersji.");
                        }

                        // Użycie FC 16 (Write Multiple Registers)
                        await _modbusMaster.WriteMultipleRegistersAsync(slaveId, address, valuesToWrite);
                        LogFrame("TX_Write", $"Zapis {regsNeeded * 16}-bit Reg {address} = {rawValue} ({e.Format.ToString()}, FC 16)", "Write OK");
                    }
                }

                e.ReadingsTab.ClearTabError();

                // 2. Wymuś natychmiastowy odczyt, aby odświeżyć wartość w siatce
                await PollDeviceOnce();
            }
            catch (MP_modbus.MyModbusSlaveException modbusEx)
            {
                string simpleError = MP_modbus.ModbusUtils.GetExceptionName(modbusEx.SlaveExceptionCode);
                e.ReadingsTab.ShowTabError($"Błąd Modbus: {simpleError}");
                LogFrame("Error", "", $"Zapis nieudany: {modbusEx.Message}");
                MessageBox.Show($"Błąd urządzenia Modbus podczas zapisu:\n{simpleError}", "Błąd zapisu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                e.ReadingsTab.ShowTabError($"Błąd zapisu: {ex.Message}");
                LogFrame("Error", "", $"Zapis nieudany: {ex.Message}");

                // ULEPSZENIE: Wyraźny komunikat dla użytkownika o błędzie zapisu
                MessageBox.Show($"Nie udało się zapisać wartości.\nSzczegóły: {ex.Message}", "Błąd zapisu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 3. Wznów polling, jeśli był wcześniej aktywny
                if (wasPolling)
                {
                    startToolStripMenuItem_Click(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Adds a new readings tab.
        /// </summary>
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage newTab = new TabPage();
            tabNo = tabNo + 1;
            newTab.Text = "Readings " + tabNo;

            ReadingsTab readingsTab = new ReadingsTab() { Dock = DockStyle.Fill };
            readingsTab.ChartDataUpdated += ReadingsTab_ChartDataUpdated; // Podpięcie zdarzenia
            readingsTab.WriteValueRequested += ReadingsTab_WriteValueRequested; // NOWE: Podpięcie zdarzenia zapisu

            newTab.Controls.Add(readingsTab);
            tabPanel1.TabPages.Add(newTab);
            tabPanel1.SelectedTab = newTab;
        }

        /// <summary>
        /// Implementacja otwierania zakładki z wykresem.
        /// </summary>
        private void chartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chartTabPage == null || chartTabPage.IsDisposed)
            {
                // 1. Utwórz i zainicjuj ChartTab (jeśli nie istnieje)
                chartControl = new ChartTab() { Dock = DockStyle.Fill };
                chartTabPage = new TabPage("Chart");
                chartTabPage.Controls.Add(chartControl);
                tabPanel1.TabPages.Add(chartTabPage);
            }

            // 2. Aktywuj zakładkę
            tabPanel1.SelectedTab = chartTabPage;

            // 3. Ręcznie wywołaj aktualizację danych wykresu po otwarciu, aby odświeżyć stan
            UpdateChartDataFromAllTabs();
        }

        /// <summary>
        /// Zbieranie danych ze wszystkich zakładek ReadingsTab i aktualizacja ChartTab.
        /// </summary>
        private void UpdateChartDataFromAllTabs()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateChartDataFromAllTabs));
                return;
            }

            if (chartControl == null || chartControl.IsDisposed)
            {
                return;
            }

            var allChartData = new List<ChartDataPoint>();

            // Iteracja przez zakładki w poszukiwaniu danych
            foreach (TabPage page in tabPanel1.TabPages)
            {
                // Sprawdź, czy kontrolka wewnątrz zakładki to ReadingsTab
                if (page.Controls.Count > 0 && page.Controls[0] is ReadingsTab readingsTab)
                {
                    allChartData.AddRange(readingsTab.GetChartData());
                }
            }

            // Wysłanie skumulowanych danych do kontrolki wykresu
            chartControl.UpdateChart(allChartData);
        }

        /// <summary>
        /// Removes the currently selected tab.
        /// </summary>
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabPanel1.SelectedTab != null)
            {
                tabPanel1.TabPages.Remove(tabPanel1.SelectedTab);
            }
        }

        /// <summary>
        /// Initiates the tab rename process using a standard dialog.
        /// </summary>
        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PerformTabRename();
        }

        /// <summary>
        /// Handles right-clicking on the tab control to show the context menu.
        /// </summary>
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

        /// <summary>
        /// Helper function to rename the currently selected tab using RenameForm.
        /// </summary>
        private void PerformTabRename()
        {
            tabToRename = tabPanel1.SelectedTab;
            if (tabToRename == null) return;

            using (RenameForm renameDialog = new RenameForm())
            {
                renameDialog.Text = "Zmień nazwę grupy";
                renameDialog.newName = tabToRename.Text;

                if (renameDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(renameDialog.newName))
                {
                    tabToRename.Text = renameDialog.newName;
                }
            }
            tabToRename = null;
        }

        private void ModbusDevice_Resize(object sender, EventArgs e)
        {
            // Brak potrzeby anulowania edycji, ponieważ używamy teraz okna modalnego
        }

        /// <summary>
        /// Handles the "New readings" menu item click (same as addToolStripMenuItem_Click).
        /// </summary>
        private void newReadingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage newTab = new TabPage();
            tabNo = tabNo + 1;
            tabPanel1.SelectedTab = newTab;
            newTab.Text = "Readings " + tabNo;
            ReadingsTab readingsTab = new ReadingsTab() { Dock = DockStyle.Fill };
            readingsTab.ChartDataUpdated += ReadingsTab_ChartDataUpdated;
            readingsTab.WriteValueRequested += ReadingsTab_WriteValueRequested; // NOWE: Podpięcie zdarzenia zapisu
            newTab.Controls.Add(readingsTab);
            tabPanel1.TabPages.Add(newTab);
        }

        /// <summary>
        /// Handles double-clicking on a tab to initiate rename.
        /// </summary>
        private void tabPanel1_DoubleClick(object sender, EventArgs e)
        {
            // ULEPSZENIE: Użycie okna dialogowego zamiast nakładanego TextBoxa
            PerformTabRename();
        }

        /// <summary>
        /// Handles the "Save" menu item click, prompting for a device name.
        /// </summary>
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
                    return; // User cancelled
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


        /// <summary>
        /// Saves the entire device configuration (device, groups, registers) to the database.
        /// This performs an "upsert" logic: updates if the name exists, inserts if it's new.
        /// </summary>
        private void SaveDeviceConfiguration()
        {
            using (var connection = new SqliteConnection($"Data Source={DatabaseHelper.GetDbPath()}"))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    long deviceId;

                    // Check if a device with this name already exists
                    var checkCmd = connection.CreateCommand();
                    checkCmd.Transaction = transaction;
                    checkCmd.CommandText = "SELECT DeviceId FROM Devices WHERE DeviceName = $name;";
                    checkCmd.Parameters.AddWithValue("$name", this.Text);

                    object existingIdResult = checkCmd.ExecuteScalar();

                    if (existingIdResult != null)
                    {
                        // --- UPDATE path ---
                        deviceId = (long)existingIdResult;

                        // Delete old groups and registers (they will be re-added)
                        var deleteGroupsCmd = connection.CreateCommand();
                        deleteGroupsCmd.Transaction = transaction;
                        deleteGroupsCmd.CommandText = "DELETE FROM ReadingGroups WHERE DeviceId = $deviceId;";
                        deleteGroupsCmd.Parameters.AddWithValue("$deviceId", deviceId);
                        deleteGroupsCmd.ExecuteNonQuery();

                        // Update the device's SlaveId
                        var updateDeviceCmd = connection.CreateCommand();
                        updateDeviceCmd.Transaction = transaction;
                        updateDeviceCmd.CommandText = "UPDATE Devices SET SlaveId = $slaveId WHERE DeviceId = $deviceId;";
                        updateDeviceCmd.Parameters.AddWithValue("$slaveId", (int)slaveId.Value);
                        updateDeviceCmd.Parameters.AddWithValue("$deviceId", deviceId);
                        updateDeviceCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // --- INSERT path ---
                        var insertDeviceCmd = connection.CreateCommand();
                        insertDeviceCmd.Transaction = transaction;
                        insertDeviceCmd.CommandText = "INSERT INTO Devices (DeviceName, SlaveId) VALUES ($name, $slaveId) RETURNING DeviceId;";
                        insertDeviceCmd.Parameters.AddWithValue("$name", this.Text);
                        insertDeviceCmd.Parameters.AddWithValue("$slaveId", (int)slaveId.Value);

                        deviceId = (long)insertDeviceCmd.ExecuteScalar();
                    }

                    // --- Save all tabs (ReadingGroups) and their registers ---
                    foreach (TabPage tabPage in tabPanel1.TabPages)
                    {
                        if (tabPage.Controls.Count > 0 && tabPage.Controls[0] is ReadingsTab readingsTab)
                        {
                            // Insert the ReadingGroup
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

                            // Insert all RegisterDefinitions for this group
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

        /// <summary>
        /// Handles the "Start" polling menu item click.
        /// </summary>
        private async void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _modbusMaster = _mainWindow?.ModbusMaster;
            if (_modbusMaster == null)
            {
                ShowDeviceError("Port is not connected! Connect via Setup tab!");
                return;
            }

            if (_isPolling) return;
            _isPolling = true;

            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;

            await PollDeviceLoop();
        }

        /// <summary>
        /// The main polling loop. Continues as long as _isPolling is true.
        /// </summary>
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
        /// Public, thread-safe method to stop polling.
        /// Can be called from other threads (e.g., MainWindow).
        /// </summary>
        public void StopPolling()
        {
            // Set the flag to stop the loop
            _isPolling = false;

            // Update UI controls, ensuring it's on the UI thread
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

        /// <summary>
        /// Handles the "Pause" button click.
        /// </summary>
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // It just calls the new, public, thread-safe method
            this.StopPolling();
        }

        /// <summary>
        /// Displays a device-level error message. Thread-safe.
        /// </summary>
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

        /// <summary>
        /// Clears the device-level error message. Thread-safe.
        /// </summary>
        private void ClearDeviceError()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearDeviceError));
                return;
            }
            lblDeviceStatus.Visible = true;
            lblDeviceStatus.Text = _isPolling ? " " : "Polling Paused"; // Show "Paused" instead of empty
            lblDeviceStatus.ForeColor = _isPolling ? Color.Green : Color.Black; // Green for OK, Black for Paused
        }

        /// <summary>
        /// Performs a single polling cycle, iterating through all tabs.
        /// </summary>
        private async Task PollDeviceOnce()
        {
            if (!_isPolling || _modbusMaster == null) return;

            // OPTYMALIZACJA: Jeśli okno jest zminimalizowane, nie odświeżaj UI
            if (this.WindowState == FormWindowState.Minimized)
            {
                return;
            }

            // Set the custom device name in the transport layer *before* polling.
            // This ensures all TX/RX logs generated during this poll cycle
            // use this device's friendly name (this.DeviceName).
            if (_modbusMaster.Transport is MP_modbus.ModbusTransportBase transport)
            {
                transport.LoggingDeviceName = this.DeviceName + " (" + this.slaveId.Value.ToString() + ")";
            }

            byte slaveId = (byte)this.SlaveId;
            int maxRetries = 0;

            if (_mainWindow != null)
            {
                maxRetries = _mainWindow.GetMaxRetries();
            }

            foreach (TabPage tabPage in tabPanel1.TabPages)
            {
                // If the user clicked "Pause" while processing the previous tab,
                // we don't want to continue with the next ones.
                if (!_isPolling) break;

                if (tabPage.Controls.Count == 0 || tabPage.Controls[0] is not ReadingsTab readingsTab) continue;

                // Pomiń zakładkę wykresu, jeśli jest otwarta
                if (tabPage == chartTabPage) continue;


                int funcCode = 0;
                ushort startAddr = 0;
                ushort quantity = 0;
                try
                {
                    funcCode = readingsTab.GetFunctionCode();
                    startAddr = (ushort)readingsTab.GetStartAddress();
                    quantity = (ushort)readingsTab.GetQuantity();
                    _txCounter++;
                    ushort[] data;
                    switch (funcCode)
                    {
                        case 0: // 01 Coils
                            bool[] coils = await _modbusMaster.ReadCoilsAsync(slaveId, startAddr, quantity);
                            data = coils.Select(c => c ? (ushort)1 : (ushort)0).ToArray();
                            _rxCounter++;
                            break;
                        case 1: // 02 Discrete Inputs
                            bool[] inputs = await _modbusMaster.ReadInputsAsync(slaveId, startAddr, quantity);
                            data = inputs.Select(i => i ? (ushort)1 : (ushort)0).ToArray();
                            _rxCounter++;
                            break;
                        case 2: // 03 Holding Registers
                            data = await _modbusMaster.ReadHoldingRegistersAsync(slaveId, startAddr, quantity);
                            _rxCounter++;
                            break;
                        case 3: // 04 Input Registers
                            data = await _modbusMaster.ReadInputRegistersAsync(slaveId, startAddr, quantity);
                            _rxCounter++;
                            break;
                        default:
                            // This shouldn't happen with a ComboBox, but just in case
                            readingsTab.ShowTabError($"Unsupported Function Code Index: {funcCode}");
                            continue; // Continue to the next tab
                    }

                    readingsTab.UpdateValues(data);
                    readingsTab.ClearTabError(); // Clear the error if the read was successful
                    _consecutiveErrorCount = 0;
                }
                catch (MP_modbus.MyModbusSlaveException modbusEx) // Modbus Error (e.g., bad address)
                {
                    // Use ModbusUtils to get the required error format
                    string fullError = MP_modbus.ModbusUtils.GetFullExceptionMessage(modbusEx.FunctionCode, modbusEx.SlaveExceptionCode);
                    string simpleError = MP_modbus.ModbusUtils.GetExceptionName(modbusEx.SlaveExceptionCode);
                    _errorCounter++;
                    // Display the simple error in the device tab
                    readingsTab.ShowTabError(simpleError);
                    readingsTab.ClearDisplayValues();
                    // Log the full error in the communication window
                    LogFrame("Error", "", fullError);

                    // We don't show this error at the device level (ShowDeviceError)
                    // nor do we stop polling, as it might only affect one tab.
                }
                catch (Exception ex) // Communication Error (e.g., Timeout, disconnection)
                {
                    _errorCounter++;
                    _consecutiveErrorCount++;
                    string commsError = $"Comms Error: {ex.Message}";

                    // Log the communication error
                    LogFrame("Error", "", commsError); // Clean error in the ErrorDescription column

                    // Show the error in the tab and at the device level
                    readingsTab.ShowTabError(commsError + _consecutiveErrorCount.ToString());
                    if (_consecutiveErrorCount >= maxRetries && maxRetries > 0)
                    {
                        string deviceError = $"Comms Error after {_consecutiveErrorCount} attempts: {ex.Message}. Polling stopped.";
                        ShowDeviceError(deviceError + _consecutiveErrorCount.ToString());
                        this.StopPolling();
                    }
                    else
                    {
                        if (maxRetries > 0)
                        {
                            ShowDeviceError($"{commsError} (Attempt {_consecutiveErrorCount}/{maxRetries})");
                        }
                        else
                        {
                            ShowDeviceError($"{commsError} (Attempt {_consecutiveErrorCount}/inf)");
                        }
                    }
                    break; // Break the tab loop because the error affects the entire connection
                }
                label1.Text = $"TX: {_txCounter}  RX: {_rxCounter}  ERR: {_errorCounter}";
            } // End of foreach loop for tabs

            // If the loop finished and polling is still active, clear the device error.
            // This handles the case where the last tab had a SlaveException,
            // but previous reads were OK - we don't want to see a device-level error then.
            if (_isPolling)
            {
                ClearDeviceError();
            }
        }

        /// <summary>
        /// Logs a frame to the main communication window.
        /// </summary>
        private void LogFrame(string direction, string dataFrame, string error = "")
        {
            var logEntry = new ModbusFrameLog
            {
                Timestamp = DateTime.Now,
                DeviceName = this.DeviceName + " (" + slaveId.Value.ToString() + ")", // Add device name to the log
                Direction = direction,
                DataFrame = dataFrame,
                ErrorDescription = error
            };
            _mainWindow?.LogCommunicationEvent(logEntry);
        }

        public void StartPolling()
        {


            ///startToolStripMenuItem_Click
            ///

            if (startToolStripMenuItem.Enabled)
            {
                startToolStripMenuItem.PerformClick();
            }

            //// Zakładając, że masz timer o nazwie np. pollTimer lub timerRead
            //if (pollTimer != null && !pollTimer.Enabled)
            //{
            //    pollTimer.Start();
            //}
        }


        /// <summary>
        /// Ensures polling is stopped when the form is closed.
        /// </summary>
        private void ModbusDevice_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.StopPolling();
        }

        private void ModbusDevice_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.StopPolling();
        }
    }
}