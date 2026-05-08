using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Kinetq.LiquidPages.HybridExtension
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
