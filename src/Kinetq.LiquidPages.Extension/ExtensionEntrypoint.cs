using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace Kinetq.LiquidPages.Extension;

/// <summary>
/// Extension entrypoint for the LiquidPages IntelliSense extension.
/// </summary>
[VisualStudioContribution]
internal class ExtensionEntrypoint : Microsoft.VisualStudio.Extensibility.Extension
{
    /// <inheritdoc/>
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
                id: "20a2d220-c724-408b-8c3c-09866f045087",
                version: new Version(1, 2),
                publisherName: "Kinetq",
                displayName: "LiquidPages Extension",
                description: "Enhanced Visual Studio support for Liquid template files in .NET projects. Designed for Kinetq.LiquidPages, a framework that brings Liquid templates to .NET while emulating RazorPages patterns. Features include syntax highlighting for .liquid files with HTML base support, and convenient quick commands (Add LiquidPage, Add LiquidErrorPage) accessible from the project context menu to streamline template creation.")
        {
            Icon = "Images/Logo32x32.png",
        },
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);
    }
}
