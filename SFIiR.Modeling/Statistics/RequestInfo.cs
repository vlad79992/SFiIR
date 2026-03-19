namespace SFIiR.Modeling.Statistics;

/// <summary>
///     Используется для отслеживания состояния заявок, поступающих в систему управления вычислениями.
///     Хранит информацию о времени поступления, состоянии обработки и времени ожидания в очереди.
///     <seealso cref="Request"/>
/// </summary>
/// <param name="ArrivalTime">Время прибытия заявки в систему.</param>
public class RequestInfo(int ArrivalTime)
{
    /// <summary>
    ///     Время прибытия заявки в систему.
    ///     Момент времени, когда заявка была принята системой и помещена в очередь.
    /// </summary>
    public int ArrivalTime { get; } = ArrivalTime;
    /// <summary>
    ///     Время окончания ожидания в очереди.
    ///     Если заявка не вышла из очереди, <paramref name="ExitTime"/> равен <see langword="null"/>.
    /// </summary>
    public int? ExitTime { get; set; } = null;
    /// <summary>
    ///     Состояние заявки в системе управления вычислениями.
    ///     Отражает текущую фазу обработки заявки в системе.
    /// </summary>
    public Status Status { get; set; } = Status.WAITING;
    /// <summary>
    ///     Время ожидания заявки в очереди.
    ///     Вычисляется как разница между временем выхода из очереди и временем прибытия.
    ///     Если заявка не завершилась, <paramref name="QueueTime"/> равен <see langword="null"/>.
    /// </summary>
    public int? QueueTime => ExitTime - ArrivalTime;
}
