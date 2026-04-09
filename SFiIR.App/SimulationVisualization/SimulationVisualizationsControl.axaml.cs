using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SFiIR.App.SimulationVisualization;
using SFiIR.Modeling;
using System.Collections.Generic;
using System.Linq;

namespace SFiIR.App;

public partial class SimulationVisualizationsControl : UserControl
{
    public SimulationVisualizationsControl()
    {
        InitializeComponent();
    }
    public void UpdateVisualization(double? arrivalLambda, double? memLambda, int? cpuLambda, int? arrivalTime,
                                    int? testTime, int? rngSeed, int? maxMem, int? cpuQuant)
    {
        if (arrivalLambda is not null && arrivalLambda > 0)
        {
            Distributions.UpdateArrivalPlot((double)arrivalLambda);
        }
        else
        {
            Distributions.ClearArrivalPlot();
        }


        if (memLambda is not null && memLambda > 0)
        {
            Distributions.UpdateMemPlot((double)memLambda);
        }
        else
        {
            Distributions.ClearMemPlot();
        }


        if (cpuLambda is not null && cpuLambda > 0)
        {
            Distributions.UpdateCpuPlot((int)cpuLambda);
        }
        else
        {
            Distributions.ClearCpuPlot();
        }


        if (Enumerable.All([arrivalLambda, memLambda, cpuLambda, arrivalTime, testTime, rngSeed, maxMem, cpuQuant], x => x is not null && x > 0))
        {
            if (arrivalTime > testTime) arrivalTime = testTime;

            var requests = Request.GetRequests((double)arrivalLambda, (double)memLambda, (int)cpuLambda, (int)rngSeed, (int)arrivalTime);
            var acs = new AutomatedControlSystem((int)maxMem, (int)cpuQuant, (int)testTime);
            acs.RunTest(requests);

            Chart.UpdateChart(acs);
            Statistics.UpdateStatistics(acs);
        }
        else
        {
            Chart.ClearPlot();
            Statistics.ClearStatistics();
        }
    }
    public void UpdateChangesVisualization(double? arrivalLambda, double? memLambda, int? cpuLambda, int? arrivalTime,
                                           int? testTime, int? rngSeed, int? maxMem, int? cpuQuant, int selectedParameter,
                                           double? startValue, double? stepValue, double? endValue)
    {
        if (selectedParameter < 0 || selectedParameter > 7)
        {
            Changes.ClearPlot();
            return;
        }

            switch (selectedParameter)
        {
            case 0: arrivalLambda = startValue; break;
            case 1: memLambda = startValue; break;
            case 2: cpuLambda = (int?)startValue; break;
            case 3: arrivalTime = (int?)startValue; break;
            case 4: testTime = (int?)startValue; break;
            case 5: maxMem = (int?)startValue; break;
            case 6: cpuQuant = (int?)startValue; break;
            case 7: rngSeed = (int?)startValue; break;
        }

        if (Enumerable.All([arrivalLambda, memLambda, cpuLambda, arrivalTime, testTime, rngSeed, maxMem, cpuQuant, startValue, stepValue, endValue], x => x is not null && x > 0))
        {
            var xValues = new List<double>();
            var yMetrics = new List<double>[6];
            for (int i = 0; i < 6; i++) yMetrics[i] = new List<double>();

            for (double value = (double)startValue; value <= (double)endValue; value += (double)stepValue)
            {
                switch (selectedParameter)
                {
                    case 0: arrivalLambda = value; break;
                    case 1: memLambda = value; break;
                    case 2: cpuLambda = (int)value; break;
                    case 3: arrivalTime = (int)value; break;
                    case 4: testTime = (int)value; break;
                    case 5: maxMem = (int)value; break;
                    case 6: cpuQuant = (int)value; break;
                    case 7: rngSeed = (int)value; break;
                }

                if (arrivalTime > testTime) arrivalTime = testTime;

                var requestsGen = Request.GetRequests((double)arrivalLambda, (double)memLambda, (int)cpuLambda, (int)rngSeed, (int)arrivalTime);
                var acs = new AutomatedControlSystem((int)maxMem, (int)cpuQuant, (int)testTime);
                acs.RunTest(requestsGen);

                var processes = acs.AllProcesses;
                var requests = acs.AllRequests;

                var distinctEntries = acs.logEntries.DistinctBy(x => x.Time);
                var idleCount = acs.logEntries.Count(x => x.CurrentActivity == Modeling.Statistics.CurrentActivity.IDLE);

                // [1] Загруженность
                double workload = (double)(acs.testTime - idleCount) / acs.testTime;

                // [2] Ср. использование памяти
                double memRatio = 0;
                try
                {
                    double avgMemUsage = distinctEntries.Average(x => x.MemoryUsage);
                    memRatio = avgMemUsage / acs.maxMem;
                }
                catch { }

                // [0] Выполнено заявок
                int completedCount = processes.Count(p => p.ProcessInfo.Status == Modeling.Statistics.Status.EXITED);
                double completedRatio = (double)completedCount / requests.Count();

                // [3] Ср. выполнение заявок
                double avgCompletion = 0;
                try
                {
                    avgCompletion = processes.Select(static p => (double)(p.ProcessInfo.CpuTime - p.CpuTime) / p.ProcessInfo.CpuTime)
                                             .Concat(Enumerable.Repeat(0.0, requests.Count() - processes.Count()))
                                             .Average();
                }
                catch { }

                // [4] Ср. время ожидания обработки
                double avgReqQueueRatio = 0;
                try
                {
                    double avgReqQueue = requests.Where(r => r.RequestInfo.Status == Modeling.Statistics.Status.EXITED)
                                                 .Average(x => (int)x.RequestInfo.QueueTime);
                    avgReqQueueRatio = avgReqQueue / acs.testTime;
                }
                catch { }

                // [5] Ср. время ожидания в очереди выполнения
                double avgProcQueueRatio = 0;
                try
                {
                    double avgProcQueue = processes.Where(p => p.ProcessInfo.Status == Modeling.Statistics.Status.EXITED)
                                                   .Average(x => x.ProcessInfo.QueueTime);
                    avgProcQueueRatio = avgProcQueue / acs.testTime;
                }
                catch { }

                xValues.Add(value);
                yMetrics[0].Add(completedRatio);
                yMetrics[1].Add(workload);
                yMetrics[2].Add(memRatio);
                yMetrics[3].Add(avgCompletion);
                yMetrics[4].Add(avgReqQueueRatio);
                yMetrics[5].Add(avgProcQueueRatio);
            }

            Changes.Update(xValues, yMetrics);
        }
        else
        {
            Changes.ClearPlot();
        }
    }
}