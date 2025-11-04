using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

namespace MP_ModbusApp
{
    public partial class ChartTab : UserControl
    {
        // Słownik do przechowywania historii punktów dla każdej serii
        private readonly Dictionary<string, List<ChartDataPoint>> _seriesData = new Dictionary<string, List<ChartDataPoint>>();
        private const int MaxPoints = 100; // Maksymalna liczba punktów w historii

        public ChartTab()
        {
            InitializeComponent();

            // Domyślne ukrycie
            this.Visible = false;

            // Konfiguracja wykresu 
            if (chart1.Series.Count > 0) chart1.Series.Clear();
            if (chart1.ChartAreas.Count == 0) chart1.ChartAreas.Add(new ChartArea("MainArea"));
            if (chart1.Legends.Count == 0) chart1.Legends.Add(new Legend("MainLegend"));

            ChartArea chartArea = chart1.ChartAreas[0];
            chartArea.AxisX.Title = "Time";
            chartArea.AxisY.Title = "Value";
            chartArea.AxisX.LabelStyle.Format = "HH:mm:ss";
            chartArea.AxisX.IsStartedFromZero = false;

            chart1.Titles.Clear();
            chart1.Titles.Add("AActual modbus readings.");
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
                _seriesData.Clear();
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

            // 4. Wyczyść historię dla serii, które zostały odznaczone
            var keysToRemove = _seriesData.Keys.Where(k => !activeSeriesNames.Contains(k)).ToList();
            foreach (var key in keysToRemove)
            {
                _seriesData.Remove(key);
            }

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
    }
}