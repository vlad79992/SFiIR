using Npgsql;
using SFiIR.Modeling;
using SFiIR.Modeling.Statistics;
using System.Diagnostics;

/// <summary>
/// Параметры одного прогона симуляции.
/// </summary>
public record SimParams(
    double ArrivalLambda,
    double MemLambda,
    int CpuLambda,
    int ArrivalTimeLimit,
    int TestTime,
    int MaxMem,
    int CpuQuant,
    int Seed);

/// <summary>
/// Результат одного прогона: параметры + метрики + время выполнения.
/// </summary>
public record SimResult(
    SimParams Params,
    Metrics Metrics,
    int DurationMs);

/// <summary>
/// Агрегированные метрики после выполнения симуляции.
/// </summary>
public record Metrics(
    int TotalRequests,              // Всего заявок сгенерировано
    int CompletedRequests,          // Успешно завершено
    int RejectedRequests,           // Отклонено (не хватило памяти)
    double CompletedRatio,          // Доля завершённых: completed/total
    double SystemUtilization,       // Загрузка CPU: busy_ticks / test_time
    double AvgMemUsage,             // Среднее использование памяти (по логам)
    double AvgCompletionRatio,      // средний процент выполнения (0..1)
    double AvgRequestQueueTime,     // Среднее время ожидания во входной очереди
    double AvgProcessQueueTime);    // Среднее время ожидания в очереди процессов (Ready Queue)

