using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Channels;

namespace SFiIR.App.SimulationSettings;

public partial class AdvancedSettingsTabControl : UserControl
{
    public event EventHandler? Changed;
    private readonly Dictionary<int, (TextBlock label, TextBox input)> parameterControls;
    public AdvancedSettingsTabControl()
    {
        InitializeComponent();

        parameterControls = new()
        {
            { 0, (ArrivalLambdaText, ArrivalLambdaBox) },
            { 1, (MemLambdaText, MemLambdaBox) },
            { 2, (CpuLambdaText, CpuLambdaBox) },
            { 3, (ArrivalTimeText, ArrivalTimeBox) },
            { 4, (TestTimeText, TestTimeBox) },
            { 5, (MaxMemText, MaxMemBox) },
            { 6, (CpuQuantText, CpuQuantBox) },
            { 7, (RngSeedText, RngSeedBox) }
        };
    }

    private void ArrivalLambdaBox_TextChanged(object? sender, TextChangedEventArgs e) => Changed?.Invoke(sender, e);
    private void MemLambdaBox_TextChanged(object? sender, TextChangedEventArgs e) => Changed?.Invoke(sender, e);
    private void CpuLambdaBox_TextChanged(object? sender, TextChangedEventArgs e) => Changed?.Invoke(sender, e);
    private void ArrivalTimeBox_TextChanged(object? sender, TextChangedEventArgs e) => Changed?.Invoke(sender, e);
    private void TestTimeBox_TextChanged(object? sender, TextChangedEventArgs e) => Changed?.Invoke(sender, e);
    private void MaxMemBox_TextChanged(object? sender, TextChangedEventArgs e) => Changed?.Invoke(sender, e);
    private void CpuQuantBox_TextChanged(object? sender, TextChangedEventArgs e) => Changed?.Invoke(sender, e);
    private void RngSeedBox_TextChanged(object? sender, TextChangedEventArgs e) => Changed?.Invoke(sender, e);
    private void StartBox_TextChanged(object? sender, TextChangedEventArgs e) => Changed?.Invoke(sender, e);
    private void StepBox_TextChanged(object? sender, TextChangedEventArgs e) => Changed?.Invoke(sender, e);
    private void EndBox_TextChanged(object? sender, TextChangedEventArgs e) => Changed?.Invoke(sender, e);
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
        Changed.Invoke(this, EventArgs.Empty);
    }
    private void SetAllParametersVisible(bool visible)
    {
        foreach (var (_, controls) in parameterControls)
        {
            controls.label.IsVisible = visible;
            controls.input.IsVisible = visible;
        }
    }

    public event EventHandler? FillButtonClicked;
    private void FillButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        FillButtonClicked?.Invoke(this, e);
    }
    public double? ArrivalLambda
    {
        get => ParseDouble(ArrivalLambdaBox.Text, out double value) ? value : null; 
        set => ArrivalLambdaBox.Text = value.ToString();
    }
    public double? MemLambda
    {
        get => ParseDouble(MemLambdaBox.Text, out double value) ? value : null; 
        set => MemLambdaBox.Text = value.ToString();
    }
    public int? CpuLambda
    {
        get => int.TryParse(CpuLambdaBox.Text, out int value) ? value : null; 
        set => CpuLambdaBox.Text = value.ToString();
    }
    public int? ArrivalTime
    {
        get => int.TryParse(ArrivalTimeBox.Text, out int value) ? value : null; 
        set => ArrivalTimeBox.Text = value.ToString();
    }
    public int? TestTime
    {
        get => int.TryParse(TestTimeBox.Text, out int value) ? value : null; 
        set => TestTimeBox.Text = value.ToString();
    }
    public int? RngSeed
    {
        get => int.TryParse(RngSeedBox.Text, out int value) ? value : null; 
        set => RngSeedBox.Text = value.ToString();
    }
    public int? MaxMem
    {
        get => int.TryParse(MaxMemBox.Text, out int value) ? value : null; 
        set => MaxMemBox.Text = value.ToString();
    }
    public int? CpuQuant
    {
        get => int.TryParse(CpuQuantBox.Text, out int value) ? value : null; 
        set => CpuQuantBox.Text = value.ToString();
    }
    public double? StartValue
    {
        get => ParseDouble(StartBox.Text, out double value) ? value : null;
        set => StartBox.Text = value?.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public double? StepValue
    {
        get => ParseDouble(StepBox.Text, out double value) ? value : null;
        set => StepBox.Text = value?.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public double? EndValue
    {
        get => ParseDouble(EndBox.Text, out double value) ? value : null;
        set => EndBox.Text = value?.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
    public int SelectedParameter => ParameterCombo.SelectedIndex;

    private static bool ParseDouble(string? text, out double value)
    {
        value = 0.0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return double.TryParse(text.Replace(",", "."),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out value);
    }
}