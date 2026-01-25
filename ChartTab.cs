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
        // Dictionary to store point history for each data series
        private readonly Dictionary<string, List<ChartDataPoint>> _seriesData = new Dictionary<string, List<ChartDataPoint>>();
        private const int MaxPoints = 1024; // Maximum number of points in history

        public ChartTab()
        {
            InitializeComponent();
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            this.btnExportToCsv.Click += new System.EventHandler(this.btnExportToCsv_Click);

            // Hide by default
            this.Visible = false;

            // Chart configuration
            if (chart1.Series.Count > 0) chart1.Series.Clear();
            if (chart1.ChartAreas.Count == 0) chart1.ChartAreas.Add(new ChartArea("MainArea"));
            if (chart1.Legends.Count == 0) chart1.Legends.Add(new Legend("MainLegend"));

            // Legend configuration to appear below the chart
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
        /// Updates the chart based on the latest data points.
        /// </summary>
        public void UpdateChart(List<ChartDataPoint> latestData)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateChart(latestData)));
                return;
            }

            // 1. Visibility management
            this.Visible = latestData != null && latestData.Count > 0;
            if (!this.Visible)
            {
                // Keep _seriesData so historical data is preserved
                chart1.Series.Clear();
                chart1.Invalidate();
                return;
            }

            // 2. Save new points to history
            AddNewPointsToHistory(latestData);

            // 3. Update series on the chart
            chart1.Series.Clear();

            // Set of series names that are currently selected in ReadingsTab
            var activeSeriesNames = latestData.Select(d => d.SeriesName).ToHashSet();

            // Add/Refresh series from history
            // Filtering to activeSeriesNames ensures only currently selected registers appear on the chart,
            // while their history (_seriesData) remains intact.
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

                // Add all historical points to the series
                foreach (var dataPoint in dataHistory)
                {
                    series.Points.AddXY(dataPoint.Timestamp.ToOADate(), dataPoint.Value);
                }

                chart1.Series.Add(series);
            }

            // Set X-axis scale based on current data range
            if (chart1.Series.Count > 0)
            {
                var allTimestamps = _seriesData.Values.SelectMany(list => list.Select(p => p.Timestamp)).ToList();
                if (allTimestamps.Any())
                {
                    double minX = allTimestamps.Min().ToOADate();
                    double maxX = allTimestamps.Max().ToOADate();

                    chart1.ChartAreas[0].AxisX.Minimum = minX;
                    chart1.ChartAreas[0].AxisX.Maximum = maxX;

                    // Ensure Y scale adjusts automatically
                    chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
                    chart1.ChartAreas[0].AxisY.Minimum = double.NaN;
                    chart1.ChartAreas[0].AxisY.Maximum = double.NaN;
                }
            }

            chart1.Invalidate();
        }

        /// <summary>
        /// Adds new points to the internal history, maintaining the maximum point limit.
        /// </summary>
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
            // Call UpdateChart with empty list to clear display and reset visibility
            UpdateChart(new List<ChartDataPoint>());
            //MessageBox.Show("Chart data cleared successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        var sb = new StringBuilder();
                        var culture = System.Globalization.CultureInfo.InvariantCulture;
                        var consolidatedData = new List<ChartDataPoint>();
                        TimeSpan consolidationWindow = TimeSpan.FromMilliseconds(90);

                        // Step 1: Data consolidation (eliminates points within 90ms window, keeping the latest)
                        foreach (var kvp in _seriesData)
                        {
                            var sortedHistory = kvp.Value.OrderByDescending(p => p.Timestamp).ToList();
                            DateTime lastRetainedTimestamp = DateTime.MaxValue;

                            foreach (var point in sortedHistory)
                            {
                                if (lastRetainedTimestamp - point.Timestamp >= consolidationWindow || lastRetainedTimestamp == DateTime.MaxValue)
                                {
                                    consolidatedData.Add(point);
                                    lastRetainedTimestamp = point.Timestamp;
                                }
                            }
                        }

                        // Step 2: Pivoting and export
                        var allSeriesNames = _seriesData.Keys.OrderBy(k => k).ToList();

                        // Group consolidated data by Timestamp, descending order (newest first)
                        var groupedData = consolidatedData
                            .GroupBy(p => p.Timestamp)
                            .OrderByDescending(g => g.Key)
                            .ToDictionary(
                                g => g.Key,
                                g => g.ToDictionary(p => p.SeriesName, p => p.Value)
                            );

                        // CSV Header
                        sb.Append("Timestamp");
                        foreach (var seriesName in allSeriesNames)
                        {
                            string cleanName = seriesName.Split(new[] { " - " }, StringSplitOptions.None).Last().Trim();
                            sb.Append($",\"{cleanName}\"");
                        }
                        sb.AppendLine();

                        // Data rows
                        foreach (var timestampGroup in groupedData)
                        {
                            // Timestamp formatted as dd.MM.yyyy HH:mm:ss.fff
                            sb.Append($"\"{timestampGroup.Key:dd.MM.yyyy HH:mm:ss:f00}\"");

                            foreach (var seriesName in allSeriesNames)
                            {
                                if (timestampGroup.Value.TryGetValue(seriesName, out double value))
                                {
                                    sb.Append($",{value.ToString("F3", culture)}");
                                }
                                else
                                {
                                    sb.Append(",");
                                }
                            }
                            sb.AppendLine();
                        }

                        File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                        //MessageBox.Show("Export completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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