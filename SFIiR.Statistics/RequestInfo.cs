namespace SFIiR.Statistics;
public class RequestInfo(int ArrivalTime)
{
    public int ArrivalTime { get; set; } = ArrivalTime;
    public int? ExitTime { get; set; } = null;
    public Status Status { get; set; } = Status.UNKNOWN;
    public int? QueueTime => ExitTime - ArrivalTime;
}
