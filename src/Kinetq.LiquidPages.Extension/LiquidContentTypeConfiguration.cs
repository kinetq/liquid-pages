using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Kinetq.LiquidPages.Extension;

public class LiquidContentTypeConfiguration
{
    [Export]
    [Name("liquid")]   // Tells the editor, "This content is of type 'liquid'"
    [BaseDefinition("htmlx")] // "And it is based on the 'htmlx' content type."
    internal static ContentTypeDefinition LiquidContentType { get; set; }

    [Export]
    [FileExtension(".liquid")]
    [ContentType("liquid")] // Maps the .liquid file extension to the 'liquid' content type.
    internal static FileExtensionToContentTypeDefinition LiquidFileExtension { get; set; }
}