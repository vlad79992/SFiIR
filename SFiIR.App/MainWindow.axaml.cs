using Avalonia.Controls;
using HarfBuzzSharp;
using System;

namespace SFiIR.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Settings.BasicSettings.Changed += BasicSettings_Changed;

            Settings.AdvancedSettings.Changed += AdvancedSettings_Changed;
            Settings.AdvancedSettings.FillButtonClicked += AdvancedSettings_FillButtonClicked;

            Settings.OlapSettings.Changed += OlapSettings_Changed;
        }

        private void AdvancedSettings_FillButtonClicked(object? sender, EventArgs e)
        {
            var settings = Settings.BasicSettings;

            var tab = Settings.AdvancedSettings.ActiveTab;

            tab.ArrivalLambda = settings.ArrivalLambda;
            tab.MemLambda = settings.MemLambda;
            tab.CpuLambda = settings.CpuLambda;
            tab.ArrivalTime = settings.ArrivalTime;
            tab.TestTime = settings.TestTime;
            tab.RngSeed = settings.RngSeed;
            tab.MaxMem = settings.MaxMem;
            tab.CpuQuant = settings.CpuQuant;

            AdvancedSettings_Changed(sender, e);
        }
        private void OlapSettings_Changed(object? sender, EventArgs e)
        {
            var tab = Settings.OlapSettings;

            Visualization.UpdateOlapVisualization(arrivalLambda: tab.ArrivalLambda, memLambda: tab.MemLambda,
                                                     cpuLambda: tab.CpuLambda, arrivalTime: tab.ArrivalTime,
                                                     testTime: tab.TestTime, maxMem: tab.MaxMem,
                                                     cpuQuant: tab.CpuQuant, selectedParameter: tab.SelectedParameter);
        }
        private void AdvancedSettings_Changed(object? sender, EventArgs e)
        {
            var tab = Settings.AdvancedSettings.ActiveTab;

            Visualization.UpdateChangesVisualization(arrivalLambda: tab.ArrivalLambda, memLambda: tab.MemLambda,
                                                     cpuLambda: tab.CpuLambda, arrivalTime: tab.ArrivalTime,
                                                     testTime: tab.TestTime, rngSeed: tab.RngSeed, maxMem: tab.MaxMem,
                                                     cpuQuant: tab.CpuQuant, selectedParameter: tab.SelectedParameter,
                                                     startValue: tab.StartValue, stepValue: tab.StepValue,
                                                     endValue: tab.EndValue);
        }

        private void BasicSettings_Changed(object? sender, System.EventArgs e)
        {
            var settings = Settings.BasicSettings;

            Visualization.UpdateVisualization(arrivalLambda: settings.ArrivalLambda, memLambda: settings.MemLambda,
                                              cpuLambda: settings.CpuLambda, arrivalTime: settings.ArrivalTime,
                                              testTime: settings.TestTime, rngSeed: settings.RngSeed,
                                              maxMem: settings.MaxMem, cpuQuant: settings.CpuQuant);
        }
    }
}