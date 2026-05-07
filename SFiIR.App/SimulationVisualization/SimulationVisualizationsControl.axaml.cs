using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Npgsql;
using SFiIR.App.SimulationVisualization;
using SFiIR.Modeling;
using System;
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

                // Загруженность
                double workload = (double)(acs.testTime - idleCount) / acs.testTime;

                // Ср. использование памяти
                double memRatio = 0;
                try
                {
                    double avgMemUsage = distinctEntries.Average(x => x.MemoryUsage);
                    memRatio = avgMemUsage / acs.maxMem;
                }
                catch { }

                // Выполнено заявок
                int completedCount = processes.Count(p => p.ProcessInfo.Status == Modeling.Statistics.Status.EXITED);
                double completedRatio = (double)completedCount / requests.Count();

                // Ср. выполнение заявок
                double avgCompletion = 0;
                try
                {
                    avgCompletion = processes.Select(static p => (double)(p.ProcessInfo.CpuTime - p.CpuTime) / p.ProcessInfo.CpuTime)
                                             .Concat(Enumerable.Repeat(0.0, requests.Count() - processes.Count()))
                                             .Average();
                }
                catch { }

                // Ср. время ожидания обработки
                double avgReqQueueRatio = 0;
                try
                {
                    double avgReqQueue = requests.Where(r => r.RequestInfo.Status == Modeling.Statistics.Status.EXITED)
                                                 .Average(x => (int)x.RequestInfo.QueueTime);
                    avgReqQueueRatio = avgReqQueue / acs.testTime;
                }
                catch { }

                // Ср. время ожидания в очереди выполнения
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

    public record OlapResult(
        double ArrivalLambda,
        double MemLambda,
        int CpuLambda,
        int ArrivalTimeLimit,
        int TestTime,
        int MaxMem,
        int CpuQuant,
        int Seed,
        double CompletedRatio,
        double SystemUtilization,
        double AvgMemUsage,
        double AvgCompletionRatio,
        double AvgRequestQueueTime,
        double AvgProcessQueueTime
    );

    public void UpdateOlapVisualization(double? arrivalLambda, double? memLambda, int? cpuLambda, int? arrivalTime,
                                        int? testTime, int? maxMem, int? cpuQuant, int selectedParameter)
    {
        if (selectedParameter < 0 || selectedParameter > 6)
        {
            Olap.ClearPlot();
            return;
        }

        var parameters = new object?[]
        {
            arrivalLambda, memLambda, cpuLambda, arrivalTime, testTime, maxMem, cpuQuant
        };

        for (int i = 0; i < parameters.Length; i++)
        {
            if (i == selectedParameter) continue;

            if (parameters[i] == null)
            {
                Olap.ClearPlot();
                return;
            }
        }

        try
        {
            string connString = "Host=localhost;Port=5432;Database=sfiir;Username=postgres;Password=;";

            var results = new List<OlapResult>();

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                string sql = @"
                    SELECT 
                        arrival_lambda, mem_lambda, cpu_lambda, arrival_time_limit, 
                        test_time, max_mem, cpu_quant, seed,
                        completed_ratio, system_utilization, avg_mem_usage, 
                        avg_completion_time, avg_request_queue_time, avg_process_queue_time
                    FROM public.olap
                    WHERE 1=1
                ";

                var cmd = new NpgsqlCommand(sql, conn);

                // 0: ArrivalLambda
                if (selectedParameter != 0 && arrivalLambda.HasValue)
                {
                    cmd.CommandText += " AND arrival_lambda = @p_arrivalLambda";
                    cmd.Parameters.AddWithValue("@p_arrivalLambda", arrivalLambda.Value);
                }

                // 1: MemLambda
                if (selectedParameter != 1 && memLambda.HasValue)
                {
                    cmd.CommandText += " AND mem_lambda = @p_memLambda";
                    cmd.Parameters.AddWithValue("@p_memLambda", memLambda.Value);
                }

                // 2: CpuLambda
                if (selectedParameter != 2 && cpuLambda.HasValue)
                {
                    cmd.CommandText += " AND cpu_lambda = @p_cpuLambda";
                    cmd.Parameters.AddWithValue("@p_cpuLambda", cpuLambda.Value);
                }

                // 3: ArrivalTime
                if (selectedParameter != 3 && arrivalTime.HasValue)
                {
                    cmd.CommandText += " AND arrival_time_limit = @p_arrivalTime";
                    cmd.Parameters.AddWithValue("@p_arrivalTime", arrivalTime.Value);
                }

                // 4: TestTime
                if (selectedParameter != 4 && testTime.HasValue)
                {
                    cmd.CommandText += " AND test_time = @p_testTime";
                    cmd.Parameters.AddWithValue("@p_testTime", testTime.Value);
                }

                // 5: MaxMem
                if (selectedParameter != 5 && maxMem.HasValue)
                {
                    cmd.CommandText += " AND max_mem = @p_maxMem";
                    cmd.Parameters.AddWithValue("@p_maxMem", maxMem.Value);
                }

                // 6: CpuQuant
                if (selectedParameter != 6 && cpuQuant.HasValue)
                {
                    cmd.CommandText += " AND cpu_quant = @p_cpuQuant";
                    cmd.Parameters.AddWithValue("@p_cpuQuant", cpuQuant.Value);
                }

                // Выполняем запрос
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var result = new OlapResult(
                            ArrivalLambda: reader.GetDouble(reader.GetOrdinal("arrival_lambda")),
                            MemLambda: reader.GetDouble(reader.GetOrdinal("mem_lambda")),
                            CpuLambda: reader.GetInt32(reader.GetOrdinal("cpu_lambda")),
                            ArrivalTimeLimit: reader.GetInt32(reader.GetOrdinal("arrival_time_limit")),
                            TestTime: reader.GetInt32(reader.GetOrdinal("test_time")),
                            MaxMem: reader.GetInt32(reader.GetOrdinal("max_mem")),
                            CpuQuant: reader.GetInt32(reader.GetOrdinal("cpu_quant")),
                            Seed: reader.GetInt32(reader.GetOrdinal("seed")),
                            CompletedRatio: reader.GetDouble(reader.GetOrdinal("completed_ratio")),
                            SystemUtilization: reader.GetDouble(reader.GetOrdinal("system_utilization")),
                            AvgMemUsage: reader.GetDouble(reader.GetOrdinal("avg_mem_usage")),
                            AvgCompletionRatio: reader.GetDouble(reader.GetOrdinal("avg_completion_ratio")),
                            AvgRequestQueueTime: reader.GetDouble(reader.GetOrdinal("avg_request_queue_time")),
                            AvgProcessQueueTime: reader.GetDouble(reader.GetOrdinal("avg_process_queue_time"))
                        );
                        results.Add(result);
                    }
                }
            }

            if (!results.Any())
            {
                Olap.ClearPlot();
                return;
            }

            switch (selectedParameter)
            {
                case 0: results = results.OrderBy(r => r.ArrivalLambda).ToList(); break;
                case 1: results = results.OrderBy(r => r.MemLambda).ToList(); break;
                case 2: results = results.OrderBy(r => r.CpuLambda).ToList(); break;
                case 3: results = results.OrderBy(r => r.ArrivalTimeLimit).ToList(); break;
                case 4: results = results.OrderBy(r => r.TestTime).ToList(); break;
                case 5: results = results.OrderBy(r => r.MaxMem).ToList(); break;
                case 6: results = results.OrderBy(r => r.CpuQuant).ToList(); break;
            }

            var xValues = new List<double>();
            var yMetrics = new List<double>[6];
            for (int i = 0; i < 6; i++) yMetrics[i] = new List<double>();

            foreach (var r in results)
            {
                double xVal = selectedParameter switch
                {
                    0 => r.ArrivalLambda,
                    1 => r.MemLambda,
                    2 => r.CpuLambda,
                    3 => r.ArrivalTimeLimit,
                    4 => r.TestTime,
                    5 => r.MaxMem,
                    6 => r.CpuQuant,
                    _ => 0
                };

                xValues.Add(xVal);

                // Метрика 0: Completed Ratio
                yMetrics[0].Add(r.CompletedRatio);

                // Метрика 1: System Utilization
                yMetrics[1].Add(r.SystemUtilization);

                // Метрика 2: Memory Usage Ratio (нормализуем относительно MaxMem из текущей строки)
                double memRatio = r.MaxMem > 0 ? r.AvgMemUsage / r.MaxMem : 0;
                yMetrics[2].Add(memRatio);

                // Метрика 3: Avg Completion Time
                yMetrics[3].Add(r.AvgCompletionRatio);

                // Метрика 4: Avg Request Queue Time Ratio (нормализуем относительно TestTime)
                double reqQueueRatio = r.TestTime > 0 ? r.AvgRequestQueueTime / r.TestTime : 0;
                yMetrics[4].Add(reqQueueRatio);

                // Метрика 5: Avg Process Queue Time Ratio (нормализуем относительно TestTime)
                double procQueueRatio = r.TestTime > 0 ? r.AvgProcessQueueTime / r.TestTime : 0;
                yMetrics[5].Add(procQueueRatio);
            }

            // Обновляем график
            Olap.Update(xValues, yMetrics);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating OLAP visualization: {ex.Message}");
            Olap.ClearPlot();
        }
    }
}