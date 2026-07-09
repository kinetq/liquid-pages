using Kinetq.LiquidPages.NetCoreServer.Sample;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

Console.WriteLine("Hello, World!");

var services = new ServiceCollection().AddLogging(builder =>
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
services.AddLiquidPages(typeof(Program).Assembly);

var serviceProvider = services.BuildServiceProvider();

var routeTree = serviceProvider.GetService<IRouteTree>();
var liquidResponseMiddleware = serviceProvider.GetService<ILiquidResponseMiddleware>();
var liquidStartup = serviceProvider.GetService<ILiquidStartup>();

liquidStartup.RegisterFilters();
liquidStartup.RegisterPageModels();
liquidStartup.RegisterFileProvider("/",
    new EmbeddedFileProvider(typeof(Program).Assembly));

routeTree.Initialize();
var server = new LiquidHttpServer(IPAddress.Loopback, 8080, routeTree, liquidResponseMiddleware);

string staticFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Static");
server.AddStaticContent(staticFilesPath, "/Static");

Console.Write("Server starting...");
server.Start();
Console.WriteLine("Done!");

Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

// Perform text input
for (; ; )
{
    string line = Console.ReadLine();
    if (string.IsNullOrEmpty(line))
        break;

    // Restart the server
    if (line == "!")
    {
        Console.Write("Server restarting...");
        server.Restart();
        Console.WriteLine("Done!");
    }
}

// Stop the server
Console.Write("Server stopping...");
server.Stop();
Console.WriteLine("Done!");