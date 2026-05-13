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
                version: new Version(1, 1),
                publisherName: "Kinetq",
                displayName: "LiquidPages Extension",
                description: "A Visual Studio extension that provides enhanced support for Liquid template files in your .NET projects."),
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);
    }
}
