namespace SFIiR.Statistics;

public struct LogEntry(int Time,
                       int MemoryUsage,
                       int? ProccessId = null,
                       CurrentActivity CurrentActivity = CurrentActivity.IDLE)
{
    public int Time { get; set; } = Time;
    public int MemoryUsage { get; set; } = MemoryUsage;
    public CurrentActivity CurrentActivity { get; set; } = CurrentActivity;
    public int? ProccessId { get; set; } = ProccessId;
}

public enum CurrentActivity
{
    /// <summary>Операции не выполняются</summary>
    IDLE,
    /// <summary>Процесс был создан из заявки</summary>
    PROCESS_ADDED,
    PROCESS_EXECUTING,
    PROCESS_COMPLETED,
    PROCESS_RETURNED
}