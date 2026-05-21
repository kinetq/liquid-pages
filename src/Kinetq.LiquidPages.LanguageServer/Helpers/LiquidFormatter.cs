using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Kinetq.LiquidPages.LanguageServer.Helpers;

/// <summary>
/// Helper class to format Liquid templates using the bundled formatter.exe
/// </summary>
public static class LiquidFormatter
{
    private static string? _formatterPath;

    /// <summary>
    /// Gets the path to the formatter executable
    /// </summary>
    private static string FormatterPath
    {
        get
        {
            if (_formatterPath == null)
            {
                // Get the directory where the language server assembly is located
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var serverDirectory = Path.GetDirectoryName(assemblyLocation)!;
                _formatterPath = Path.Combine(serverDirectory, "formatter.exe");

                if (!File.Exists(_formatterPath))
                {
                    throw new FileNotFoundException($"Formatter executable not found at: {_formatterPath}");
                }
            }

            return _formatterPath;
        }
    }

    /// <summary>
    /// Formats Liquid template content
    /// </summary>
    /// <param name="content">The Liquid template content to format</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted content</returns>
    public static async Task<string> FormatAsync(string content, CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = FormatterPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardInputEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = startInfo };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                outputBuilder.AppendLine(args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                errorBuilder.AppendLine(args.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Write the content to stdin
        await process.StandardInput.WriteAsync(content);
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();

        // Wait for the process to exit
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = errorBuilder.ToString();
            throw new InvalidOperationException($"Formatter failed with exit code {process.ExitCode}: {error}");
        }

        // Return the formatted output (trim the extra newline that was added)
        var result = outputBuilder.ToString();
        return result.TrimEnd('\r', '\n');
    }

    /// <summary>
    /// Checks if the formatter is available
    /// </summary>
    public static bool IsAvailable()
    {
        try
        {
            return File.Exists(FormatterPath);
        }
        catch
        {
            return false;
        }
    }
}
