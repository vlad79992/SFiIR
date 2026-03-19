namespace SFIiR.Modeling.Statistics;

/// <summary>
///     Лог состояния симуляции в конкретный момент времени <paramref name="Time"/>.
///     Содержит информацию об использовании памяти, текущем процессе и состоянии системы.
/// </summary>
/// <param name="Time">Момент записи лога.</param>
/// <param name="MemoryUsage">Использование памяти в данный момент времени.</param>
/// <param name="ProccessId">
///     ID процесса, к которому относится запись.
///     Если <see cref="Process">процесс</see> не выполняется, <paramref name="ProccessId"/> равен <see langword="null"/>.
/// </param>
/// <param name="RemainingTime">
///     Количество времени, которое осталось процессу до завершения.
///     Если <see cref="Process">процесс</see> не выполняется, <paramref name="RemainingTime"/> равен <see langword="null"/>.
/// </param>
/// <param name="CurrentActivity">
///     Текущее состояние системы или процесса в момент фиксации лога.
///     Определяет тип события, произошедшего в данный момент времени.
/// </param>
public readonly struct LogEntry(
    int Time,
    int MemoryUsage,
    int? ProccessId = null,
    int? RemainingTime = null,
    CurrentActivity CurrentActivity = CurrentActivity.IDLE)
{
    /// <summary>Момент записи лога</summary>
    public int Time { get; } = Time;
    /// <summary>Использование памяти в конкретный момент времени</summary>
    public int MemoryUsage { get; } = MemoryUsage;

    /// <summary>Текущее состояние системы или процесса в момент фиксации лога</summary>
    public CurrentActivity CurrentActivity { get; } = CurrentActivity;
    /// <summary>
    ///     ID процесса, к которому относится запись.
    ///     Если процесс не выполняется, значение равно <see langword="null"/>.
    /// </summary>
    public int? ProccessId { get; } = ProccessId;
    /// <summary>
    ///     Количество времени, которое осталось процессу до завершения.
    ///     Если процесс не выполняется, значение равно <see langword="null"/>.
    /// </summary>
    public int? RemainingTime { get; } = RemainingTime;
}

/// <summary>
///     Перечисление текущих состояний системы или процесса в момент фиксации лога.
///     Определяет тип события, произошедшего в системе управления вычислениями.
/// </summary>
public enum CurrentActivity
{
    /// <summary>Операции не выполняются</summary>
    IDLE,
    /// <summary>Процесс был создан из <see cref="Request">заявки</see> и добавлен в очередь выполнения</summary>
    PROCESS_ADDED,
    /// <summary>Процесс выполняется</summary>
    PROCESS_EXECUTING,
    /// <summary>Процесс завершился, ресурсы освобождены</summary>
    PROCESS_COMPLETED,
    /// <summary>Процесс вернулся из очереди</summary>
    PROCESS_RETURNED,
    /// <summary>Кванты процессора закончились, процесс вернулся в очередь</summary>
    PROCESS_PREEMPTED
}