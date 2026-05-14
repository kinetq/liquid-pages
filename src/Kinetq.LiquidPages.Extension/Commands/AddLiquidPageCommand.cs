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
using Kinetq.LiquidPages.Extension.Helpers;

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

        var solutions =
            await Extensibility.Workspaces()
                .QuerySolutionAsync(snapshots => snapshots.With(s => new { s.Path }), cancellationToken);

        // There is always exactly one ISolutionSnapshot if a solution is open.
        var solution = solutions.FirstOrDefault();
        if (solution is null)
        {
            // No solution is open (e.g., Open Folder mode).
            return;
        }

        var solutionDir = Path.GetDirectoryName(solution.Path);
        if (string.IsNullOrEmpty(solutionDir))
        {
            await Extensibility.Shell().ShowPromptAsync(
                "Unable to determine solution directory.",
                PromptOptions.OK,
                cancellationToken);
            return;
        }

        // Show dialog to get page name from user
        var dialog = new AddLiquidPageDialogControl();

        // Start showing the dialog (non-blocking)
        var dialogResult = await Extensibility.Shell().ShowDialogAsync(dialog, DialogOption.OKCancel, cancellationToken);
        if (dialogResult == DialogResult.Cancel)
        {
            logger.TraceEvent(TraceEventType.Information, 0,
                "User cancelled page creation");

            return;
        }

        // Convert to PascalCase
        AddLiquidPageData dialogData = (AddLiquidPageData)dialog.DataContext;
        var pageName = dialogData.PageName.ToPascalCase();

        logger.TraceEvent(TraceEventType.Information, 0,
            $"Creating page with name: {pageName}");

        // Execute dotnet new liquidpage command
        var result = await ExecuteDotnetNewCommand(projectDir, solutionDir, dialogData, cancellationToken);
        dialog.Dispose();

        await Extensibility.Shell().ShowPromptAsync(
            result,
            PromptOptions.OK,
            cancellationToken);
    }

    private async Task<string> ExecuteDotnetNewCommand(
        string projectDir,
        string solutionDir,
        AddLiquidPageData pageNameData,
        CancellationToken cancellationToken)
    {
        string forceFlag = pageNameData.Force == true ? "--force" : string.Empty;

        try
        {
            var configStringBuilder = new StringBuilder();
            // Check if .editorconfig already exists at solution root
            var vsWorkspaceSettingsPath = Path.Combine(solutionDir, ".vs", "VSWorkspaceSettings.json");
            bool vsWorkspaceSettingsExists = File.Exists(vsWorkspaceSettingsPath);
            if (!vsWorkspaceSettingsExists)
            {
                configStringBuilder.Append(" --CreateWorkspaceSettings");
            }

            // Check if .filenesting.json already exists at solution root
            var fileNestingPath = Path.Combine(solutionDir, ".filenesting.json");
            bool fileNestingExists = File.Exists(fileNestingPath);

            // Only create .filenesting.json if it doesn't exist (prevents overwriting user config)
            if (!fileNestingExists)
            {
                configStringBuilder.Append(" --CreateFileNesting");
            }

            string arguments = $"new liquidpageconfig {configStringBuilder}";
            var result = await StartProcess(solutionDir, arguments, cancellationToken);

            if (result.Item1 == 0)
            {
                logger.TraceEvent(TraceEventType.Information, 0,
                    $"Successfully adding configuration files for LiquidPages");

                if (!fileNestingExists)
                {
                    await Extensibility.Workspaces().UpdateSolutionAsync(
                        // The query function selects all solutions (the only one open)
                        query => query,
                        // The update function applies the AddFile operations
                        update => update
                            .AddFile(fileNestingPath),
                        cancellationToken);
                }
            }
            else
            {
                logger.TraceEvent(TraceEventType.Error, 0,
                                $"Failed to add configuration files. Exit code: {result.Item1}\nError: {result.Item3}");

                return $"⚠ Failed to add configuration files for Liquid Pages\n\nError: {result.Item3}\nOutput: {result.Item2}";
            }
        }
        catch (Exception ex)
        {
            logger.TraceEvent(TraceEventType.Error, 0,
                $"Exception while creating Liquid Page: {ex.Message}");
            return $"⚠ Error creating Liquid Page: {ex.Message}";
        }

        try
        {
            string pageName = pageNameData.PageName;
            string generateLayout = pageNameData.GenerateLayout == true ? "--GenerateLayout" : string.Empty;

            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(forceFlag))
            {
                stringBuilder.Append($" {forceFlag}");
            }

            if (!string.IsNullOrEmpty(generateLayout))
            {
                stringBuilder.Append($" {generateLayout}");
            }

            string arguments = $"new liquidpage --name {pageNameData.PageName}{stringBuilder}";
            var result = await StartProcess(projectDir, arguments, cancellationToken);

            if (result.Item1 == 0)
            {
                logger.TraceEvent(TraceEventType.Information, 0,
                    $"Successfully created Liquid Page: {pageName}");
                return $"✓ Liquid Page '{pageName}' created successfully!\n\nFiles created:\n• {pageName}.liquid\n• {pageName}.liquid.cs";
            }

            logger.TraceEvent(TraceEventType.Error, 0,
                $"Failed to create Liquid Page. Exit code: {result.Item1}\nError: {result.Item3}");

            return $"⚠ Failed to create Liquid Page\n\nError: {result.Item3}\nOutput: {result.Item2}";
        }
        catch (Exception ex)
        {
            logger.TraceEvent(TraceEventType.Error, 0,
                $"Exception while creating Liquid Page: {ex.Message}");
            return $"⚠ Error creating Liquid Page: {ex.Message}";
        }
    }

    private async Task<(int, string, string)> StartProcess(string workingDirectory, string arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

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

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        return (process.ExitCode, output, error);
    }
}