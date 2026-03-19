namespace SFIiR.Random;

/// <summary>
/// Генератор псевдослучайных чисел, реализующий линейный конгруэнтный метод (LCG).
/// Используется для генерации последовательностей псевдослучайных чисел с равномерным распределением.
/// </summary>
/// <param name="seed">Начальное значение для генерации последовательности.</param>
/// <remarks>
/// <see href="https://ru.ruwiki.ru/wiki/Линейный_конгруэнтный_метод">
/// Линейный конгруэнтный метод
/// </see>
/// </remarks>
public class LCG(long seed,
                 long a = 1103515245L,
                 long c = 12345L,
                 long m = 2147483648L)
{
    private long next = seed;
    private readonly long a = a;
    private readonly long c = c;
    private readonly long m = m;
    /// <summary>
    /// Устанавливает новое начальное значение для генератора
    /// </summary>
    /// <value>Новое значение зерна</value>
    public long Seed { set => next = value; }
    /// <summary>
    /// Генерирует следующее псевдослучайное число с равномерным распределением
    /// </summary>
    /// <returns>
    /// Число типа <see langword="double"/> в полуоткрытом интервале <c>[0.0, 1.0)</c>
    /// </returns>
    public double GetRand()
    {
        next = (next * a + c) % m;
        return ((double)next / m);
    }
    /// <summary>
    /// Генерирует последовательность из <paramref name="n"/> псевдослучайных чисел.
    /// </summary>
    /// <param name="n">Количество чисел в последовательности.</param>
    /// <returns>
    /// Коллекция <see cref="IEnumerable{Double}"></see>, содержащая <paramref name="n"/> чисел 
    /// в диапазоне <c>[0.0, 1.0)</c>.
    /// </returns>
    public IEnumerable<double> GenerateSequence(int n) => Enumerable.Range(0, n).Select(_ => this.GetRand());
}