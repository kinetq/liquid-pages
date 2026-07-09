using System.Globalization;
using System.Text.Json;

namespace Kinetq.LiquidPages.Perf.Models;

public class PerfViewModel
{
    private static readonly IReadOnlyList<MetricDefinition> ComparisonMetrics =
    [
        new("application", "benchmarks/build-time", "App Build Time (ms)"),
        new("application", "benchmarks/start-time", "App Start Time (ms)"),
        new("application", "benchmarks/published-size", "Published Size (KB)"),
        new("application", "benchmarks/working-set", "App Working Set (MB)"),
        new("application", "benchmarks/private-memory", "App Private Memory (MB)"),
        new("application", "benchmarks/cpu/global", "App Max Global CPU (%)"),
        new("load", "http/rps/mean", "Load RPS Mean"),
        new("load", "http/latency/95", "Load Latency P95 (ms)"),
        new("load", "http/throughput", "Load Throughput (MB/s)"),
        new("load", "http/requests", "Load Total Requests"),
        new("load", "benchmarks/working-set", "Load Working Set (MB)"),
        new("load", "benchmarks/private-memory", "Load Private Memory (MB)")
    ];

    public string Title { get; set; } = "Performance Benchmark Comparison";

    public IList<PerfBenchmarkRun> Runs { get; } = new List<PerfBenchmarkRun>();

    public IList<PerfComparisonRow> ComparisonRows { get; } = new List<PerfComparisonRow>();

    public int ResponseCount => Runs.Count;

    public string BaselineLabel => Runs.FirstOrDefault()?.Label ?? "n/a";

    public void AddBenchmarkResponse(string filePath, PerfBenchmarkResponse benchmarkResponse)
    {
        Runs.Add(new PerfBenchmarkRun
        {
            Label = BuildLabel(filePath, Runs.Count + 1),
            BenchmarkResponse = benchmarkResponse
        });

        RebuildComparisonRows();
    }

    private void RebuildComparisonRows()
    {
        ComparisonRows.Clear();

        foreach (var metric in ComparisonMetrics)
        {
            var row = BuildComparisonRow(metric);
            if (row.Values.Count > 0)
            {
                ComparisonRows.Add(row);
            }
        }
    }

    private PerfComparisonRow BuildComparisonRow(MetricDefinition metric)
    {
        var row = new PerfComparisonRow
        {
            MetricKey = $"{metric.JobName}:{metric.ResultKey}",
            MetricName = metric.DisplayName
        };

        double? baseline = null;

        foreach (var run in Runs)
        {
            var value = TryGetMetricValue(run.BenchmarkResponse, metric.JobName, metric.ResultKey);

            if (baseline == null && value.HasValue)
            {
                baseline = value.Value;
            }

            var comparisonValue = new PerfComparisonValue
            {
                Label = run.Label,
                Value = value,
                DisplayValue = FormatValue(value),
                MetricKey = metric.ResultKey
            };

            if (baseline.HasValue && value.HasValue)
            {
                var delta = value.Value - baseline.Value;
                comparisonValue.Delta = delta;
                comparisonValue.DisplayDelta = FormatSignedValue(delta);

                if (baseline.Value != 0)
                {
                    var percentDelta = (delta / baseline.Value) * 100d;
                    comparisonValue.DeltaPercentage = percentDelta;
                    comparisonValue.DisplayDeltaPercentage = $"{percentDelta:+0.##;-0.##;0}%";
                }
                else
                {
                    comparisonValue.DisplayDeltaPercentage = "n/a";
                }
            }
            else
            {
                comparisonValue.DisplayDelta = "n/a";
                comparisonValue.DisplayDeltaPercentage = "n/a";
            }

            row.Values.Add(comparisonValue);
        }

        var maxValue = row.Values
            .Where(static value => value.Value.HasValue)
            .Select(static value => value.Value!.Value)
            .DefaultIfEmpty(0d)
            .Max();

        row.MaxValue = maxValue;

        foreach (var value in row.Values)
        {
            if (!value.Value.HasValue || maxValue <= 0)
            {
                value.BarWidthPercentage = 0;
                continue;
            }

            value.BarWidthPercentage = (value.Value.Value / maxValue) * 100d;
        }

        return row;
    }

    private static double? TryGetMetricValue(PerfBenchmarkResponse benchmarkResponse, string jobName, string resultKey)
    {
        var job = jobName switch
        {
            "application" => benchmarkResponse.JobResults?.Jobs?.Application,
            "load" => benchmarkResponse.JobResults?.Jobs?.Load,
            _ => null
        };

        if (job?.Results == null || !job.Results.TryGetValue(resultKey, out var element))
        {
            return null;
        }

        return element.ValueKind switch
        {
            JsonValueKind.Number when element.TryGetDouble(out var numericValue) => numericValue,
            JsonValueKind.String when double.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null
        };
    }

    private static string BuildLabel(string filePath, int index)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return $"Run {index}";
        }

        const string prefix = "liquid_results_";
        if (fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            fileName = fileName[prefix.Length..];
        }

        return fileName.Replace('_', ' ');
    }

    private static string FormatValue(double? value)
    {
        if (!value.HasValue)
        {
            return "n/a";
        }

        return value.Value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatSignedValue(double? value)
    {
        if (!value.HasValue)
        {
            return "n/a";
        }

        return value.Value.ToString("+0.##;-0.##;0", CultureInfo.InvariantCulture);
    }

    public sealed record MetricDefinition(string JobName, string ResultKey, string DisplayName);
}

public class PerfBenchmarkRun
{
    public string Label { get; set; } = string.Empty;

    public PerfBenchmarkResponse BenchmarkResponse { get; set; } = new();
}

public class PerfComparisonRow
{
    public string MetricKey { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public double MaxValue { get; set; }
    public IList<PerfComparisonValue> Values { get; } = new List<PerfComparisonValue>();
}

public class PerfComparisonValue
{
    public string MetricKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    public double? Value { get; set; }

    public string DisplayValue { get; set; } = "n/a";

    public double BarWidthPercentage { get; set; }

    public double? Delta { get; set; }

    public string DisplayDelta { get; set; } = "n/a";

    public double? DeltaPercentage { get; set; }

    public string DisplayDeltaPercentage { get; set; } = "n/a";

    private string[] InverseDeltaColumns = new string[]
    {
        "benchmarks/build-time",
        "benchmarks/start-time",
        "benchmarks/published-size",
        "benchmarks/working-set",
        "benchmarks/private-memory",
        "benchmarks/cpu/global",
        "http/latency/95",
        "benchmarks/working-set"
};
    
    public string Color
    {
        get
        {
            if (Delta is 0)
            {
                return "#e2e8f0";
            }
            
            return InverseDeltaColumns.Contains(MetricKey)
                ? (Delta is < 0 ? "#4ade80" : "#f87171")
                : (Delta is > 0 ? "#4ade80" : "#f87171");
        }
    }
}
