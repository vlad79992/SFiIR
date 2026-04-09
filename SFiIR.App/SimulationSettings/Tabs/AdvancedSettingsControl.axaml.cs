using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SFiIR.App.SimulationSettings;

public partial class AdvancedSettingsControl : UserControl
{
    private int tabCounter = 0;
    public event EventHandler? Changed;
    public event EventHandler? FillButtonClicked;
    public AdvancedSettingsTabControl ActiveTab =>
        (MainTabControl.SelectedItem as TabItem)?.Content as AdvancedSettingsTabControl
        ?? throw new InvalidOperationException("No active tab");
    public AdvancedSettingsControl()
    {
        InitializeComponent();
        AddNewTab();
    }

    private void AddButton_Click(object? sender, RoutedEventArgs e) => AddNewTab();

    private void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        if (MainTabControl.SelectedItem is TabItem selectedTab)
        {
            if (selectedTab.Content is AdvancedSettingsTabControl content)
            {
                content.Changed -= TabContent_Changed;
                content.FillButtonClicked -= TabContent_FillButtonClicked;
            }

            MainTabControl.Items.Remove(selectedTab);

            if (MainTabControl.Items.Count == 0)
                AddNewTab();
        }
    }

    private void AddNewTab()
    {
        tabCounter++;
        var content = new AdvancedSettingsTabControl();

        content.Changed += TabContent_Changed;
        content.FillButtonClicked += TabContent_FillButtonClicked;

        var tabItem = new TabItem
        {
            Header = tabCounter.ToString(),
            Content = content
        };

        MainTabControl.Items.Add(tabItem);
        MainTabControl.SelectedItem = tabItem;
    }

    public AdvancedSettingsTabControl? GetActiveTab()
    {
        if (MainTabControl.SelectedItem is TabItem selected)
            return selected.Content as AdvancedSettingsTabControl;
        return null;
    }
    private void TabContent_Changed(object? sender, EventArgs e) => Changed?.Invoke(this, e);
    private void TabContent_FillButtonClicked(object? sender, EventArgs e) => FillButtonClicked?.Invoke(this, e);
    private void MainTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Changed?.Invoke(this, e);
}