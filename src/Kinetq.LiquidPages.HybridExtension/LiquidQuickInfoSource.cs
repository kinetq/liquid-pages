using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Kinetq.LiquidPages.HybridExtension
{
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("LiquidPages Model QuickInfo")]
    [ContentType("liquid")]
    [Order(Before = "default")]
    internal sealed class LiquidQuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        internal LiquidModelResolver ModelResolver { get; set; } = null;

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
            => new LiquidQuickInfoSource(textBuffer, ModelResolver);
    }

    internal sealed class LiquidQuickInfoSource : IQuickInfoSource
    {
        private readonly ITextBuffer _buffer;
        private readonly LiquidModelResolver _resolver;
        private bool _disposed;

        public LiquidQuickInfoSource(ITextBuffer buffer, LiquidModelResolver resolver)
        {
            _buffer = buffer;
            _resolver = resolver;
        }

        public void AugmentQuickInfoSession(
            IQuickInfoSession session,
            IList<object> quickInfoContent,
            out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;
            if (_disposed) return;

            if (!_buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc))
                return;

            var triggerPoint = session.GetTriggerPoint(_buffer.CurrentSnapshot);
            if (triggerPoint is null) return;

            var line = triggerPoint.Value.GetContainingLine();
            var word = ExtractWordAt(line.GetText(), triggerPoint.Value - line.Start);
            if (string.IsNullOrEmpty(word)) return;

            var modelSymbol = _resolver
                .ResolveModelAsync(doc.FilePath, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            if (modelSymbol is null) return;

            var property = modelSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p =>
                    p.DeclaredAccessibility == Accessibility.Public &&
                    string.Equals(
                        LiquidModelResolver.ToSnakeCase(p.Name),
                        word,
                        StringComparison.OrdinalIgnoreCase));

            if (property is null) return;

            var tooltip = $"(property) {property.Type.ToDisplayString()} " +
                          $"{property.ContainingType.Name}.{property.Name}";

            var xmlDoc = property.GetDocumentationCommentXml();
            if (!string.IsNullOrWhiteSpace(xmlDoc))
                tooltip += Environment.NewLine + StripXmlTags(xmlDoc);

            quickInfoContent.Add(tooltip);

            var wordIndex = IndexOfWord(line.GetText(), triggerPoint.Value - line.Start);
            applicableToSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(
                new SnapshotSpan(line.Start + wordIndex, word.Length),
                SpanTrackingMode.EdgeInclusive);
        }

        private static string ExtractWordAt(string line, int position)
        {
            if (position < 0 || position > line.Length) return string.Empty;

            int start = position;
            while (start > 0 && IsWordChar(line[start - 1])) start--;

            int end = position;
            while (end < line.Length && IsWordChar(line[end])) end++;

            // Replace range operator with Substring for C# 7.3 compatibility
            return line.Substring(start, end - start);
        }

        private static int IndexOfWord(string line, int position)
        {
            int start = position;
            while (start > 0 && IsWordChar(line[start - 1])) start--;
            return start;
        }

        private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

        private static string StripXmlTags(string xml)
        {
            // Very lightweight — strips XML tags from doc comment output
            return System.Text.RegularExpressions.Regex.Replace(xml, "<[^>]+>", string.Empty).Trim();
        }

        public void Dispose() => _disposed = true;
    }
}

