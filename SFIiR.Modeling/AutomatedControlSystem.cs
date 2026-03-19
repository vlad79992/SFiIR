using SFIiR.Modeling.Statistics;

namespace SFIiR.Modeling;

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
    private readonly int maxMem = maxMem;
    private readonly int testTime = testTime;
    private readonly int cpuQuant = cpuQuant;
    private readonly LinkedList<Request> entryQueue = new();
    private readonly LinkedList<Process> processQueue = new();
#if DEBUG_LOGGING
    /// <summary>
    ///     Журнал событий симуляции, содержащий информацию о состоянии системы в каждый момент времени.
    ///     Доступен только при компиляции с определенным символом DEBUG_LOGGING.
    /// </summary>
    public readonly List<LogEntry> logEntries = new();
    /// <summary>
    ///     Список завершенных заявок, успешно обработанных системой.
    ///     Доступен только при компиляции с определенным символом DEBUG_LOGGING.
    /// </summary>
    public readonly List<Request> finishedRequests = new();
    /// <summary>
    ///     Список завершенных процессов, успешно выполненных системой.
    ///     Доступен только при компиляции с определенным символом DEBUG_LOGGING.
    /// </summary>
    public readonly List<Process> finishedProcesses = new();
#endif
    /// <summary>
    ///     Реализует алгоритм кругового циклического планирования (Round Robin).
    ///     Выполняет один цикл обработки текущего процесса с выделением квантов процессорного времени.
    /// </summary>
    private void RoundRobin()
    {
        if (processQueue.Count == 0) return;

        var process = processQueue.First!.Value;
        processQueue.RemoveFirst();
#if DEBUG_LOGGING
        AddToLogs(process, CurrentActivity.PROCESS_RETURNED);
        int waitTime = curTime - process.ProcessInfo.PreemptedTime;
        process.ProcessInfo.QueueTime += waitTime;
        process.ProcessInfo.Status = Status.EXECUTING;
#endif
        for (int i = 0; i < cpuQuant; i++)
        {
            if (curTime == testTime) return;
            curTime++;
            process.CpuTime--;
#if DEBUG_LOGGING
            process.ProcessInfo.ExecutionTime++;
            AddToLogs(process, CurrentActivity.PROCESS_EXECUTING);
#endif
            if (process.CpuTime <= 0)
            {
                curMem -= process.MemUsage;
#if DEBUG_LOGGING
                AddToLogs(process, CurrentActivity.PROCESS_COMPLETED);
                process.ProcessInfo.Status = Status.EXITED;
                process.ProcessInfo.EndTime = curTime;
                finishedProcesses.Add(process);
#endif
                return;
            }
        }

        if (process.CpuTime > 0)
        {
#if DEBUG_LOGGING
            process.ProcessInfo.PreemptedTime = curTime;
            process.ProcessInfo.Status = Status.WAITING;
            AddToLogs(process, CurrentActivity.PROCESS_PREEMPTED);
#endif
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
#if DEBUG_LOGGING
                    AddToLogs(process, CurrentActivity.PROCESS_ADDED);
                    process.ProcessInfo.PreemptedTime = curTime;
                    req.RequestInfo.ExitTime = curTime;
                    req.RequestInfo.Status = Status.EXITED;
                    finishedRequests.Add(req);
#endif
                    processQueue.AddLast(process);
                }
                else
                {
                    //памяти не хватает
                    var req = entryQueue.First.Value;
                    if (req.MemUsage > maxMem)
                    {
                        entryQueue.RemoveFirst();
#if DEBUG_LOGGING
                        req.RequestInfo.ExitTime = curTime;
                        req.RequestInfo.Status = Status.NOT_ENOUGH_MEMORY;
                        finishedRequests.Add(req);
#endif
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
            curTime++;
        }
    }
#if DEBUG_LOGGING
    /// <summary>
    ///     Добавляет запись в журнал событий симуляции.
    ///     Метод доступен только при компиляции с определенным символом DEBUG_LOGGING.
    /// </summary>
    /// <param name="Process">Процесс, к которому относится запись журнала.</param>
    /// <param name="CurrentActivity">Текущее состояние системы или процесса.</param>
    private void AddToLogs(Process Process, CurrentActivity CurrentActivity) => logEntries.Add(new(
            Time: curTime,
            MemoryUsage: curMem,
            ProccessId: Process.Id,
            RemainingTime: Process.CpuTime,
            CurrentActivity: CurrentActivity.PROCESS_COMPLETED
        ));
#endif
}