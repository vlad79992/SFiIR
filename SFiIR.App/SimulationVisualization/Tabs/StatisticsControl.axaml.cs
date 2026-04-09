using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using SFiIR.Modeling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;

namespace SFiIR.App.SimulationVisualization;

public partial class StatisticsControl : UserControl
{
    RadialGaugePlot? gauge = null;
    public StatisticsControl()
    {
        this.ActualThemeVariantChanged += OnThemeChanged;
        InitializeComponent();
        ClearStatistics();
        CardsList.Items.Add(new StatisticsCardControl());
    }
        
    public void UpdateStatistics(AutomatedControlSystem acs)
    {
        ClearStatistics();
        var processes = acs.AllProcesses;
        var requests = acs.AllRequests;
        UpdateAcsStatistics(acs, processes, requests);
        UpdateCards(acs, processes, requests);
    }

    private void UpdateAcsStatistics(AutomatedControlSystem acs, IEnumerable<Process> processes, IEnumerable<Request> requests)
    {
        double[] values = { 0, 0, 0, 0, 0, 0, 1 };
        var distinctEntries = acs.logEntries.DistinctBy(x => x.Time);
        var idleCount = acs.logEntries.Count(x => x.CurrentActivity == Modeling.Statistics.CurrentActivity.IDLE);
        
        double workload = ((double)(acs.testTime - idleCount) / (double)acs.testTime);
        WorkloadText.Text = workload.ToString("P1");
        values[1] = workload;
        try
        {
            double avgMemUsage = distinctEntries.Average(x => x.MemoryUsage);
            AverageMemoryUsageText.Text = avgMemUsage.ToString("F2");
            values[2] = avgMemUsage / acs.maxMem;
        }
        catch (Exception) { }
        int completedRequestsCount = processes.Count(p => p.ProcessInfo.Status == Modeling.Statistics.Status.EXITED);
        CompletedRequestsText.Text = completedRequestsCount.ToString();
        AllRequestsText.Text = requests.Count().ToString();
        values[0] = (double)completedRequestsCount / requests.Count();

        try
        {
            double avgRequestCompletion = processes.Select(static p => (double)(p.ProcessInfo.CpuTime - p.CpuTime) / (p.ProcessInfo.CpuTime))
                                                   .Concat(Enumerable.Repeat(0.0, requests.Count() - processes.Count()))
                                                   .Average();
            AverageRequestCompletionPercentageText.Text = avgRequestCompletion.ToString("P1");
            values[3] = avgRequestCompletion;
        }
        catch (Exception) { }

        try
        {
            double avgRequestQueueTime = requests.Where(r => r.RequestInfo.Status == Modeling.Statistics.Status.EXITED).Average(x => (int)x.RequestInfo.QueueTime);
            AverageRequestQueueTimeText.Text = avgRequestQueueTime.ToString("F2");
            values[4] = avgRequestQueueTime / acs.testTime;
        }
        catch (Exception) { }

        try
        {
            double avgProcessQueueTime = processes.Where(p => p.ProcessInfo.Status == Modeling.Statistics.Status.EXITED).Average(x => x.ProcessInfo.QueueTime);
            AverageProcessQueueTimeText.Text = avgProcessQueueTime.ToString("F2");
            values[5] = avgProcessQueueTime / acs.testTime;
        }
        catch (Exception) { }

        UpdateGauge(values);
    }

