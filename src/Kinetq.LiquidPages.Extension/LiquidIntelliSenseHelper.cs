using System.Text;
using Microsoft.CodeAnalysis;

namespace Kinetq.LiquidPages.Extension
{
    /// <summary>
    /// Helper class to analyze liquid template content and provide model property suggestions.
    /// </summary>
    internal static class LiquidIntelliSenseHelper
    {
        /// <summary>
        /// Determines if a position in the text is inside a liquid expression ({{ }} or {% %}).
        /// </summary>
        public static bool IsInsideLiquidExpression(string text, int position)
        {
            if (position < 0 || position > text.Length)
                return false;

            int openDouble = text.LastIndexOf("{{", position, StringComparison.Ordinal);
            int closeDouble = text.LastIndexOf("}}", position, StringComparison.Ordinal);
            int openTag = text.LastIndexOf("{%", position, StringComparison.Ordinal);
            int closeTag = text.LastIndexOf("%}", position, StringComparison.Ordinal);

            bool insideOutput = openDouble >= 0 && openDouble > closeDouble;
            bool insideTag = openTag >= 0 && openTag > closeTag;

            return insideOutput || insideTag;
        }

        /// <summary>
        /// Extracts the word at a specific position in a line of text.
        /// </summary>
        public static string ExtractWordAt(string line, int position)
        {
            if (position < 0 || position > line.Length) 
                return string.Empty;

            int start = position;
            while (start > 0 && IsWordChar(line[start - 1])) 
                start--;

            int end = position;
            while (end < line.Length && IsWordChar(line[end])) 
                end++;

            return line.Substring(start, end - start);
        }

        /// <summary>
        /// Builds completion items from a model type symbol.
        /// </summary>
        public static List<CompletionItem> BuildCompletionItems(INamedTypeSymbol modelSymbol)
        {
            var items = new List<CompletionItem>();
            var properties = LiquidModelResolver.GetPublicProperties(modelSymbol);

            foreach (var property in properties)
            {
                var snakeCaseName = LiquidModelResolver.ToSnakeCase(property.Name);
                var description = $"({property.Type.ToDisplayString()}) {property.ContainingType.Name}.{property.Name}";

                var xmlDoc = property.GetDocumentationCommentXml();
                if (!string.IsNullOrWhiteSpace(xmlDoc))
                {
                    description += Environment.NewLine + StripXmlTags(xmlDoc);
                }

                items.Add(new CompletionItem
                {
                    Label = snakeCaseName,
                    InsertText = snakeCaseName,
                    Description = description,
                    Kind = CompletionItemKind.Property
                });
            }

            return items;
        }

        private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

        private static string StripXmlTags(string xml)
        {
            // Lightweight - strips XML tags from doc comment output
            return System.Text.RegularExpressions.Regex.Replace(xml, "<[^>]+>", string.Empty).Trim();
        }
    }

    /// <summary>
    /// Represents a completion item for IntelliSense.
    /// </summary>
    internal class CompletionItem
    {
        public string Label { get; set; } = string.Empty;
        public string InsertText { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CompletionItemKind Kind { get; set; }
    }

    /// <summary>
    /// Completion item kind enumeration.
    /// </summary>
    internal enum CompletionItemKind
    {
        Property,
        Method,
        Field,
        Variable
    }
}
