using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Runtime.InteropServices;

namespace SFiIR.App;

public partial class OlapChangesControl : UserControl
{
    List<double> xValues;
    List<double>[] yMetrics;
    public OlapChangesControl()
    {
        InitializeComponent();

        this.ActualThemeVariantChanged += OnThemeChanged;

        OnThemeChanged(this, EventArgs.Empty);
    }

    private void DropdownPopup_Closed(object? sender, System.EventArgs e)
    {
        DropdownToggle.IsChecked = false;
    }

    public void ClearPlot()
    {
        Chart.Plot.Clear();
        Chart.Refresh();
    }

    private void ApplyTheme()
    {
        var bg = GetHex("RegionColorBrush");
        var axisColor = GetHex("BaseMediumBrush");
        var textColor = GetHex("BaseHighBrush");
        var accent = GetHex("AccentBrush");

        Chart.Plot.FigureBackground.Color = ScottPlot.Color.FromHex(bg);
        Chart.Plot.DataBackground.Color = ScottPlot.Color.FromHex(bg);
        Chart.Plot.Legend.BackgroundColor = ScottPlot.Color.FromHex(bg);

        Chart.Plot.Axes.Color(ScottPlot.Color.FromHex(axisColor));
        Chart.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex(axisColor).WithAlpha(0.4f);
        Chart.Plot.Grid.MinorLineColor = ScottPlot.Color.FromHex(axisColor).WithAlpha(0.15f);

        Chart.Plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex(textColor);
        Chart.Plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex(textColor);
        Chart.Plot.Axes.Bottom.Label.ForeColor = ScottPlot.Color.FromHex(textColor);
        Chart.Plot.Axes.Left.Label.ForeColor = ScottPlot.Color.FromHex(textColor);
        Chart.Plot.Legend.FontColor = ScottPlot.Color.FromHex(textColor);

        Chart.Refresh();
    }
    private static string GetHex(string key)
    {
        if (Application.Current?.TryFindResource(key, out var obj) != true)
        {
            throw new Exception($"{key} is not found");
        }

        var brush = (SolidColorBrush)obj;
        var hex = $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";

        return hex;
    }

    private new void OnThemeChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(ApplyTheme);
    }
    public void Update(List<double> xValues, List<double>[] yMetrics)
    {
        this.xValues = xValues;
        this.yMetrics = yMetrics;
        if (xValues == null || yMetrics == null || yMetrics.Length != 6 || xValues.Count == 0)
            return;

        Chart.Plot.Clear();
        
        IPalette palette = new ScottPlot.Palettes.Category10();

        bool[] checkBoxes =
        {
            (CompletedRequests.IsChecked ?? false),
            (Workload.IsChecked ?? false),
            (AverageMemoryUsage.IsChecked ?? false),
            (AverageRequestCompletion.IsChecked ?? false),
            (AverageRequestQueueTime.IsChecked ?? false),
            (AverageProcessQueueTime.IsChecked ?? false)
        };

        var metricNames = new[]
        {
            checkBoxes[0] ? "Доля выполненных заявок" : null,
            checkBoxes[1] ? "Загруженность системы" : null,
            checkBoxes[2] ? "Использование памяти" : null,
            checkBoxes[3] ? "Среднее выполнение" : null,
            checkBoxes[4] ? "Ожидание обработки" : null,
            checkBoxes[5] ? "Ожидание в очереди" : null
        };

        for (int i = 0; i < 6; i++)
        {
            if (!checkBoxes[i]) continue;
            if (yMetrics[i] == null || yMetrics[i].Count != xValues.Count)
                continue;

            var scatter = Chart.Plot.Add.Scatter(
                xValues.ToArray(),
                yMetrics[i].ToArray());

            scatter.LegendText = metricNames[i];
            scatter.LineWidth = 2;
            scatter.Color = palette.GetColor(i + 1);
        }

        var refLine = Chart.Plot.Add.HorizontalLine(1.0);
        refLine.Color = ScottPlot.Colors.Gray.WithAlpha(0.3f);
        refLine.LineWidth = 1;
        refLine.LineStyle.Pattern = LinePattern.Dashed;

        Chart.Plot.ShowLegend();
        Chart.Plot.Legend.FontSize = 8;

        ApplyTheme();
        Chart.Plot.Axes.AutoScale();
        Chart.Refresh();
    }

    private void CheckBoxChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Update(xValues, yMetrics);
    }
}