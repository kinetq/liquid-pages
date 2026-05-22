using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.LanguageServer;

namespace Kinetq.LiquidPages.Extension;

public static class LiquidDocumentTypeConfiguration
{
    /// <summary>
    /// Document type configuration for Liquid template files.
    /// Inherits from HTML to get HTML syntax highlighting as the base.
    /// </summary>
    [VisualStudioContribution]
    [Experimental("VSEXTPREVIEW_LSP")]
    public static DocumentTypeConfiguration LiquidDocumentType => new("liquid")
    {
        FileExtensions = new[] { ".liquid" },
        BaseDocumentType = LanguageServerProvider.LanguageServerBaseDocumentType
    };
}