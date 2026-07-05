using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kinetq.LiquidPages.Perf.Models;

public sealed class PerfBenchmarkResponse
{
    [JsonPropertyName("returnCode")]
    public int ReturnCode { get; set; }

    [JsonPropertyName("jobResults")]
    public PerfJobResults JobResults { get; set; } = new();

    [JsonPropertyName("benchmarks")]
    public List<JsonElement> Benchmarks { get; set; } = [];
}

public sealed class PerfJobResults
{
    [JsonPropertyName("jobs")]
    public PerfJobs Jobs { get; set; } = new();

    [JsonPropertyName("properties")]
    public Dictionary<string, JsonElement> Properties { get; set; } = [];
}

public sealed class PerfJobs
{
    [JsonPropertyName("application")]
    public PerfJob Application { get; set; } = new();

    [JsonPropertyName("load")]
    public PerfJob Load { get; set; } = new();
}

public sealed class PerfJob
{
    [JsonPropertyName("results")]
    public Dictionary<string, JsonElement> Results { get; set; } = [];

    [JsonPropertyName("metadata")]
    public List<PerfMetadataEntry> Metadata { get; set; } = [];

    [JsonPropertyName("dependencies")]
    public List<JsonElement> Dependencies { get; set; } = [];

    [JsonPropertyName("measurements")]
    public List<List<PerfMeasurementEntry>> Measurements { get; set; } = [];

    [JsonPropertyName("environment")]
    public PerfEnvironment Environment { get; set; } = new();

    [JsonPropertyName("variables")]
    public PerfVariables Variables { get; set; } = new();

    [JsonPropertyName("benchmarks")]
    public List<JsonElement> Benchmarks { get; set; } = [];
}

public sealed class PerfMetadataEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;
}

public sealed class PerfMeasurementEntry
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public JsonElement Value { get; set; }
}

public sealed class PerfEnvironment
{
    [JsonPropertyName("hw")]
    public string Hardware { get; set; } = string.Empty;

    [JsonPropertyName("env")]
    public string EnvironmentName { get; set; } = string.Empty;

    [JsonPropertyName("os")]
    public string OperatingSystem { get; set; } = string.Empty;

    [JsonPropertyName("arch")]
    public string Architecture { get; set; } = string.Empty;

    [JsonPropertyName("proc")]
    public int ProcessorCount { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

public sealed class PerfVariables
{
    [JsonPropertyName("headers")]
    public Dictionary<string, string> Headers { get; set; } = [];

    [JsonPropertyName("presetHeaders")]
    public string? PresetHeaders { get; set; }

    [JsonPropertyName("serverAddress")]
    public string? ServerAddress { get; set; }

    [JsonPropertyName("connections")]
    public int? Connections { get; set; }

    [JsonPropertyName("warmup")]
    public int? Warmup { get; set; }

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [JsonPropertyName("requests")]
    public int? Requests { get; set; }

    [JsonPropertyName("timeout")]
    public int? Timeout { get; set; }

    [JsonPropertyName("rate")]
    public int? Rate { get; set; }

    [JsonPropertyName("transport")]
    public string? Transport { get; set; }

    [JsonPropertyName("serverScheme")]
    public string? ServerScheme { get; set; }

    [JsonPropertyName("serverPort")]
    public int? ServerPort { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("bodyFile")]
    public string? BodyFile { get; set; }

    [JsonPropertyName("certFile")]
    public string? CertFile { get; set; }

    [JsonPropertyName("keyFile")]
    public string? KeyFile { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("verb")]
    public string? Verb { get; set; }

    [JsonPropertyName("customHeaders")]
    public List<JsonElement> CustomHeaders { get; set; } = [];
}
