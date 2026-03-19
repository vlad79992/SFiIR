using SFIiR.Modeling;


var acs = new AutomatedControlSystem(100, 3, 600);
var requests = Request.GetRequests(1.0 / 10.0, 1.0 / 25.0, 10, 42, 600);
Console.WriteLine(requests.Count());
acs.RunTest(requests);

#if DEBUG_LOGGING
foreach (var proc in acs.finishedProcesses)
{
    Console.WriteLine($"id={proc.Id};requestArrival={proc.ProcessInfo.RequestInfo.ArrivalTime};" +
        $"startTime={proc.ProcessInfo.RequestInfo.ExitTime};EndTime={proc.ProcessInfo.EndTime};" +
        $"execTime={proc.ProcessInfo.ExecutionTime};queueTime={proc.ProcessInfo.QueueTime};mem={proc.MemUsage}");
}
#endif