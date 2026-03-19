namespace SFIiR.Random;

/// <summary>
/// Статический класс для генерации случайных величин, 
/// подчиняющихся экспоненциальному распределению/>
/// </summary>
/// <remarks>
/// <see href="https://ru.ruwiki.ru/wiki/Экспоненциальное_распределение">
/// Экспоненциальное распределение
/// </see>
/// </remarks>
public static class ExponentialDistribution
{
    /// <summary>
    ///     Генерирует случайное число с экспоненциальным распределением.
    ///     Используется для моделирования времени между событиями в простейшем (пуассоновском) потоке событий.
    /// </summary>
    /// <param name="lambda">
    ///     Параметр масштаба (интенсивность) экспоненциального распределения.
    ///     Среднее количество событий за единицу времени, 
    ///     то есть среднее время ожидания нового события равно <b>1/<paramref name="lambda"/></b>.
    /// </param>
    /// <param name="lcg">Генератор псевдослучайных чисел.</param>
    /// <exception cref="ArgumentException">
    ///     Выбрасывается, если параметр <paramref name="lambda"/> не является положительным.
    /// </exception> s
    public static double Get(double lambda, LCG lcg)
    {
        if (lambda <= 0) throw new ArgumentException("Лямбда должна быть положительной");
        return - Math.Log(lcg.GetRand()) / lambda;
    }
}
