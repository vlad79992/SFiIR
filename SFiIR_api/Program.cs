using SFiIR.Modeling;
using SFiIR.Modeling.Statistics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

app.MapGet("/", () => Results.Redirect("/index.html"));

app.MapGet("/api/getRequests", (double arrivalLambda, double memLambda, int cpuLambda, int testTime, int rngSeed) =>
{
    var requests = Request.GetRequests(
        arrivalLambda: arrivalLambda,
        memLambda: memLambda,
        cpuLambda: cpuLambda,
        rngSeed: rngSeed,
        testTime: testTime
    );

    return Results.Json(requests.Select(r => new
    {
        r.Id,
        r.ArrivalTime,
        r.MemUsage,
        r.CpuTime
    }));
});

app.MapGet("/api/getProcesses", (double arrivalLambda, double memLambda, int cpuLambda,
    int arrivalTime, int testTime, int rngSeed, int maxMem, int cpuQuant) =>
{
    var requests = Request.GetRequests(arrivalLambda, memLambda, cpuLambda, rngSeed, arrivalTime);
    var acs = new AutomatedControlSystem(maxMem, cpuQuant, testTime);
    acs.RunTest(requests);

    var intervals = new List<(int pid, int start, int end)>();
    int? curPid = null, curStart = null;

    foreach (var log in acs.logEntries.OrderBy(l => l.Time))
    {
        if (log.CurrentActivity == CurrentActivity.PROCESS_EXECUTING)
        {
            if (curPid != log.ProccessId)
            {
                if (curPid.HasValue) intervals.Add((curPid.Value, curStart.Value, log.Time));
                curPid = log.ProccessId;
                curStart = log.Time;
            }
        }
        else if (curPid.HasValue)
        {
            intervals.Add((curPid.Value, curStart.Value, log.Time));
            curPid = null;
        }
    }

    if (curPid.HasValue) intervals.Add((curPid.Value, curStart.Value, testTime));

    //Console.WriteLine($"\n=== РЕЗУЛЬТАТ: {intervals.Count} интервалов ===");
    //foreach (var (pid, start, end) in intervals)
    //    Console.WriteLine($"  Process #{pid}: {start} - {end}");
    //Console.WriteLine("=== КОНЕЦ АНАЛИЗА ===\n");

    foreach (var log in acs.logEntries)
        Console.WriteLine(log);

    return Results.Json(new
    {
        intervals = intervals.Select(i => new { processId = i.pid, startTime = i.start, endTime = i.end }),
        logCount = acs.logEntries.Count,
        processCount = intervals.Select(i => i.pid).Distinct().Count()
    });
});

app.Run();