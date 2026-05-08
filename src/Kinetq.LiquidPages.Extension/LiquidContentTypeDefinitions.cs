using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Kinetq.LiquidPages.Extension;

internal static class LiquidContentTypeDefinitions
{
    // Define a new content type named "liquid" that inherits from "html",
    // so it automatically gets all HTML syntax highlighting.
    [Export]
    [Name("liquid")]
    [BaseDefinition("html")]
    internal static ContentTypeDefinition LiquidContentTypeDefinition = null;

    // Map the .liquid file extension to the "liquid" content type.
    [Export]
    [FileExtension(".liquid")]
    [ContentType("liquid")]
    internal static FileExtensionToContentTypeDefinition LiquidFileExtensionDefinition = null;
}