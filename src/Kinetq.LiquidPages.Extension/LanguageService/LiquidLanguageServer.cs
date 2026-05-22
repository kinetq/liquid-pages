using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.LanguageServer;
using Microsoft.VisualStudio.RpcContracts.LanguageServerProvider;
using Nerdbank.Streams;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Reflection;

namespace Kinetq.LiquidPages.Extension.LanguageService;
#pragma warning disable VSEXTPREVIEW_LSP
[VisualStudioContribution]
public class LiquidLanguageServerProvider : LanguageServerProvider
{
    private readonly TraceSource _traceSource;
    public LiquidLanguageServerProvider(
        ExtensionCore container, 
        VisualStudioExtensibility extensibilityObject)
        : base(container, extensibilityObject)
    {
        _traceSource = new TraceSource("Kinetq.LiquidPages.Extension.LanguageServer", SourceLevels.All);
    }

    public override LanguageServerProviderConfiguration LanguageServerProviderConfiguration => new(
        "%LiquidPages.LiquidLanguageServerProvider.DisplayName%",
        [DocumentFilter.FromDocumentType(LiquidDocumentTypeConfiguration.LiquidDocumentType)]);

    protected override void Dispose(bool isDisposing)
    {
        _traceSource.TraceInformation($"Disposing Liquid Language Server.");
        base.Dispose(isDisposing);
    }

    protected override Task InitializeAsync(CancellationToken cancellationToken)
    {
        _traceSource.TraceInformation($"Initializing Liquid Language Server.");
        return base.InitializeAsync(cancellationToken);
    }

    public override Task OnServerInitializationResultAsync(ServerInitializationResult serverInitializationResult, LanguageServerInitializationFailureInfo? initializationFailureInfo, CancellationToken cancellationToken)
    {
        _traceSource.TraceInformation($"OnServerInitializationResultAsync called with result: {serverInitializationResult}");

        if (serverInitializationResult == ServerInitializationResult.Failed)
        {
            _traceSource.TraceInformation($"Server initialization FAILED. Failure info available: {initializationFailureInfo != null}");
            Enabled = false;
        }
        else
        {
            _traceSource.TraceInformation("Server initialization succeeded");
        }

        return base.OnServerInitializationResultAsync(serverInitializationResult, initializationFailureInfo, cancellationToken);
    }

    public override Task<IDuplexPipe?> CreateServerConnectionAsync(CancellationToken cancellationToken)
    {
        _traceSource.TraceInformation("CreateServerConnectionAsync called - LANGUAGE SERVER IS ACTIVATING!");

        // Get the path to the language server executable
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var extensionDirectory = Path.GetDirectoryName(assemblyLocation)!;
        var languageServerExe = Path.Combine(extensionDirectory, "liquid-language-server.exe");

        _traceSource.TraceInformation($"Assembly location: {assemblyLocation}");
        _traceSource.TraceInformation($"Extension directory: {extensionDirectory}");
        _traceSource.TraceInformation($"Looking for language server at: {languageServerExe}");

        if (!File.Exists(languageServerExe))
        {
            var error = $"Language server executable not found at: {languageServerExe}";
            _traceSource.TraceInformation($"ERROR: {error}");

            // List all exe files in the directory to help debug
            try
            {
                var exeFiles = Directory.GetFiles(extensionDirectory, "*.exe");
                _traceSource.TraceInformation($"Available exe files in directory: {string.Join(", ", exeFiles.Select(Path.GetFileName))}");
            }
            catch (Exception ex)
            {
                _traceSource.TraceInformation($"Failed to list exe files: {ex.Message}");
            }

            throw new FileNotFoundException(error);
        }

        _traceSource.TraceInformation("Language server executable found, starting process...");

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
                _traceSource.TraceInformation($"Language Server STDERR: {e.Data}");
            }
        };

        if (process.Start())
        {
            _traceSource.TraceInformation($"Language server process started with PID: {process.Id}");
            process.BeginErrorReadLine();

            var pipe = FullDuplexStream.Splice(process.StandardOutput.BaseStream, process.StandardInput.BaseStream).UsePipe();
            _traceSource.TraceInformation("Duplex pipe created successfully");

            return Task.FromResult<IDuplexPipe?>(pipe);
        }

        _traceSource.TraceInformation("ERROR: Failed to start language server process");
        return Task.FromResult<IDuplexPipe?>(null);
    }
}
#pragma warning restore VSEXTPREVIEW_LSP