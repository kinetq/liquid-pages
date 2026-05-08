using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace Kinetq.LiquidPages.Extension
{
    internal static class LiquidContentTypeDefinition
    {
        [Export]
        [Name("liquid")]
        [BaseDefinition("htmlx")]
        public static ContentTypeDefinition LiquidContentType { get; set; }

        [Export]
        [FileExtension(".liquid")]
        [ContentType("liquid")]
        public static FileExtensionToContentTypeDefinition LiquidFileExtension { get; set; }
    }
}
