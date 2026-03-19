using ScottPlot;
using ScottPlot.Plottables;
using SFIiR.Random;
using System.Security.Cryptography;

namespace SFIiR;

public class GeneratePlot
{
    /// <summary>
    /// Количество точек в равномерном и экспоненциальном распределении
    /// </summary>
    public static int PointsNum
    {
        private get;
        set => field = (value >= 0)
            ? value
            : throw new ArgumentException(null, nameof(value));
    } = 1_000;
    /// <summary>
    /// Количество генераций случайных значений для создания графика
    /// </summary>
    public static int Iters
    {
        private get;
        set => field = (value >= 0)
            ? value
            : throw new ArgumentException(null, nameof(value));
    } = 10_000_000;
    
    public static void Uniform(string filepath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filepath, nameof(filepath));
        var lcg = new LCG(42);
        var plot = new Plot();
        var hist = new double[PointsNum];

        for (int i = 0; i < Iters; i++)
        {
            var val = (int)(lcg.GetRand() * hist.Length);
            hist[val] += 1.0 / Iters;
        }

        plot.Axes.SetLimitsY(0, hist.Max() * 2.5);
        plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.EmptyTickGenerator();
        plot.Title("Равномерное распределение");
        
        plot.Add.ScatterPoints([.. Enumerable.Range(1, PointsNum).Select(x => x / (double)PointsNum)], hist);
        plot.SavePng(filepath, 800, 600);
    }
    
    private static (double[] xs, double[] ys) ExponentialSequence(double lambda, double binWidth)
    {
        var lcg = new LCG(42);
        var hist = new double[PointsNum];

        for (int i = 0; i < Iters; i++)
        {
            var val = ExponentialDistribution.Get(lambda, lcg);
            var binIndex = (int)(val / binWidth);
            if (binIndex < PointsNum)
                hist[binIndex] += 1.0 / Iters / binWidth;

        }
        var xValues = Enumerable.Range(0, PointsNum)
            .Select(i => (i + 0.5) * binWidth)
            .ToArray();

        return (xValues, hist);
    }
    public static void Exponential(double lambda, string filepath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filepath);
        
        double xMax = 5.0 / lambda;
        double binWidth = xMax / PointsNum;
        var plot = new Plot();

        var seq = ExponentialSequence(lambda, binWidth);
        plot.Add.ScatterPoints(seq.xs, seq.ys);

        plot.Axes.SetLimitsX(0, xMax);
        plot.Axes.SetLimitsY(0, seq.ys.Max() * 1.1);
        plot.Title("Экспоненциальное распределение");

        plot.SavePng(filepath, 800, 600);
    }
    public static void Exponential(double[] lambdas, string filepath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filepath);

        double xMax = 5.0 / lambdas.Min();
        double yMax = 0.0;
        double binWidth = xMax / PointsNum;
        var plot = new Plot();

        foreach (var lambda in lambdas)
        {
            var seq = ExponentialSequence(lambda, binWidth);
            var scatter = plot.Add.ScatterPoints(seq.xs, seq.ys);
            scatter.LegendText = $"λ = {lambda}";
            yMax = Math.Max(yMax, seq.ys.Max());
        }

        plot.Axes.SetLimitsX(0, xMax);
        plot.Axes.SetLimitsY(0, yMax * 1.1);
        plot.Legend.Alignment = Alignment.UpperRight;
        plot.Title("Экспоненциальное распределение");

        plot.SavePng(filepath, 800, 600);
    }
    private static (double[] xs, double[] ys) PoissonSequence(double lambda, int xMax)
    {
        var lcg = new LCG(42);

        var hist = new double[xMax + 1];

        for (int i = 0; i < Iters; ++i)
        {
            var val = PoissonDistribution.Get(lambda, lcg);
            if (val < xMax)
                hist[val] += 1.0 / Iters;
        }

        var xValues = Enumerable.Range(0, xMax + 1).Select(x => (double)x).ToArray();
        return (xValues, hist);
    }
    public static void Poisson(double lambda, string filepath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filepath);
        int xMax = (int)(lambda + 6 * Math.Sqrt(lambda));
        var plot = new Plot();
        var seq = PoissonSequence(lambda, xMax);
        plot.Axes.SetLimitsX(0, xMax);
        plot.Axes.SetLimitsY(0, seq.ys.Max() * 1.1);

        plot.Add.Scatter(seq.xs, seq.ys);
        plot.Title("Пуассоновское распределение");

        plot.SavePng(filepath, 800, 600);
    }
    public static void Poisson(double[] lambdas, string filepath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filepath);
        int xMax = (int)(lambdas.Max() + 6 * Math.Sqrt(lambdas.Max()));
        double yMax = 0.0;
        var plot = new Plot();
        foreach (var lambda in lambdas.Distinct())
        {
            var seq = PoissonSequence(lambda, xMax);
            yMax = Math.Max(yMax, seq.ys.Max());
            var scatter = plot.Add.Scatter(seq.xs, seq.ys);
            scatter.LegendText = $"λ = {lambda}";
        }

        plot.Axes.SetLimitsX(0, xMax);
        plot.Axes.SetLimitsY(0, yMax * 1.1);
        plot.Legend.Alignment = Alignment.UpperRight;
        plot.Title("Пуассоновское распределение");

        plot.SavePng(filepath, 800, 600);
    }
}
