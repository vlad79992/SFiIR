using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace SFiIR.App.SimulationVisualization;

public partial class DistributionsControl : UserControl
{
    private Scatter? memScatter;
    private Scatter? arrivalScatter;
    private Scatter? cpuScatter;
    public DistributionsControl()
    {
        InitializeComponent();
        this.ActualThemeVariantChanged += OnThemeChanged;
        ArrivalLambdaPlot.UserInputProcessor.RemoveAll<ScottPlot.Interactivity.UserActionResponses.MouseWheelZoom>();
        MemLambdaPlot.UserInputProcessor.RemoveAll<ScottPlot.Interactivity.UserActionResponses.MouseWheelZoom>();
        CpuLambdaPlot.UserInputProcessor.RemoveAll<ScottPlot.Interactivity.UserActionResponses.MouseWheelZoom>();
        OnThemeChanged(this, EventArgs.Empty);
    }
    public void UpdateArrivalPlot(double arrivalLambda)
    {
        var color = ScottPlot.Color.FromHex(GetHex("BaseHighBrush"));
        ArrivalLambdaPlot.Plot.Clear();
        var data = GetExponentialPoints(arrivalLambda);
        arrivalScatter = ArrivalLambdaPlot.Plot.Add.ScatterLine(data.xs, data.ys, color);
        arrivalScatter.LineWidth = 2;
        ArrivalLambdaPlot.Plot.Axes.AutoScale();
        ArrivalLambdaPlot.Refresh();
    }
    public void ClearArrivalPlot()
    {
        arrivalScatter = null;
        ArrivalLambdaPlot.Reset();
        ApplyTheme();
        ArrivalLambdaPlot.Refresh(); ;
    }
    public void UpdateMemPlot(double memLambda)
    {
        var color = ScottPlot.Color.FromHex(GetHex("BaseHighBrush"));
        ClearMemPlot();
        var data = GetExponentialPoints(memLambda);
        memScatter = MemLambdaPlot.Plot.Add.ScatterLine(data.xs, data.ys, color);
        memScatter.LineWidth = 2;
        MemLambdaPlot.Plot.Axes.AutoScale();
        MemLambdaPlot.Refresh();
    }
    public void ClearMemPlot()
    {
        memScatter = null;
        MemLambdaPlot.Reset();
        ApplyTheme();
        MemLambdaPlot.Refresh();
    }
    public void UpdateCpuPlot(int cpuLambda)
    {
        var color = ScottPlot.Color.FromHex(GetHex("BaseHighBrush"));
        ClearCpuPlot();

        var data = GetPoissonPoints(cpuLambda);
        cpuScatter = CpuLambdaPlot.Plot.Add.Scatter(data.xs, data.ys,color);
        cpuScatter.LineWidth = 2;
        CpuLambdaPlot.Plot.Axes.AutoScaleY();
        CpuLambdaPlot.Plot.Axes.SetLimitsX(Math.Max(0, cpuLambda - 6 * Math.Sqrt(cpuLambda)), cpuLambda + 6 * Math.Sqrt(cpuLambda));
        CpuLambdaPlot.Refresh();
    }
    public void ClearCpuPlot()
    {
        cpuScatter = null;
        CpuLambdaPlot.Reset();
        ApplyTheme();
        CpuLambdaPlot.Refresh();
    }
    private static (double[] xs, double[] ys) GetPoissonPoints(double lambda)
    {
        if (lambda > 100) 
            return GetPoissonPointsFast(lambda);

        int maxK = (int)Math.Ceiling(lambda + 6 * Math.Sqrt(lambda));
        var xs = new double[maxK + 1];
        var ys = new double[maxK + 1];

        double logPk = -lambda;

        for (int k = 0; k <= maxK; k++)
        {
            xs[k] = k;
            ys[k] = Math.Exp(logPk);
            if (k < maxK) logPk += Math.Log(lambda) - Math.Log(k + 1);
        }

        return (xs, ys);
    }
    private static (double[] xs, double[] ys) GetExponentialPoints(double lambda)
    {
        int points = 1000;
        double xMax = 5.0 / lambda;
        var xs = new double[points];
        var ys = new double[points];

        for (int i = 0; i < points; i++)
        {
            xs[i] = xMax * i / (points - 1);
            ys[i] = lambda * Math.Exp(-lambda * xs[i]);
        }
        return (xs, ys);
    }
    private static (double[] xs, double[] ys) GetPoissonPointsFast(double lambda)
    {
        int halfWidth = (int)Math.Ceiling(6 * Math.Sqrt(lambda));
        int kMode = (int)Math.Floor(lambda);
        int kStart = Math.Max(0, kMode - halfWidth);
        int kEnd = (int)Math.Ceiling(lambda) + halfWidth;
        int count = kEnd - kStart + 1;

        var xs = new double[count];
        var ys = new double[count];

        double invLambda = 1.0 / lambda;
        double logPMode = -0.5 * Math.Log(2.0 * Math.PI * lambda)
                          - invLambda / 12.0
                          + (invLambda * invLambda * invLambda) / 360.0;

        double peakP = Math.Exp(logPMode);
        int modeIndex = kMode - kStart;

        xs[modeIndex] = kMode;
        ys[modeIndex] = peakP;

        double currentP = peakP;
        for (int k = kMode; k < kEnd; k++)
        {
            currentP *= lambda / (k + 1);
            xs[k + 1 - kStart] = k + 1;
            ys[k + 1 - kStart] = currentP;
        }

        currentP = peakP;
        for (int k = kMode; k > kStart; k--)
        {
            currentP *= k / lambda;
            xs[k - 1 - kStart] = k - 1;
            ys[k - 1 - kStart] = currentP;
        }

        return (xs, ys);
    }
    private void ApplyTheme()
    {
        foreach (var plot in new AvaPlot[] { ArrivalLambdaPlot, MemLambdaPlot, CpuLambdaPlot})
        {
            var bg = GetHex("RegionColorBrush");
            var axisColor = GetHex("BaseMediumBrush");
            var textColor = GetHex("BaseHighBrush");
            var accent = GetHex("AccentBrush");

            plot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex(bg);
            plot.Plot.DataBackground.Color = ScottPlot.Color.FromHex(bg);

            plot.Plot.Axes.Color(ScottPlot.Color.FromHex(axisColor));
            plot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex(axisColor).WithAlpha(0.4f);
            plot.Plot.Grid.MinorLineColor = ScottPlot.Color.FromHex(axisColor).WithAlpha(0.15f);

            plot.Plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex(textColor);
            plot.Plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex(textColor);
            plot.Plot.Axes.Bottom.Label.ForeColor = ScottPlot.Color.FromHex(textColor);
            plot.Plot.Axes.Left.Label.ForeColor = ScottPlot.Color.FromHex(textColor);
        }
        var color = ScottPlot.Color.FromHex(GetHex("BaseHighBrush"));

        memScatter?.Color = color;
        arrivalScatter?.Color = color;
        cpuScatter?.Color = color;

        MemLambdaPlot.Refresh();
        ArrivalLambdaPlot.Refresh();
        CpuLambdaPlot.Refresh();

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