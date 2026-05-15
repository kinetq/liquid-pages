using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Kinetq.LiquidPages.Extension.LanguageService;

/// <summary>
/// MEF-based content type definition for Liquid files.
/// This is required to properly register the Liquid content type with Visual Studio's
/// editor infrastructure and enable formatting support.
/// </summary>
internal static class LiquidContentTypeDefinition
{
    /// <summary>
    /// Defines the "liquid" content type as a derivative of "htmlx".
    /// The HTMLX content type provides full HTML editing capabilities including:
    /// - Formatting (Ctrl+K, Ctrl+D)
    /// - Syntax highlighting
    /// - IntelliSense
    /// - Auto-closing tags
    /// - Brace matching
    /// </summary>
    [Export]
    [Name("liquid")]
    [BaseDefinition("htmlx")]
    internal static ContentTypeDefinition? LiquidContentType;

    /// <summary>
    /// Associates the .liquid file extension with the liquid content type.
    /// </summary>
    [Export]
    [FileExtension(".liquid")]
    [ContentType("liquid")]
    internal static FileExtensionToContentTypeDefinition? LiquidFileExtension;
}
