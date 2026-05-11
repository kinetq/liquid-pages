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

        await this.Extensibility.Shell().ShowDialogAsync(dialog, cancellationToken);

        if (!dialog.IsConfirmed || string.IsNullOrWhiteSpace(dialog.PageName))
        {
            this.logger.TraceEvent(TraceEventType.Information, 0,
                "User cancelled page creation");
            return;
        }

        var pageName = dialog.PageName.Trim();

        // Execute dotnet new liquidpage command
        var result = await ExecuteDotnetNewCommand(projectDir, pageName, cancellationToken);

        await Extensibility.Shell().ShowPromptAsync(
            result,
            PromptOptions.OK,
            cancellationToken);
    }

    private async Task<string> ExecuteDotnetNewCommand(string projectDir, string pageName, CancellationToken cancellationToken)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"new liquidpage --name {pageName}",
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

            this.logger.TraceEvent(TraceEventType.Information, 0,
                $"Executing: dotnet new liquidpage --name {pageName} in {projectDir}");

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            var output = outputBuilder.ToString();
            var error = errorBuilder.ToString();

            if (process.ExitCode == 0)
            {
                this.logger.TraceEvent(TraceEventType.Information, 0,
                    $"Successfully created Liquid Page: {pageName}");
                return $"✓ Liquid Page '{pageName}' created successfully!\n\nFiles created:\n• {pageName}.liquid\n• {pageName}.liquid.cs";
            }
            else
            {
                this.logger.TraceEvent(TraceEventType.Error, 0,
                    $"Failed to create Liquid Page. Exit code: {process.ExitCode}\nError: {error}");

                // Check if template is not installed
                if (error.Contains("No templates found") || error.Contains("liquidpage"))
                {
                    return $"⚠ Template not installed!\n\nPlease install the LiquidPages template first:\n\n  dotnet new install Kinetq.LiquidPages.Scaffolder\n\nThen try again.";
                }

                return $"⚠ Failed to create Liquid Page\n\nError: {error}\nOutput: {output}";
            }
        }
        catch (Exception ex)
        {
            this.logger.TraceEvent(TraceEventType.Error, 0,
                $"Exception while creating Liquid Page: {ex.Message}");
            return $"⚠ Error creating Liquid Page: {ex.Message}";
        }
    }
}