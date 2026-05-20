using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.LanguageServer;
using Microsoft.VisualStudio.RpcContracts.LanguageServerProvider;
using Nerdbank.Streams;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Reflection;

namespace Kinetq.LiquidPages.Extension.LanguageService;

[Experimental("VSEXTPREVIEW_LSP")]
[VisualStudioContribution]
internal class LiquidLanguageServerProvider : LanguageServerProvider
{
    private readonly TraceSource _traceSource;
    public LiquidLanguageServerProvider(
        ExtensionCore container, 
        VisualStudioExtensibility extensibilityObject, 
        TraceSource traceSource)
        : base(container, extensibilityObject)
    {
        LanguageServerOptions.InitializationOptions = JToken.Parse(@"[{""server"":""initialize""}]");
        _traceSource = traceSource;
    }
    
    public override Task<IDuplexPipe?> CreateServerConnectionAsync(CancellationToken cancellationToken)
    {
        // Get the path to the language server executable
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var extensionDirectory = Path.GetDirectoryName(assemblyLocation)!;
        var languageServerExe = Path.Combine(extensionDirectory, "liquid-language-server.exe");

        if (!File.Exists(languageServerExe))
        {
            _traceSource.TraceEvent(TraceEventType.Error, 0, $"Language server executable not found at: {languageServerExe}");
            throw new FileNotFoundException($"Language server executable not found at: {languageServerExe}");
        }

        _traceSource.TraceEvent(TraceEventType.Information, 0, $"Starting language server: {languageServerExe}");

        // Start the language server process
        var startInfo = new ProcessStartInfo
        {
            FileName = languageServerExe,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = extensionDirectory
        };
        
        var process = new Process { StartInfo = startInfo };

        // Log stderr for debugging
        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _traceSource.TraceEvent(TraceEventType.Warning, 0, $"Language Server stderr: {e.Data}");
            }
        };

        if (process.Start())
        {
            process.BeginErrorReadLine();
            _traceSource.TraceEvent(TraceEventType.Information, 0, "Language server process started successfully");

            return Task.FromResult<IDuplexPipe?>(
                FullDuplexStream.Splice(process.StandardOutput.BaseStream, process.StandardInput.BaseStream).UsePipe());
        }

        _traceSource.TraceEvent(TraceEventType.Error, 0, "Failed to start language server process");
        return Task.FromResult<IDuplexPipe?>(null);
    }

    public override LanguageServerProviderConfiguration LanguageServerProviderConfiguration => new(
        "%LiquidPages.LiquidLanguageServerProvider.DisplayName%",
        [DocumentFilter.FromDocumentType(LiquidDocumentTypeConfiguration.LiquidDocumentType)]);

    public override Task OnServerInitializationResultAsync(ServerInitializationResult serverInitializationResult, LanguageServerInitializationFailureInfo? initializationFailureInfo, CancellationToken cancellationToken)
    {
        if (serverInitializationResult == ServerInitializationResult.Failed)
        {
            _traceSource.TraceInformation($"Exception: {initializationFailureInfo.Exception} Status: {initializationFailureInfo.StatusMessage}");
            Enabled = false;
        }

        return base.OnServerInitializationResultAsync(serverInitializationResult, initializationFailureInfo, cancellationToken);
    }
}