public static class SimulationRunner
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=sfiir;Username=postgres;Password=;";

    private const int MaxParallelism = 32;
    private const int ProgressStep = 250;

    public static async Task RunBatchAsync(
        IEnumerable<SimParams> parameters,
        CancellationToken ct = default,
        IProgress<string>? progress = null)
    {
        var paramList = parameters.ToList();
        var total = paramList.Count;

        // Сразу сообщаем общее количество
        progress?.Report($"0/{total}");

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = MaxParallelism,
            CancellationToken = ct
        };

        var completed = 0;

        await Parallel.ForEachAsync(paramList, options, async (p, token) =>
        {
            try
            {
                var result = await RunSingleSimulationAsync(p, token);

                await SaveResultAsync(result, token);

                var count = Interlocked.Increment(ref completed);

                if (count % ProgressStep == 0 || count == total)
                {
                    progress?.Report($"{count}/{total}");
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                progress?.Report($"ERR:{p.ArrivalLambda}/{p.MemLambda}:{ex.Message}");
            }
        });

        if (completed % ProgressStep != 0)
        {
            progress?.Report($"{completed}/{total}");
        }
    }

    /// <summary>
    /// Выполняет один прогон симуляции и возвращает метрики.
    /// </summary>
    private static Task<SimResult> RunSingleSimulationAsync(SimParams p, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();

            var requests = Request.GetRequests(
                p.ArrivalLambda,
                p.MemLambda,
                p.CpuLambda,
                p.Seed,
                p.ArrivalTimeLimit
            );

            var acs = new AutomatedControlSystem(p.MaxMem, p.CpuQuant, p.TestTime);
            acs.RunTest(requests);

            var metrics = ExtractMetrics(acs, p);

            ClearSimulationData(acs);

            return new SimResult(
                Params: p,
                Metrics: metrics,
                DurationMs: (int)sw.ElapsedMilliseconds
            );
        }, ct);
    }

    /// <summary>
    /// Простая вставка одной строки в БД.
    /// </summary>
    private static async Task SaveResultAsync(SimResult result, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO olap (
                arrival_lambda, mem_lambda, cpu_lambda, arrival_time_limit, test_time,
                max_mem, cpu_quant, seed,
                total_requests, completed_requests, rejected_requests, completed_ratio,
                system_utilization, avg_mem_usage, avg_completion_ratio, -- ИЗМЕНЕНО ИМЯ КОЛОНКИ
                avg_request_queue_time, avg_process_queue_time
            ) VALUES (
                @arrivalLambda, @memLambda, @cpuLambda, @arrivalTimeLimit, @testTime,
                @maxMem, @cpuQuant, @seed,
                @totalRequests, @completedRequests, @rejectedRequests, @completedRatio,
                @systemUtilization, @avgMemUsage, @avgCompletionRatio, -- ИЗМЕНЕНО ИМЯ ПАРАМЕТРА
                @avgRequestQueueTime, @avgProcessQueueTime
            )
            ON CONFLICT DO NOTHING
            """;

        cmd.Parameters.AddWithValue("arrivalLambda", result.Params.ArrivalLambda);
        cmd.Parameters.AddWithValue("memLambda", result.Params.MemLambda);
        cmd.Parameters.AddWithValue("cpuLambda", result.Params.CpuLambda);
        cmd.Parameters.AddWithValue("arrivalTimeLimit", result.Params.ArrivalTimeLimit);
        cmd.Parameters.AddWithValue("testTime", result.Params.TestTime);
        cmd.Parameters.AddWithValue("maxMem", result.Params.MaxMem);
        cmd.Parameters.AddWithValue("cpuQuant", result.Params.CpuQuant);
        cmd.Parameters.AddWithValue("seed", result.Params.Seed);

        cmd.Parameters.AddWithValue("totalRequests", result.Metrics.TotalRequests);
        cmd.Parameters.AddWithValue("completedRequests", result.Metrics.CompletedRequests);
        cmd.Parameters.AddWithValue("rejectedRequests", result.Metrics.RejectedRequests);
        cmd.Parameters.AddWithValue("completedRatio", result.Metrics.CompletedRatio);
        cmd.Parameters.AddWithValue("systemUtilization", result.Metrics.SystemUtilization);
        cmd.Parameters.AddWithValue("avgMemUsage", result.Metrics.AvgMemUsage);
        cmd.Parameters.AddWithValue("avgCompletionRatio", result.Metrics.AvgCompletionRatio); // ИСПОЛЬЗУЕМ НОВОЕ ЗНАЧЕНИЕ
        cmd.Parameters.AddWithValue("avgRequestQueueTime", result.Metrics.AvgRequestQueueTime);
        cmd.Parameters.AddWithValue("avgProcessQueueTime", result.Metrics.AvgProcessQueueTime);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    /// <summary>
    /// Расчёт агрегированных метрик из результатов симуляции.
    /// </summary>
    private static Metrics ExtractMetrics(AutomatedControlSystem acs, SimParams p)
    {
        var allReqs = acs.AllRequests.ToList();
        var finished = acs.finishedRequests;

        var completed = finished.Count(r => r.RequestInfo.Status == Status.EXITED);
        var rejected = finished.Count(r => r.RequestInfo.Status == Status.NOT_ENOUGH_MEMORY);
        var total = allReqs.Count;

        double completedRatio = total > 0 ? (double)completed / total : 0;

        double cpuBusyTicks = acs.ProcessIntervals.Sum(i => i.end - i.start);
        double systemUtilization = p.TestTime > 0 ? cpuBusyTicks / p.TestTime : 0;

        double avgMemUsage = acs.logEntries.Count > 0
            ? acs.logEntries.Average(l => l.MemoryUsage)
            : 0;

        var processes = acs.AllProcesses;
        var requests = acs.AllRequests;

        double avgCompletionRatio = 0;
        try
        {
            avgCompletionRatio = processes.Select(static p => (double)(p.ProcessInfo.CpuTime - p.CpuTime) / p.ProcessInfo.CpuTime)
                             .Concat(Enumerable.Repeat(0.0, requests.Count() - processes.Count()))
                             .Average();
        }
        catch
        {
            avgCompletionRatio = 0;
        }

        double avgRequestQueueTime = acs.RequestIntervals.Count > 0
            ? acs.RequestIntervals.Average(i => i.end - i.start)
            : 0;

        var allProcs = acs.AllProcesses.ToList();
        double avgProcessQueueTime = allProcs.Count > 0
            ? allProcs.Average(pr => pr.ProcessInfo.QueueTime)
            : 0;

        return new Metrics(
            TotalRequests: total,
            CompletedRequests: completed,
            RejectedRequests: rejected,
            CompletedRatio: completedRatio,
            SystemUtilization: systemUtilization,
            AvgMemUsage: avgMemUsage,
            AvgCompletionRatio: avgCompletionRatio,
            AvgRequestQueueTime: avgRequestQueueTime,
            AvgProcessQueueTime: avgProcessQueueTime
        );
    }

    /// <summary>
    /// Очищает коллекции симуляции для освобождения памяти.
    /// </summary>
    private static void ClearSimulationData(AutomatedControlSystem acs)
    {
        acs.logEntries.Clear();
        acs.ProcessIntervals.Clear();
        acs.RequestIntervals.Clear();
        acs.finishedRequests.Clear();
        acs.finishedProcesses.Clear();
    }
}

public static class SimConfig
{
    public static readonly double[] ArrivalLambdas = { 0.1, 0.25, 0.5, 0.75, 1, 1.5, 2, 3, 5, 7.5, 10, 15, 20, 25 };
    public static readonly double[] MemLambdas = { 0.01, 0.025, 0.05, 0.075, 0.1, 0.15, 0.2, 0.3, 0.5, 0.75, 1.0 };
    public static readonly int[] CpuLambdas = { 1, 3, 5, 7, 10, 15, 20, 25, 30, 40, 50 };
    public static readonly int[] ArrivalTimeLimits = { 100, 250, 500, 1000, 2000 };
    public static readonly int[] TestTimes = { 200, 500, 1000, 2000 };
    public static readonly int[] MaxMems = { 25, 50, 75, 100, 125, 150, 175, 200, 225, 250 };
    public static readonly int[] CpuQuants = { 1, 3, 5, 7, 10, 12, 15, 18, 20, 25 };
    public const int DefaultSeed = 43;

    public static IEnumerable<SimParams> GetAllCombinations()
    {
        foreach (var al in ArrivalLambdas)
            foreach (var ml in MemLambdas)
                foreach (var cl in CpuLambdas)
                    foreach (var atl in ArrivalTimeLimits)
                        foreach (var tt in TestTimes)
                            foreach (var mm in MaxMems)
                                foreach (var cq in CpuQuants)
                                {
                                    if (atl > tt) continue;
                                    yield return new SimParams(al, ml, cl, atl, tt, mm, cq, DefaultSeed);
                                }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var progress = new Progress<string>(msg =>
        {
            Console.Write($"\r{msg}");
        });

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        Console.WriteLine($"Параметров: {SimConfig.GetAllCombinations().Count():N0}");
        Console.WriteLine("Нажмите Ctrl+C для остановки\n");

        try
        {
            await SimulationRunner.RunBatchAsync(
                parameters: SimConfig.GetAllCombinations(),
                ct: cts.Token,
                progress: progress);

            Console.WriteLine("\nЗАВЕРШЕНО");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nОСТАНОВЛЕНО ПОЛЬЗОВАТЕЛЕМ");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nОШИБКА: {ex.Message}");
            Console.ResetColor();
        }
    }
}