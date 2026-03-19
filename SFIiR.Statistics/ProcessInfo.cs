namespace SFIiR.Statistics;

public class ProcessInfo(RequestInfo RequestInfo)
{
    private RequestInfo RequestInfo { get; set; } = RequestInfo;
    public Status Status { get; set; } = Status.UNKNOWN;
    public int QueueTime { get; set; } = 0;
    public int InsertTime { get; set; } = 0;
    public int ExecutionTime { get; set; } = 0;
}