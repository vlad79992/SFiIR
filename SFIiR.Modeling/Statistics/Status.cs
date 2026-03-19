namespace SFIiR.Modeling.Statistics;

/// <summary>
///     Используется в <see cref="ProcessInfo"/> и <see cref="RequestInfo"/> для обозначения их состояния
///     в системе управления вычислениями с разделением времени.
/// </summary>
public enum Status
{
    /// <summary>
    ///     Используется, чтобы показать, что <see cref="ProcessInfo"/> или <see cref="RequestInfo"/> вышли из очереди.
    ///     Заявка или процесс завершили свое выполнение и покинули систему.
    /// </summary>
    EXITED,
    /// <summary>
    ///     Используется, чтобы показать, что <see cref="ProcessInfo"/> или <see cref="RequestInfo"/> находятся в очереди.
    /// </summary>
    WAITING,
    /// <summary>
    ///     Используется только в <see cref="RequestInfo"/>, если в системе <b>всего</b> меньше памяти, чем необходимо для создания процесса.
    ///     Указывает на ситуацию, когда заявка не может быть преобразована в процесс из-за нехватки основной памяти.
    /// </summary>
    NOT_ENOUGH_MEMORY,
    /// <summary>
    ///     Используется только в <see cref="ProcessInfo"/>, чтобы показать, что он находится в состоянии выполнения.
    /// </summary>
    EXECUTING,
    UNKNOWN
}