    private void UpdateCards(AutomatedControlSystem acs, IEnumerable<Process> processes, IEnumerable<Request> requests)
    {
        var processesDict = processes.ToDictionary(x => x.Id);
        var requestsDict = requests.ToDictionary(x => x.Id);

        Dictionary<int, StatisticData> data = new();

        foreach (var interval in acs.RequestIntervals)
        {
            int pid = interval.processId;

            if (!data.ContainsKey(pid)) data.Add(pid, new());

            data[pid].RequestArrivalTime = interval.start;
            data[pid].TimeInQueue = interval.end - interval.start;
            data[pid].RequiredCpuTime = requestsDict[pid].CpuTime;
            data[pid].RequiredMemory = requestsDict[pid].MemUsage;

            if (!processesDict.ContainsKey(pid)) continue;

            data[pid].ProcessArrivalTime = interval.end;
            data[pid].ProcessQueueTime = processesDict[pid].ProcessInfo.QueueTime;
            data[pid].ProcessExecutionTime = processesDict[pid].ProcessInfo.ExecutionTime;

        }

        foreach (var x in data) x.Value.Id = x.Key;


        foreach (var x in data.Values.OrderBy(x => x.Id))
        {
            CardsList.Items.Add(new StatisticsCardControl()
            {
                Id = x.Id,
                RequestArrivalTime = x.RequestArrivalTime,
                TimeInQueue = x.TimeInQueue,
                ProcessArrivalTime = x.ProcessArrivalTime,
                ProcessQueueTime = x.ProcessQueueTime,
                ProcessExecutionTime = x.ProcessExecutionTime,
                RequiredCpuTime = x.RequiredCpuTime,
                RequiredMemory = x.RequiredMemory,
            });
        }
    }
    private void UpdateGauge(double[] values)
    {
        if (values.Length != 7) throw new ArgumentException(nameof(values));

        StatisticsGauge.Reset();

        gauge = StatisticsGauge.Plot.Add.RadialGaugePlot(values);

        gauge.Colors[^1] = ScottPlot.Color.FromHex(GetHex("RegionColorBrush"));

        gauge.ShowLevels = false;
        gauge.Labels =
        [
            "Выполнено заявок",
            "Загруженность",
            "Ср. использование памяти",
            "Ср. выполнение заявок",
            "Ср. время ожидания обработки",
            "Ср. время ожидания в очереди выполнения",
            null
        ];
        gauge.OrderInsideOut = false;
        StatisticsGauge.Plot.ShowLegend();
        StatisticsGauge.Plot.Legend.FontSize = 8;
        ApplyTheme();
        StatisticsGauge.Refresh();
    }
    public void ClearStatistics()
    {
        CardsList.Items.Clear();

        UpdateGauge([0, 0, 0, 0, 0, 0, 1]);

        WorkloadText.Text = "null";
        AverageMemoryUsageText.Text = "null";
        CompletedRequestsText.Text = "null";
        AllRequestsText.Text = "null";
        AverageRequestCompletionPercentageText.Text = "null";
        AverageRequestQueueTimeText.Text = "null";
        AverageProcessQueueTimeText.Text = "null";

        StatisticsGauge.Refresh();
    }

    private class StatisticData
    {
        public int? Id { get; set; }
        public int? RequestArrivalTime { get; set; }
        public int? TimeInQueue { get; set; }
        public int? ProcessArrivalTime { get; set; }
        public int? ProcessQueueTime { get; set; }
        public int? ProcessExecutionTime { get; set; }
        public int? RequiredCpuTime { get; set; }
        public int? RequiredMemory { get; set; }
    }
    private void ApplyTheme()
    {
        var bg = GetHex("RegionColorBrush");
        var axisColor = GetHex("BaseMediumBrush");
        var textColor = GetHex("BaseHighBrush");
        var accent = GetHex("AccentBrush");

        StatisticsGauge.Plot.FigureBackground.Color = ScottPlot.Color.FromHex(bg);
        StatisticsGauge.Plot.DataBackground.Color = ScottPlot.Color.FromHex(bg);
        StatisticsGauge.Plot.Legend.BackgroundColor = ScottPlot.Color.FromHex(bg);
        gauge?.Colors[^1] = ScottPlot.Color.FromHex(bg);

        StatisticsGauge.Plot.Axes.Color(ScottPlot.Color.FromHex(axisColor));
        StatisticsGauge.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex(axisColor).WithAlpha(0.4f);
        StatisticsGauge.Plot.Grid.MinorLineColor = ScottPlot.Color.FromHex(axisColor).WithAlpha(0.15f);

        StatisticsGauge.Plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex(textColor);
        StatisticsGauge.Plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex(textColor);
        StatisticsGauge.Plot.Axes.Bottom.Label.ForeColor = ScottPlot.Color.FromHex(textColor);
        StatisticsGauge.Plot.Axes.Left.Label.ForeColor = ScottPlot.Color.FromHex(textColor);
        StatisticsGauge.Plot.Legend.FontColor = ScottPlot.Color.FromHex(textColor);

        var color = ScottPlot.Color.FromHex(GetHex("BaseHighBrush"));

        StatisticsGauge.Refresh();
    }
    private string GetHex(string key)
    {
        if (Application.Current?.TryFindResource(key, out var obj) != true)
        {
            throw new Exception($"{key} is not found");
        }

        var brush = (SolidColorBrush)obj;
        var hex = $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";

        return hex;
    }
    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(ApplyTheme);
    }
}