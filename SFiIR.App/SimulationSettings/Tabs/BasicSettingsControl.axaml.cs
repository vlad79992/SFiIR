using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SFiIR.App.SimulationVisualization;
using System;

namespace SFiIR.App.SimulationSettings;

public partial class BasicSettingsControl : UserControl
{
    public event EventHandler? Changed;
    public BasicSettingsControl()
    {
        InitializeComponent();
    }


    private void ArrivalLambdaBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
    private void MemLambdaBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
    private void CpuLambdaBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
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
    private void ArrivalTimeBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void TestTimeBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void MaxMemBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void CpuQuantBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void RngSeedBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public double? ArrivalLambda => ParseDouble(ArrivalLambdaBox.Text, out double value) ? value : null;
    public double? MemLambda => ParseDouble(MemLambdaBox.Text, out double value) ? value : null;
    public int? CpuLambda => int.TryParse(CpuLambdaBox.Text, out int value) ? value : null;
    public int? ArrivalTime => int.TryParse(ArrivalTimeBox.Text, out int value) ? value : null;
    public int? TestTime => int.TryParse(TestTimeBox.Text, out int value) ? value : null;
    public int? RngSeed => int.TryParse(RngSeedBox.Text, out int value) ? value : null;
    public int? MaxMem => int.TryParse(MaxMemBox.Text, out int value) ? value : null;
    public int? CpuQuant => int.TryParse(CpuQuantBox.Text, out int value) ? value : null;
}