using SFIiR.Modeling.Statistics;
using SFIiR.Random;

namespace SFIiR.Modeling;

/// <summary>
/// Заявка на решение задачи в АСУ.
/// Представляет собой запрос на выполнение вычислительной задачи с заданными требованиями к ресурсам.
/// </summary>
/// <remarks>
/// <i>АСУ – автоматизированная система управления.</i>
/// <para>
/// При символе компиляции <c>DEBUG_LOGGING</c> класс дополнительно содержит свойство <see cref="RequestInfo"/>.
/// </para>
/// </remarks>
/// <seealso cref="Process"/>
/// <param name="Id">Уникальный идентификатор заявки.</param>
/// <param name="MemUsage">Память, необходимая для создания процесса.</param>
/// <param name="CpuTime">Процессорное время, необходимое для выполнения.</param>
/// <param name="ArrivalTime">Время добавления в очередь заявок.</param>
public class Request(int Id, int MemUsage, int CpuTime, int ArrivalTime)
{
    public int Id { get; } = Id;
    /// <summary>Память, необходимая для создания <see cref="Process">программного процесса</see>
    /// из текущей заявки
    /// </summary>
    public int MemUsage { get; } = MemUsage;
    /// <summary>Процессорное время, необходимое <see cref="Process">программному процессу</see>,
    /// созданному из текущей заявки для выполнения
    /// </summary>
    public int CpuTime { get; } = CpuTime;
    /// <summary>Время добавления в очередь заявок</summary>
    public int ArrivalTime { get; } = ArrivalTime;
#if DEBUG_LOGGING
    /// <summary>Используется для отслеживания и логирования состояния текущей заявки</summary>
    public RequestInfo RequestInfo { get; } = new(ArrivalTime);
#endif
    /// <summary>
    /// Метод для генерации простейшего потока заявок.   
    /// <para><b>Параметры распределений:</b></para>
    /// <list type="bullet">
    /// <item>
    /// <description><paramref name="arrivalLambda"/> — интенсивность входящего потока. 
    /// Среднее время ожидания = <b>1 / <paramref name="arrivalLambda"/></b>.</description>
    /// </item>
    /// <item>
    /// <description><paramref name="memLambda"/> — параметр экспоненциального распределения для памяти. 
    /// Средняя память = <b>1 / <paramref name="memLambda"/></b>.</description>
    /// </item>
    /// <item>
    /// <description><paramref name="cpuLambda"/> — мат. ожидание времени выполнения (Пуассон). 
    /// Среднее время = <b><paramref name="cpuLambda"/></b> квантов.</description>
    /// </item>
    /// </list>
    /// 
    /// <para><b>Примеры:</b></para>
    /// <code>
    /// arrivalLambda = 0.5 -> среднее ожидание 1 / 0.5 = 2.0 ед. времени
    /// arrivalLambda = 2.0 -> среднее ожидание 1 / 2.0 = 0.5 ед. времени
    /// 
    /// memLambda = 0.5 -> средняя память 1 / 0.5 = 2.0 единицы
    /// 
    /// cpuLambda = 5  -> среднее выполнение = 5 квантов
    /// cpuLambda = 20 -> среднее выполнение = 20 квантов
    /// </code>
    /// </summary>
    /// <param name="arrivalLambda">
    /// Интенсивность входящего потока (параметр экспоненты). 
    /// См. <see cref="ExponentialDistribution.Get(double, LCG)"/>.
    /// </param>
    /// <param name="memLambda">
    /// Параметр экспоненциального распределения для памяти. 
    /// См. <see cref="ExponentialDistribution.Get(double, LCG)"/>.
    /// </param>
    /// <param name="cpuLambda">
    /// Параметр распределения Пуассона для процессорного времени (в квантах). 
    /// См. <see cref="PoissonDistribution.Get(double, LCG)"/>.
    /// </param>
    /// <param name="rngSeed">Начальное значение (seed) для ГПСЧ.</param>
    /// <param name="testTime">Ограничение времени прибытия заявок: интервал [0, <paramref name="testTime"/>).</param>
    /// <returns>Список сгенерированных заявок.</returns>
    /// <seealso cref="ExponentialDistribution.Get(double, LCG)"/>
    /// <seealso cref="PoissonDistribution.Get(double, LCG)"/>
    /// <seealso cref="LCG"/>
    public static IEnumerable<Request> GetRequests(double arrivalLambda, double memLambda, int cpuLambda, int rngSeed, int testTime)
    {
        var lcg = new LCG(rngSeed);
        var requests = new List<Request>();
        
        // время прибытия первой заявки
        double currentTime = ExponentialDistribution.Get(arrivalLambda, lcg);
        int idCounter = 1;

        while (currentTime < testTime)
        {
            int memUsage = Math.Max(1, (int)Math.Round(ExponentialDistribution.Get(memLambda, lcg)));
            int cpuTime =  Math.Max(1, PoissonDistribution.Get(cpuLambda, lcg));

            requests.Add(new Request(
                Id: idCounter++,
                MemUsage: memUsage,
                CpuTime: cpuTime,
                ArrivalTime: (int)Math.Round(currentTime)
            ));

            // время прибытия следующей заявки
            currentTime += ExponentialDistribution.Get(arrivalLambda, lcg);
        }

        requests.Sort((a, b) => a.ArrivalTime.CompareTo(b.ArrivalTime));

        return requests;
    }
}