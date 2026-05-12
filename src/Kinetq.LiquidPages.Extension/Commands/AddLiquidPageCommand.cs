using System.Text;
using Microsoft.VisualStudio.RpcContracts.Notifications;

namespace Kinetq.LiquidPages.Extension.Commands;

using Kinetq.LiquidPages.Extension.Dialogs;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.ProjectSystem.Query;
using System.Diagnostics;

[VisualStudioContribution]
internal class AddLiquidPageCommand : Command
{
    private readonly TraceSource logger;

    public AddLiquidPageCommand(VisualStudioExtensibility extensibility, TraceSource traceSource)
        : base(extensibility)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    public override CommandConfiguration CommandConfiguration => new("Add LiquidPage")
    {
        Icon = new(ImageMoniker.Custom("{b1a9eb31-d18e-4617-985a-e4e511f68994}:LiquidPages"), IconSettings.IconAndText),
        Placements =
        [
            CommandPlacement.VsctParent(
                new Guid("{d309f791-903f-11d0-9efc-00a0c911004f}"), // guidSHLMainMenu
                id: 518,   // IDM_VS_CTXT_PROJNODE_ADD
                priority: 0)
        ]
    };

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var activeProject = await context.GetActiveProjectAsync(cancellationToken);
        if (activeProject?.Path == null)
        {
            await Extensibility.Shell().ShowPromptAsync(
                "No project selected.",
                PromptOptions.OK,
                cancellationToken);
            return;
        }

        var projectDir = Path.GetDirectoryName(activeProject.Path);
        if (string.IsNullOrEmpty(projectDir))
        {
            await Extensibility.Shell().ShowPromptAsync(
                "Unable to determine project directory.",
                PromptOptions.OK,
                cancellationToken);
            return;
        }

        // Show dialog to get page name from user
        var dialog = new PageNameDialogControl();

        // Start showing the dialog (non-blocking)
        var dialogResult = await Extensibility.Shell().ShowDialogAsync(dialog, DialogOption.OKCancel, cancellationToken);
        if (dialogResult == DialogResult.Cancel)
        {
            logger.TraceEvent(TraceEventType.Information, 0,
                "User cancelled page creation");

            return;
        }

        // Convert to PascalCase
        PageNameData dialogData = (PageNameData)dialog.DataContext;
        var pageName = ToPascalCase(dialogData.PageName.Trim());

        logger.TraceEvent(TraceEventType.Information, 0,
            $"Creating page with name: {pageName}");

        // Execute dotnet new liquidpage command
        var result = await ExecuteDotnetNewCommand(projectDir, dialogData, cancellationToken);
        dialog.Dispose();

        await Extensibility.Shell().ShowPromptAsync(
            result,
            PromptOptions.OK,
            cancellationToken);
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // Remove invalid characters and split by common separators
        var parts = input.Split(new[] { ' ', '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries);

        var result = new System.Text.StringBuilder();

        foreach (var part in parts)
        {
            if (part.Length == 0)
                continue;

            // Capitalize first letter, lowercase the rest
            result.Append(char.ToUpperInvariant(part[0]));

            if (part.Length > 1)
            {
                result.Append(part.Substring(1).ToLowerInvariant());
            }
        }

        return result.ToString();
    }

    private async Task<string> ExecuteDotnetNewCommand(
        string projectDir, 
        PageNameData pageNameData,
        CancellationToken cancellationToken)
    {
        string pageName = pageNameData.PageName;
        string forceFlag = pageNameData.Force == true ? "--force" : string.Empty;
        string generateLayout = pageNameData.Force == true ? "--GenerateLayout" : string.Empty;
        string embeddedResourceConfig = pageNameData.Force == true ? "--EmbeddedResourceConfig" : string.Empty;

        StringBuilder stringBuilder = new StringBuilder();
        if (!string.IsNullOrEmpty(forceFlag))
        {
            stringBuilder.Append($" {forceFlag}");
        }

        if (!string.IsNullOrEmpty(generateLayout))
        {
            stringBuilder.Append($" {generateLayout}");
        }

        if (!string.IsNullOrEmpty(embeddedResourceConfig))
        {
            stringBuilder.Append($" {embeddedResourceConfig}");
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"new liquidpage --name {pageNameData.PageName}{stringBuilder}",
                WorkingDirectory = projectDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };

            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };

            logger.TraceEvent(TraceEventType.Information, 0,
                $"Executing: dotnet new liquidpage --name {pageName} in {projectDir}");

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            var output = outputBuilder.ToString();
            var error = errorBuilder.ToString();

            if (process.ExitCode == 0)
            {
                logger.TraceEvent(TraceEventType.Information, 0,
                    $"Successfully created Liquid Page: {pageName}");
                return $"✓ Liquid Page '{pageName}' created successfully!\n\nFiles created:\n• {pageName}.liquid\n• {pageName}.liquid.cs";
            }

            logger.TraceEvent(TraceEventType.Error, 0,
                $"Failed to create Liquid Page. Exit code: {process.ExitCode}\nError: {error}");

            // Check if files would be overwritten
            if (error.Contains("already exists") || error.Contains("would overwrite") || output.Contains("already exists"))
            {
                return $"⚠ Files already exist!\n\nThe page '{pageName}' would overwrite existing files.\n\nCheck the 'Force overwrite' option to replace existing files.";
            }

            // Check if template is not installed
            if (error.Contains("No templates found") || error.Contains("liquidpage"))
            {
                return $"⚠ Template not installed!\n\nPlease install the LiquidPages template first:\n\n  dotnet new install Kinetq.LiquidPages.Scaffolder\n\nThen try again.";
            }

            return $"⚠ Failed to create Liquid Page\n\nError: {error}\nOutput: {output}";
        }
        catch (Exception ex)
        {
            logger.TraceEvent(TraceEventType.Error, 0,
                $"Exception while creating Liquid Page: {ex.Message}");
            return $"⚠ Error creating Liquid Page: {ex.Message}";
        }
    }
}