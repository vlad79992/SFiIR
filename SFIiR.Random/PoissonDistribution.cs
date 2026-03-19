namespace SFIiR.Random;

/// <summary>
/// Статический класс для генерации случайных величин, 
/// подчиняющихся распределению Пуассона со средней интенсивностью <paramref name="lambda"/>
/// </summary>
/// <remarks>
/// <see href="https://ru.ruwiki.ru/wiki/Распределение_Пуассона">
/// Распределение пуассона
/// </see>
/// </remarks>
public static class PoissonDistribution
{
    /// <summary>
    /// Генерирует целочисленное случайное значение, распределенное по закону Пуассона.
    /// Алгоритм основан на связи между экспоненциальным и пуассоновским распределениями.
    /// </summary>
    /// <param name="lambda">
    /// Среднее количество событий (математическое ожидание) и дисперсия.
    /// Параметр должен быть строго положительным.
    /// </param>
    /// <param name="lcg">Генератор псевдослучайных чисел.</param>
    /// <exception cref="ArgumentException">
    /// Выбрасывается, если параметр <paramref name="lambda"/> не является положительным.
    /// </exception> 
    public static int Get(double lambda, LCG lcg)
    {
        if (lambda <= 0) throw new ArgumentException("Лямбда должна быть положительной");
        var l = Math.Exp(-lambda);
        var k = 0;
        var p = 1.0;
        while (p > l)
        {
            k++;
            p *= lcg.GetRand();
        }
        return k - 1;
    }
}