using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using System.IO;
using System.Text;

namespace MP_ModbusApp
{
    public partial class ChartTab : UserControl
    {
        // Słownik do przechowywania historii punktów dla każdej serii
        private readonly Dictionary<string, List<ChartDataPoint>> _seriesData = new Dictionary<string, List<ChartDataPoint>>();
        private const int MaxPoints = 1024; // Maksymalna liczba punktów w historii

        public ChartTab()
        {
            InitializeComponent();
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            this.btnExportToCsv.Click += new System.EventHandler(this.btnExportToCsv_Click);

            // Domyślne ukrycie
            this.Visible = false;

            // Konfiguracja wykresu 
            if (chart1.Series.Count > 0) chart1.Series.Clear();
            if (chart1.ChartAreas.Count == 0) chart1.ChartAreas.Add(new ChartArea("MainArea"));
            if (chart1.Legends.Count == 0) chart1.Legends.Add(new Legend("MainLegend"));

            // Konfiguracja legendy, aby pojawiała się pod wykresem
            Legend legend = chart1.Legends.FirstOrDefault();
            if (legend != null)
            {
                legend.Docking = Docking.Bottom;
                legend.Alignment = StringAlignment.Center;
                legend.IsDockedInsideChartArea = false;
            }

            ChartArea chartArea = chart1.ChartAreas[0];
            chartArea.AxisX.Title = "Time";
            chartArea.AxisY.Title = "Value";
            chartArea.AxisX.LabelStyle.Format = "HH:mm:ss";
            chartArea.AxisX.IsStartedFromZero = false;

            chart1.Titles.Clear();
            chart1.Titles.Add("Actual modbus readings.");
        }

