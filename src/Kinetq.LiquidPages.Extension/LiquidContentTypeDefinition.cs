using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Kinetq.LiquidPages.Extension
{
    internal static class LiquidContentTypeDefinition
    {
        [Export]
        [Name("liquid")]
        [BaseDefinition("HTML")]
        internal static ContentTypeDefinition LiquidContentType;

        [Export]
        [FileExtension(".liquid")]
        [ContentType("liquid")]
        internal static FileExtensionToContentTypeDefinition LiquidFileExtension;
    }
}
