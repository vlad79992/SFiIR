using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace SFiIR.App;

public partial class StatisticsCardControl : UserControl
{
    public StatisticsCardControl()
    {
        InitializeComponent();
        Id = RequestArrivalTime = TimeInQueue = ProcessArrivalTime = ProcessQueueTime
            = ProcessExecutionTime = RequiredCpuTime = RequiredMemory = null;
    }
    public int? Id
    {
        get => int.TryParse(IdText.Text, out var id) ? id : null;
        set
        {
            IdText.Text = value?.ToString() ?? "null";
            if (value is not null)
                IdText.Bind(TextBlock.ForegroundProperty, Resources.GetResourceObservable("AccentBrush"));
            else
                IdText.Foreground = Brushes.Red;
        }
    }
    public int? RequestArrivalTime
    {
        get => int.TryParse(RequestArrivalTimeText.Text, out var id) ? id : null;
        set
        {
            RequestArrivalTimeText.Text = value?.ToString() ?? "null";
            if (value is not null)
                RequestArrivalTimeText.Bind(TextBlock.ForegroundProperty, Resources.GetResourceObservable("AccentBrush"));
            else
                RequestArrivalTimeText.Foreground = Brushes.Red;
        }
    }
    public int? TimeInQueue
    {
        get => int.TryParse(TimeInQueueText.Text, out var id) ? id : null;
        set
        {
            TimeInQueueText.Text = value?.ToString() ?? "null";
            if (value is not null)
                TimeInQueueText.Bind(TextBlock.ForegroundProperty, Resources.GetResourceObservable("AccentBrush"));
            else
                TimeInQueueText.Foreground = Brushes.Red;
        }
    }
    public int? ProcessArrivalTime
    {
        get => int.TryParse(ProcessArrivalTimeText.Text, out var id) ? id : null;
        set
        {
            ProcessArrivalTimeText.Text = value?.ToString() ?? "null";
            if (value is not null)
                ProcessArrivalTimeText.Bind(TextBlock.ForegroundProperty, Resources.GetResourceObservable("AccentBrush"));
            else
                ProcessArrivalTimeText.Foreground = Brushes.Red;
        }
    }
    public int? ProcessQueueTime
    {
        get => int.TryParse(ProcessQueueTimeText.Text, out var id) ? id : null;
        set
        {
            ProcessQueueTimeText.Text = value?.ToString() ?? "null";
            if (value is not null)
                ProcessQueueTimeText.Bind(TextBlock.ForegroundProperty, Resources.GetResourceObservable("AccentBrush"));
            else
                ProcessQueueTimeText.Foreground = Brushes.Red;
        }
    }
    public int?  ProcessExecutionTime
    {
        get => int.TryParse(ProcessExecutionTimeText.Text, out var id) ? id : null;
        set
        {
            ProcessExecutionTimeText.Text = value?.ToString() ?? "null";
            if (value is not null)
                ProcessExecutionTimeText.Bind(TextBlock.ForegroundProperty, Resources.GetResourceObservable("AccentBrush"));
            else
                ProcessExecutionTimeText.Foreground = Brushes.Red;
        }
    }
    public int? RequiredCpuTime
    {
        get => int.TryParse(RequiredCpuTimeText.Text, out var id) ? id : null;
        set
        {
            RequiredCpuTimeText.Text = value?.ToString() ?? "null";
            if (value is not null)
                RequiredCpuTimeText.Bind(TextBlock.ForegroundProperty, Resources.GetResourceObservable("AccentBrush"));
            else
                RequiredCpuTimeText.Foreground = Brushes.Red;
        }
    }
    public int? RequiredMemory
    {
        get => int.TryParse(RequiredMemoryText.Text, out var id) ? id : null;
        set
        {
            RequiredMemoryText.Text = value?.ToString() ?? "null";
            if (value is not null)
                RequiredMemoryText.Bind(TextBlock.ForegroundProperty, Resources.GetResourceObservable("AccentBrush"));
            else
                RequiredMemoryText.Foreground = Brushes.Red;
        }
    }
}