using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;

namespace Kinetq.LiquidPages.Extension;

/// <summary>
/// Command to install the Liquid Page item template to Visual Studio's templates directory.
/// </summary>
[VisualStudioContribution]
internal class InstallItemTemplateCommand : Command
{
    private readonly TraceSource logger;

    public InstallItemTemplateCommand(TraceSource traceSource)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    public override CommandConfiguration CommandConfiguration => new("Install Liquid Page Template...")
    {
        Icon = new(ImageMoniker.KnownValues.Extension, IconSettings.IconAndText),
        Placements = [CommandPlacement.KnownPlacements.ToolsMenu]
    };

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Detect Visual Studio version
            var vsVersion = DetectVisualStudioVersion();

            // Get source template directory (in extension output)
            var extensionDir = Path.GetDirectoryName(typeof(InstallItemTemplateCommand).Assembly.Location);
            var sourceDir = Path.Combine(extensionDir!, "ItemTemplates", "LiquidPage");

            // Get target directory
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var targetBaseDir = Path.Combine(documentsPath, vsVersion, "Templates", "ItemTemplates", "Visual C#");
            var targetDir = Path.Combine(targetBaseDir, "LiquidPage");

            // Check if source exists
            if (!Directory.Exists(sourceDir))
            {
                await this.Extensibility.Shell().ShowPromptAsync(
                    $"Error: Template files not found at:\n{sourceDir}\n\nPlease ensure the extension is properly installed.",
                    PromptOptions.OK,
                    cancellationToken);
                return;
            }

            // Check if already installed
            if (Directory.Exists(targetDir))
            {
                var overwrite = await this.Extensibility.Shell().ShowPromptAsync(
                    "The Liquid Page template is already installed.\n\nDo you want to reinstall it?",
                    PromptOptions.OKCancel,
                    cancellationToken);

                if (!overwrite)
                {
                    return;
                }

                // Remove existing
                Directory.Delete(targetDir, true);
            }

            // Create target directory
            Directory.CreateDirectory(targetBaseDir);

            // Copy template files
            CopyDirectory(sourceDir, targetDir);

            this.logger.TraceEvent(TraceEventType.Information, 0,
                $"Installed Liquid Page template to: {targetDir}");

            await this.Extensibility.Shell().ShowPromptAsync(
                "✓ Liquid Page template installed successfully!\n\n" +
                "Next steps:\n" +
                "1. Restart Visual Studio\n" +
                "2. Right-click on a folder\n" +
                "3. Select 'Add > New Item'\n" +
                "4. Search for 'Liquid Page'\n\n" +
                $"Template installed to:\n{targetDir}",
                PromptOptions.OK,
                cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.TraceEvent(TraceEventType.Error, 0,
                $"Access denied installing template: {ex.Message}");

            await this.Extensibility.Shell().ShowPromptAsync(
                "Error: Access denied.\n\n" +
                "Visual Studio may not have permission to write to the templates directory.\n" +
                "Try running Visual Studio as Administrator, or manually copy the template files.",
                PromptOptions.OK,
                cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.TraceEvent(TraceEventType.Error, 0,
                $"Error installing template: {ex.Message}");

            await this.Extensibility.Shell().ShowPromptAsync(
                $"Error installing template:\n{ex.Message}\n\n" +
                $"You can manually run the PowerShell script:\n" +
                $"Install-ItemTemplate.ps1 -VSVersion 2026",
                PromptOptions.OK,
                cancellationToken);
        }
    }

    private static string DetectVisualStudioVersion()
    {
        // Try to detect VS version from environment or default to 2026
        var vsVersion = Environment.GetEnvironmentVariable("VisualStudioVersion");

        if (!string.IsNullOrEmpty(vsVersion))
        {
            if (vsVersion.StartsWith("17."))
                return "Visual Studio 2022";
            if (vsVersion.StartsWith("18."))
                return "Visual Studio 2026";
        }

        // Default to 2026 since user is running VS 2026
        return "Visual Studio 2026";
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        // Create target directory
        Directory.CreateDirectory(targetDir);

        // Copy all files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var targetFile = Path.Combine(targetDir, fileName);
            File.Copy(file, targetFile, true);
        }

        // Copy subdirectories recursively
        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(directory);
            var targetSubDir = Path.Combine(targetDir, dirName);
            CopyDirectory(directory, targetSubDir);
        }
    }
}
