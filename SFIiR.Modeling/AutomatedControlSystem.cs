using SFiIR.Modeling.Statistics;
using System.Diagnostics;

namespace SFiIR.Modeling;

/// <summary>
///     Используется для проведения тестирования диспетчеризации программных процессов
///     по методу кругового циклического планирования в системе управления вычислениями.
///     Реализует алгоритм обработки заявок с ограничениями по памяти и квантам процессорного времени.
/// </summary>
/// <param name="maxMem">Количество памяти в системе.</param>
/// <param name="cpuQuant">Количество квантов процессора, отведенное для каждого процесса до возвращения в очередь.</param>
/// <param name="testTime">Время тестирования.</param>
public class AutomatedControlSystem(
        int maxMem,
        int cpuQuant,
        int testTime)
{
    private int curMem = 0;
    private int curTime = 0;
    public readonly int maxMem = maxMem;
    public readonly int testTime = testTime;
    public readonly int cpuQuant = cpuQuant;
    private readonly LinkedList<Request> entryQueue = new();
    private readonly LinkedList<Process> processQueue = new();
    /// <summary>
    ///     Журнал событий симуляции, содержащий информацию о состоянии системы в каждый момент времени.
    /// </summary>
    public readonly List<LogEntry> logEntries = new();
    /// <summary>
    ///     Список завершенных заявок, успешно обработанных системой.
    /// </summary>
    public readonly List<Request> finishedRequests = new();
    /// <summary>
    ///     Список завершенных процессов, успешно выполненных системой.
    /// </summary>
    public readonly List<Process> finishedProcesses = new();
    /// <summary>
    ///     Список всех заявок
    /// </summary>
    public IEnumerable<Request> AllRequests => entryQueue.Concat(finishedRequests).DistinctBy(x => x.Id);
    /// <summary>
    ///     Список всех процессов
    /// </summary>
    public IEnumerable<Process> AllProcesses => processQueue.Concat(finishedProcesses).DistinctBy(x => x.Id);
    /// <summary>
    ///     Интервалы <b>выполнения</b> процессов
    /// </summary>
    public readonly List<(int processId, int start, int end)> ProcessIntervals = new();
    /// <summary>
    ///     Интервалы <b>ожидания</b> заявок во входной очереди
    /// </summary>
    public readonly List<(int processId, int start, int end)> RequestIntervals = new();
    /// <summary>
    ///     Реализует алгоритм кругового циклического планирования (Round Robin).
    ///     Выполняет один цикл обработки текущего процесса с выделением квантов процессорного времени.
    /// </summary>
    private void RoundRobin()
    {
        if (processQueue.Count == 0) return;

        var process = processQueue.First!.Value;
        processQueue.RemoveFirst();

        var interval = (process.Id, curTime, (int) -1);
        AddToLogs(process, CurrentActivity.PROCESS_RETURNED);
        int waitTime = curTime - process.ProcessInfo.PreemptedTime;
        process.ProcessInfo.QueueTime += waitTime;
        process.ProcessInfo.Status = Status.EXECUTING;

        for (int i = 0; i < cpuQuant; i++)
        {
            if (curTime == testTime) return;
            curTime++;
            process.CpuTime--;
            process.ProcessInfo.ExecutionTime++;

            AddToLogs(process, CurrentActivity.PROCESS_EXECUTING);

            if (process.CpuTime <= 0)
            {
                curMem -= process.MemUsage;

                interval.Item3 = curTime;
                ProcessIntervals.Add(interval);
                AddToLogs(process, CurrentActivity.PROCESS_COMPLETED);
                process.ProcessInfo.Status = Status.EXITED;
                process.ProcessInfo.EndTime = curTime;
                finishedProcesses.Add(process);

                return;
            }
        }

        if (process.CpuTime > 0)
        {
            interval.Item3 = curTime;
            ProcessIntervals.Add(interval);
            process.ProcessInfo.PreemptedTime = curTime;
            process.ProcessInfo.Status = Status.WAITING;
            AddToLogs(process, CurrentActivity.PROCESS_PREEMPTED);

            processQueue.AddLast(process);
        }
    }
    /// <summary>
    ///     Запускает тест с параметрами, указанными при создании класса.
    ///     Выполняет симуляцию обработки заявок в системе управления вычислениями.
    /// </summary>
    /// <param name="requests">Заявки, для которых выполняется тестирование.</param>
    public void RunTest(IEnumerable<Request> requests)
    {
        foreach (Request request in requests) entryQueue.AddLast(request);

        while (curTime != testTime)
        {
            while (entryQueue.Count > 0 && entryQueue.First!.Value.ArrivalTime <= curTime)
            {
                if (entryQueue.First.Value.MemUsage + curMem <= maxMem)
                {
                    var req = entryQueue.First.Value;
                    entryQueue.RemoveFirst();
                    curMem += req.MemUsage;

                    var process = new Process(req);

                    AddToLogs(process, CurrentActivity.PROCESS_ADDED);
                    process.ProcessInfo.PreemptedTime = curTime;
                    req.RequestInfo.ExitTime = curTime;
                    req.RequestInfo.Status = Status.EXITED;
                    finishedRequests.Add(req);

                    processQueue.AddLast(process);
                }
                else
                {
                    //памяти не хватает
                    var req = entryQueue.First.Value;
                    if (req.MemUsage > maxMem)
                    {
                        entryQueue.RemoveFirst();

                        req.RequestInfo.ExitTime = curTime;
                        req.RequestInfo.Status = Status.NOT_ENOUGH_MEMORY;
                        finishedRequests.Add(req);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (processQueue.Count > 0)
            {
                RoundRobin();
                continue;
            }
            else
            {
                AddToLogs(null, CurrentActivity.IDLE);
            }
            curTime++;
        }

        
        RequestIntervals.AddRange(entryQueue.Select(req => (req.Id, req.ArrivalTime, curTime)));
        RequestIntervals.AddRange(finishedRequests.Select(req => (req.Id, req.ArrivalTime, (int)req.RequestInfo.ExitTime)));

        foreach (var p in processQueue) p.ProcessInfo.QueueTime += curTime - p.ProcessInfo.PreemptedTime;
    }
    /// <summary>
    ///     Добавляет запись в журнал событий симуляции.
    /// </summary>
    /// <param name="Process">Процесс, к которому относится запись журнала.</param>
    /// <param name="CurrentActivity">Текущее состояние системы или процесса.</param>
    private void AddToLogs(Process? Process, CurrentActivity CurrentActivity) => logEntries.Add(new(
            Time: curTime,
            MemoryUsage: curMem,
            ProccessId: Process?.Id,
            RemainingTime: Process?.CpuTime,
            CurrentActivity: CurrentActivity
        ));
}