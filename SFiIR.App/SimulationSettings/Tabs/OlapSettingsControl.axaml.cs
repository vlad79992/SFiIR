using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScottPlot.TickGenerators.TimeUnits;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading.Channels;

namespace SFiIR.App.SimulationSettings;

public partial class OlapSettingsControl : UserControl
{
    private static readonly double[] ArrivalLambdas = { 0.1, 0.25, 0.5, 0.75, 1, 1.5, 2, 3, 5, 7.5, 10, 15, 20, 25 };
    private static readonly double[] MemLambdas = { 0.01, 0.025, 0.05, 0.075, 0.1, 0.15, 0.2, 0.3, 0.5, 0.75, 1.0 };
    private static readonly int[] CpuLambdas = { 1, 3, 5, 7, 10, 15, 20, 25, 30, 40, 50 };
    private static readonly int[] ArrivalTimeLimits = { 100, 250, 500, 1000, 2000 };
    private static readonly int[] TestTimes = { 200, 500, 1000, 2000 };
    private static readonly int[] MaxMems = { 25, 50, 75, 100, 125, 150, 175, 200, 225, 250 };
    private static readonly int[] CpuQuants = { 1, 3, 5, 7, 10, 12, 15, 18, 20, 25 };

    public event EventHandler? Changed;
    private readonly Dictionary<int, (TextBlock label, ComboBox input)> parameterControls;
    public OlapSettingsControl()
    {
        InitializeComponent();

        parameterControls = new()
        {
            { 0, (ArrivalLambdaText, ArrivalLambdaCombo) },
            { 1, (MemLambdaText, MemLambdaCombo) },
            { 2, (CpuLambdaText, CpuLambdaCombo) },
            { 3, (ArrivalTimeText, ArrivalTimeCombo) },
            { 4, (TestTimeText, TestTimeCombo) },
            { 5, (MaxMemText, MaxMemCombo) },
            { 6, (CpuQuantText, CpuQuantCombo) },
        };
    }
    private void SelectionChanged(object? sender, SelectionChangedEventArgs e) => Changed?.Invoke(sender, e);
    private void ParameterCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedIndex = ParameterCombo.SelectedIndex;
        if (selectedIndex == -1)
        {
            SetAllParametersVisible(true);
            return;
        }

        foreach (var (idx, controls) in parameterControls)
        {
            bool isVisible = idx != selectedIndex;
            controls.label.IsVisible = isVisible;
            controls.input.IsVisible = isVisible;
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }
    private void SetAllParametersVisible(bool visible)
    {
        foreach (var (_, controls) in parameterControls)
        {
            controls.label.IsVisible = visible;
            controls.input.IsVisible = visible;
        }
    }

    public double? ArrivalLambda => ArrivalLambdaCombo.SelectedIndex is int s && s >= 0 ? ArrivalLambdas[s] : null;
    public double? MemLambda => MemLambdaCombo.SelectedIndex is int s && s >= 0 ? MemLambdas[s] : null;
    public int? CpuLambda => CpuLambdaCombo.SelectedIndex is int s && s >= 0 ? CpuLambdas[s] : null;
    public int? ArrivalTime => ArrivalTimeCombo.SelectedIndex is int s && s >= 0 ? ArrivalTimeLimits[s] : null;
    public int? TestTime => TestTimeCombo.SelectedIndex is int s && s >= 0 ? TestTimes[s] : null;
    public int? MaxMem => MaxMemCombo.SelectedIndex is int s && s >= 0 ? MaxMems[s] : null;
    public int? CpuQuant => CpuQuantCombo.SelectedIndex is int s && s >= 0 ? CpuQuants[s] : null;
    public int SelectedParameter => ParameterCombo.SelectedIndex;
}