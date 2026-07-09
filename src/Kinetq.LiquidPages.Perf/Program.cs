using System.Text;
using System.Text.Json;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Perf.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddLiquidPages(typeof(Program).Assembly);
    services.AddLogging(builder =>
{
    builder.ClearProviders();
    // Clear Microsoft's default providers (like eventlogs and others)
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.TimestampFormat = "hh:mm:ss ";
    }).SetMinimumLevel(LogLevel.Debug);
});
var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();

var registeredTypeManager = serviceProvider.GetService<ILiquidRegisteredTypesManager>();
registeredTypeManager.RegisterType<PerfBenchmarkResponse>();
registeredTypeManager.RegisterType<PerfJobResults>();
registeredTypeManager.RegisterType<PerfJobs>();
registeredTypeManager.RegisterType<PerfJob>();
registeredTypeManager.RegisterType<PerfMetadataEntry>();
registeredTypeManager.RegisterType<PerfMeasurementEntry>();
registeredTypeManager.RegisterType<PerfEnvironment>();
registeredTypeManager.RegisterType<PerfViewModel>();
registeredTypeManager.RegisterType<PerfComparisonRow>();
registeredTypeManager.RegisterType<PerfBenchmarkRun>();
registeredTypeManager.RegisterType<PerfComparisonValue>();
registeredTypeManager.RegisterType<PerfViewModel.MetricDefinition>();


var startup = scope.ServiceProvider.GetRequiredService<ILiquidStartup>();
startup.RegisterFileProvider("/", new EmbeddedFileProvider(typeof(Program).Assembly));


string outputPath = args[0];

var files = Directory.GetFiles(outputPath, "liquid_results_*.json");
var viewModel = new PerfViewModel();

foreach (var file in files)
{
    var fileContents = await File.ReadAllTextAsync(file);
    var benchMarkResponse = JsonSerializer.Deserialize<PerfBenchmarkResponse>(fileContents);

    if (benchMarkResponse != null)
    {
        viewModel.AddBenchmarkResponse(file, benchMarkResponse);
    }
}

var htmlRenderer = scope.ServiceProvider.GetRequiredService<IHtmlRenderer>();
var html = await htmlRenderer.RenderHtml("/", "Templates/perf.liquid", new RenderModel()
{
    ViewModel = viewModel
});

await File.WriteAllTextAsync(Path.Join(outputPath, "perf-results.html"), html, Encoding.UTF8);

Console.WriteLine($"HTML successfully written to: {outputPath}");
