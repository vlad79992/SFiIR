using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.TickGenerators;
using SFiIR.Modeling;
using SFiIR.Modeling.Statistics;
using System;
using System.Diagnostics;
using System.Linq;

namespace SFiIR.App.SimulationVisualization;

public partial class ChartControl : UserControl
{
    private readonly NumericAutomatic tickGenX = new NumericAutomatic()
    {
        LabelFormatter = (double x) => x % 2 == 0 ? (x / 2).ToString() : "",
        IntegerTicksOnly = true,
    };
    private readonly NumericAutomatic tickGenY = new NumericAutomatic()
    {
        IntegerTicksOnly = true,
        LabelFormatter = (double x) => x % 2 == 0 ? (x / 2).ToString() : "",
    };
    public ChartControl()
    {
        InitializeComponent();

        this.ActualThemeVariantChanged += OnThemeChanged;

        Chart.Plot.Axes.Left.TickGenerator = tickGenY;
        Chart.Plot.Axes.Bottom.TickGenerator = tickGenX;

        OnThemeChanged(this, EventArgs.Empty);
    }
    public void UpdateChart(AutomatedControlSystem acs)
    {
        var chart = Chart.Plot;
        chart.Clear();
        Chart.Plot.Axes.Left.TickGenerator = tickGenY;
        Chart.Plot.Axes.Bottom.TickGenerator = tickGenX;

        var palette = new ScottPlot.Palettes.Category20();

        foreach (var interval in acs.ProcessIntervals)
        {
            if (interval.start == interval.end) continue;

            var rect = chart.Add.Rectangle(left: interval.start * 2,
                                right: interval.end * 2,
                                bottom: interval.processId * 2 - 1,
                                top: (interval.processId + 1) * 2 - 1);
            rect.FillColor = palette.GetColor(interval.processId);
            rect.LineColor = ScottPlot.Colors.Black;

        }
        foreach (var interval in acs.RequestIntervals)
        {
            if (interval.start == interval.end) continue;

            ScottPlot.Plottables.ErrorBar eb = new(
                xs: [(interval.start * 2 + interval.end * 2) / 2],
                ys: [interval.processId * 2],
                xErrorsNegative: [(interval.end * 2 - interval.start * 2) / 2],
                xErrorsPositive: [(interval.end * 2 - interval.start * 2) / 2],
                yErrorsNegative: [0],
                yErrorsPositive: [0]);

            eb.LineWidth = 2;
            eb.Color = palette.GetColor(interval.processId);

            chart.Add.Plottable(eb);
        }
        chart.Axes.AutoScale();
        Chart.Refresh();
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

        Chart.Plot.Axes.Color(ScottPlot.Color.FromHex(axisColor));
        Chart.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex(axisColor).WithAlpha(0.4f);
        Chart.Plot.Grid.MinorLineColor = ScottPlot.Color.FromHex(axisColor).WithAlpha(0.15f);

        Chart.Plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex(textColor);
        Chart.Plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex(textColor);
        Chart.Plot.Axes.Bottom.Label.ForeColor = ScottPlot.Color.FromHex(textColor);
        Chart.Plot.Axes.Left.Label.ForeColor = ScottPlot.Color.FromHex(textColor);

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
}