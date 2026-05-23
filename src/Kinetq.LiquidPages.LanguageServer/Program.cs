using System.Diagnostics;
using Kinetq.LiquidPages.LanguageServer.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Server;

// Set up file logging for debugging
var logFile = Path.Combine(Path.GetTempPath(), $"liquid-language-server-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log");
var logFactory = LoggerFactory.Create(builder =>
{
    builder.AddFile(logFile);
    builder.SetMinimumLevel(LogLevel.Trace);
});
var logger = logFactory.CreateLogger("LiquidLanguageServer");

logger.LogInformation("Starting Liquid Language Server");
logger.LogInformation($"Log file: {logFile}");
logger.LogInformation($"Process ID: {Process.GetCurrentProcess().Id}");

try
{
    // Create the language server with proper stream handling
    var server = await LanguageServer.From(options =>
        options
            .WithInput(Console.OpenStandardInput())
            .WithOutput(Console.OpenStandardOutput())
            .ConfigureLogging(builder =>
            {
                builder.AddFile(logFile);
                builder.SetMinimumLevel(LogLevel.Trace);
            })
            .WithServices(services =>
            {
                services.AddSingleton<LiquidTextDocumentSyncHandler>();
            })
            .WithHandler<LiquidTextDocumentSyncHandler>()
            .WithHandler<LiquidDocumentFormattingHandler>()
            .OnInitialize((server, request, token) =>
            {
                logger.LogInformation("Server initialized with client: {ClientName}", request.ClientInfo?.Name);
                return Task.CompletedTask;
            })
            .OnInitialized((server, request, response, token) =>
            {
                response.Capabilities.DocumentFormattingProvider = true;
                logger.LogInformation("Server initialization complete");
                return Task.CompletedTask;
            })
    ).ConfigureAwait(false);

    logger.LogInformation("Language server created successfully, waiting for exit...");

    // Wait for the server to exit
    await server.WaitForExit.ConfigureAwait(false);

    logger.LogInformation("Language server exited normally");
}
catch (Exception ex)
{
    logger.LogError(ex, "Fatal error in language server");
    throw;
}
finally
{
    logFactory.Dispose();
}