        /// <summary>
        /// Aktualizuje wykres na podstawie najnowszych danych.
        /// </summary>
        public void UpdateChart(List<ChartDataPoint> latestData)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateChart(latestData)));
                return;
            }

            // 1. Zarządzanie widocznością
            this.Visible = latestData != null && latestData.Count > 0;
            if (!this.Visible)
            {
                // Pozostawiamy _seriesData, aby dane historyczne były zachowane
                chart1.Series.Clear();
                chart1.Invalidate();
                return;
            }

            // 2. Zapisz nowe punkty do historii
            AddNewPointsToHistory(latestData);

            // 3. Aktualizacja serii na wykresie
            chart1.Series.Clear();

            // Zbiór nazw serii, które są aktualnie zaznaczone w ReadingsTab
            var activeSeriesNames = latestData.Select(d => d.SeriesName).ToHashSet();

            // Dodaj/odśwież serię z historii
            // Filtrowanie tylko do activeSeriesNames zapewnia, że na wykresie pojawiają się tylko
            // aktualnie zaznaczone rejestry, ale ich historia (_seriesData) pozostaje nienaruszona.
            foreach (var kvp in _seriesData.Where(k => activeSeriesNames.Contains(k.Key)))
            {
                string seriesName = kvp.Key;
                List<ChartDataPoint> dataHistory = kvp.Value;

                Series series = new Series(seriesName)
                {
                    ChartType = SeriesChartType.Line,
                    XValueType = ChartValueType.DateTime,
                    IsVisibleInLegend = true,
                    BorderWidth = 2,
                    MarkerStyle = MarkerStyle.Circle,
                    MarkerSize = 5
                };

                // Dodaj całą historię punktów do serii
                foreach (var dataPoint in dataHistory)
                {
                    series.Points.AddXY(dataPoint.Timestamp.ToOADate(), dataPoint.Value);
                }

                chart1.Series.Add(series);
            }

            // Usunięto logikę usuwania nieaktywnych serii z _seriesData, aby zachować historię.

            // Ustaw skalę osi X na podstawie aktualnej daty/czasu (ostatniego i pierwszego punktu)
            if (chart1.Series.Count > 0)
            {
                var allTimestamps = _seriesData.Values.SelectMany(list => list.Select(p => p.Timestamp)).ToList();
                if (allTimestamps.Any())
                {
                    double minX = allTimestamps.Min().ToOADate();
                    double maxX = allTimestamps.Max().ToOADate();

                    // Ustaw minimalny i maksymalny zakres osi X
                    chart1.ChartAreas[0].AxisX.Minimum = minX;
                    chart1.ChartAreas[0].AxisX.Maximum = maxX;

                    // Upewnij się, że skala Y dostosowuje się automatycznie
                    chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
                    chart1.ChartAreas[0].AxisY.Minimum = double.NaN;
                    chart1.ChartAreas[0].AxisY.Maximum = double.NaN;
                }
            }


            chart1.Invalidate();
        }

        private void AddNewPointsToHistory(List<ChartDataPoint> newPoints)
        {
            foreach (var newPoint in newPoints)
            {
                if (!_seriesData.ContainsKey(newPoint.SeriesName))
                {
                    _seriesData[newPoint.SeriesName] = new List<ChartDataPoint>();
                }

                var history = _seriesData[newPoint.SeriesName];
                history.Add(newPoint);

                if (history.Count > MaxPoints)
                {
                    history.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Handles the Clear button click. Clears all historical data from the chart.
        /// </summary>
        private void btnClearLog_Click(object sender, EventArgs e)
        {
            _seriesData.Clear();
            // Wywołaj UpdateChart z pustą listą, aby wyczyścić wyświetlacz i zresetować widoczność
            UpdateChart(new List<ChartDataPoint>());
            MessageBox.Show("Chart data cleared successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles the Export to CSV button click. Exports all historical data to CSV in pivot format.
        /// </summary>
        private void btnExportToCsv_Click(object sender, EventArgs e)
        {
            if (_seriesData.Count == 0 || _seriesData.Values.All(list => list.Count == 0))
            {
                MessageBox.Show("No data to export.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV File (*.csv)|*.csv";
                saveFileDialog.FileName = $"ModbusChartData_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new System.Text.StringBuilder();
                        var culture = System.Globalization.CultureInfo.InvariantCulture;
                        var consolidatedData = new List<ChartDataPoint>();
                        TimeSpan consolidationWindow = TimeSpan.FromMilliseconds(90);

                        // --- Krok 1: Konsolidacja danych (eliminuje punkty w oknie 90ms, zachowując najnowszy) ---
                        foreach (var kvp in _seriesData)
                        {
                            // Sortujemy malejąco, aby móc łatwo utrzymać latestRetainedTime
                            var sortedHistory = kvp.Value.OrderByDescending(p => p.Timestamp).ToList();
                            DateTime lastRetainedTimestamp = DateTime.MaxValue;

                            foreach (var point in sortedHistory)
                            {
                                // Jeżeli punkt jest poza oknem konsolidacji (stary) LUB jest to pierwszy (najnowszy) punkt w serii
                                if (lastRetainedTimestamp - point.Timestamp >= consolidationWindow || lastRetainedTimestamp == DateTime.MaxValue)
                                {
                                    consolidatedData.Add(point);
                                    lastRetainedTimestamp = point.Timestamp;
                                }
                                // Punkty, których czas jest za blisko nowszego, już zachowanego punktu, są odrzucane.
                            }
                        }

                        // --- Krok 2: Pivotowanie i eksport ---

                        // Zbieranie wszystkich unikalnych nazw serii
                        var allSeriesNames = _seriesData.Keys.OrderBy(k => k).ToList();

                        // Grupujemy skonsolidowane dane według Timestamp. Utrzymujemy sortowanie malejące.
                        var groupedData = consolidatedData
                            .GroupBy(p => p.Timestamp)
                            .OrderByDescending(g => g.Key) // Sortowanie malejące - najnowsze na górze (Żądanie 2)
                            .ToDictionary(
                                g => g.Key,
                                g => g.ToDictionary(p => p.SeriesName, p => p.Value)
                            );

                        // 2a. CSV Header: Timestamp, Rejestr_a, Rejestr_b, ...
                        sb.Append("Timestamp");
                        foreach (var seriesName in allSeriesNames)
                        {
                            // Używamy tylko nazwy rejestru jako nagłówka kolumny
                            string cleanName = seriesName.Split(new[] { " - " }, StringSplitOptions.None).Last().Trim();
                            sb.Append($",\"{cleanName}\"");
                        }
                        sb.AppendLine();

                        // 2b. Wiersze danych
                        foreach (var timestampGroup in groupedData)
                        {
                            // A. Timestamp - Formatuje jako dd.MM.yyyy HH:mm:ss.fff (zawsze z milisekundami - Żądanie 3)
                            sb.Append($"\"{timestampGroup.Key:dd.MM.yyyy HH:mm:ss:f00}\"");

                            // B. Wartości dla każdej serii w porządku nagłówków
                            foreach (var seriesName in allSeriesNames)
                            {
                                // Pobierz wartość dla tej serii i tego znacznika czasowego
                                if (timestampGroup.Value.TryGetValue(seriesName, out double value))
                                {
                                    // Dodaj wartość sformatowaną jako string
                                    sb.Append($",{value.ToString("F3", culture)}");
                                }
                                else
                                {
                                    // Jeśli brakuje wartości dla tej serii w tym czasie, wstaw pustą komórkę
                                    sb.Append(",");
                                }
                            }
                            sb.AppendLine();
                        }

                        File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Export completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error during export: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}