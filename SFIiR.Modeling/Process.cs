using SFIiR.Modeling.Statistics;
namespace SFIiR.Modeling;

/// <summary>
///     Процесс, сформированный из заявки при достаточном количестве свободной памяти.
///     Представляет собой программный процесс, готовый к выполнению в системе управления вычислениями.
/// </summary>
/// <remarks>
///     При символе компиляции <c>DEBUG_LOGGING</c>
///     класс дополнительно содержит свойство <see cref="ProcessInfo"/>.
/// </remarks>
/// <param name="Request">Заявка, из которой сформирован процесс.</param>
/// <seealso cref="Request"/>
public class Process(Request Request)
{
    /// <summary>
    ///     Уникальный идентификатор процесса, совпадающий с идентификатором исходной заявки.
    /// </summary>
    public int Id { get; } = Request.Id;
    /// <summary>
    ///     Количество времени процессора (в квантах), которое осталось до завершения процесса.
    ///     Уменьшается по мере выполнения процесса в системе.
    /// </summary>
    public int CpuTime { get; set; } = Request.CpuTime;
    /// <summary>
    ///     Память, потребляемая процессом.
    ///     Объем основной памяти, необходимый для выполнения процесса.
    /// </summary>
    public int MemUsage { get; } = Request.MemUsage;
#if DEBUG_LOGGING
    /// <summary>
    ///     Используется для отслеживания и логирования состояния текущего процесса.
    ///     Содержит информацию о статусе, времени ожидания и выполнения процесса.
    ///     Доступно только при компиляции с определенным символом DEBUG_LOGGING.
    /// </summary>
    public ProcessInfo ProcessInfo { get; } = new(Request.RequestInfo);
#endif
}