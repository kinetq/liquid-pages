using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;

namespace Kinetq.LiquidPages.HybridExtension;

/// <summary>
/// Defines the Liquid document type for .liquid files.
/// </summary>
internal static class LiquidDocumentType
{
    /// <summary>
    /// Document type configuration for Liquid template files.
    /// </summary>
    [VisualStudioContribution]
    internal static DocumentTypeConfiguration LiquidDocumentTypeDefinition => new("Liquid")
    {
        FileExtensions = new[] { ".liquid" },
        BaseDocumentType = DocumentType.KnownValues.Text,
    };
}
