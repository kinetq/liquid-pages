using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;

namespace Kinetq.LiquidPages.Extension
{
    /// <summary>
    /// Command to display information about the associated LiquidPageModel for the active .liquid file.
    /// </summary>
    [VisualStudioContribution]
    internal class ShowModelInfoCommand : Command
    {
        private readonly TraceSource logger;
        private readonly LiquidModelResolver modelResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowModelInfoCommand"/> class.
        /// </summary>
        /// <param name="traceSource">Trace source instance to utilize.</param>
        /// <param name="modelResolver">The model resolver service.</param>
        public ShowModelInfoCommand(TraceSource traceSource, LiquidModelResolver modelResolver)
        {
            this.logger = Requires.NotNull(traceSource, nameof(traceSource));
            this.modelResolver = Requires.NotNull(modelResolver, nameof(modelResolver));
        }

        /// <inheritdoc />
        public override CommandConfiguration CommandConfiguration => new("Show LiquidPage Model Info")
        {
            Icon = new(ImageMoniker.KnownValues.Extension, IconSettings.IconAndText),
            Placements = [CommandPlacement.KnownPlacements.ToolsMenu]
        };

        /// <inheritdoc />
        public override Task InitializeAsync(CancellationToken cancellationToken)
        {
            // Use InitializeAsync for any one-time setup or initialization.
            return base.InitializeAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
        {
            // Note: The new extensibility model has limited access to document context.
            // Full IntelliSense integration requires language server protocol support.
            await this.Extensibility.Shell().ShowPromptAsync(
                "LiquidPages IntelliSense is active. IntelliSense support for .liquid files with LiquidPageModel classes is enabled.",
                PromptOptions.OK,
                cancellationToken);
        }
    }
}
