namespace Kinetq.LiquidPages.Extension.Commands;

using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.ProjectSystem.Query;
using System.Diagnostics;

[VisualStudioContribution]
internal class ApplyFileNestingCommand : Command
{
    private readonly TraceSource logger;
    public ApplyFileNestingCommand(VisualStudioExtensibility extensibility, TraceSource traceSource)
        : base(extensibility)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    public override CommandConfiguration CommandConfiguration => new("Apply LiquidPages File Nesting")
    {
        Icon = new(ImageMoniker.KnownValues.Extension, IconSettings.IconAndText),
        Placements =
        [
            CommandPlacement.VsctParent(
                new Guid("{d309f791-903f-11d0-9efc-00a0c911004f}"), // guidSHLMainMenu
                id: 1072,   // IDM_VS_CTXT_PROJNODE_ADD
                priority: 0)
        ]
    };

    // ... ExecuteCommandAsync method remains the same as before ...
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        // Copy .filenesting.json to project
        var extensionDir = Path.GetDirectoryName(typeof(InstallItemTemplateCommand).Assembly.Location);
        var successMessage = await CopyFileNestingToProject(extensionDir!, cancellationToken);

        await Extensibility.Shell().ShowPromptAsync(
            successMessage,
            PromptOptions.OK,
            cancellationToken);
    }

    private async Task<string> CopyFileNestingToProject(string extensionDir, CancellationToken cancellationToken)
    {
        try
        {
            // Get source .filenesting.json file
            var sourceFileNesting = Path.Combine(extensionDir, ".filenesting.json");
            if (!File.Exists(sourceFileNesting))
            {
                this.logger.TraceEvent(TraceEventType.Warning, 0,
                    $".filenesting.json not found at: {sourceFileNesting}");
                return "";
            }

            // Get active project
            var project = await this.Extensibility.Workspaces().QueryProjectsAsync(
                project => project.With(p => p.Path),
                cancellationToken);

            var activeProject = project.FirstOrDefault();
            if (activeProject?.Path == null)
            {
                this.logger.TraceEvent(TraceEventType.Warning, 0,
                    "No active project found for .filenesting.json copy");
                return "";
            }

            var projectDir = Path.GetDirectoryName(activeProject.Path);
            if (string.IsNullOrEmpty(projectDir))
            {
                return "";
            }

            var targetFileNesting = Path.Combine(projectDir, ".filenesting.json");

            // Check if file already exists
            if (File.Exists(targetFileNesting))
            {
                var overwrite = await this.Extensibility.Shell().ShowPromptAsync(
                    $".filenesting.json already exists in your project.\n\nDo you want to overwrite it?",
                    PromptOptions.OKCancel,
                    cancellationToken);

                if (!overwrite)
                {
                    return "✓ .filenesting.json was not overwritten";
                }
            }

            // Copy the file
            File.Copy(sourceFileNesting, targetFileNesting, true);

            this.logger.TraceEvent(TraceEventType.Information, 0,
                $"Copied .filenesting.json to: {targetFileNesting}");

            return $"✓ .filenesting.json copied to project:\n{targetFileNesting}";
        }
        catch (Exception ex)
        {
            this.logger.TraceEvent(TraceEventType.Warning, 0,
                $"Failed to copy .filenesting.json: {ex.Message}");
            return $"⚠ Could not copy .filenesting.json: {ex.Message}";
        }
    }
}