using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Kinetq.LiquidPages.HybridExtension
{
    [Export(typeof(ICompletionSourceProvider))]
    [Name("LiquidPages Model Completion")]
    [ContentType("liquid")]
    [Order(Before = "default")]
    internal sealed class LiquidCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import] internal LiquidModelResolver ModelResolver { get; set; } = null;

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
            => new LiquidCompletionSource(textBuffer, ModelResolver);
    }

    internal sealed class LiquidCompletionSource : ICompletionSource
    {
        private readonly ITextBuffer _buffer;
        private readonly LiquidModelResolver _resolver;
        private bool _disposed;

        public LiquidCompletionSource(ITextBuffer buffer, LiquidModelResolver resolver)
        {
            _buffer = buffer;
            _resolver = resolver;
        }

        public void AugmentCompletionSession(
            ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (_disposed) return;

            if (!_buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc))
                return;

            var triggerPoint = session.GetTriggerPoint(_buffer.CurrentSnapshot);
            if (triggerPoint is null || !IsInsideLiquidExpression(triggerPoint.Value))
                return;

            var modelSymbol = _resolver
                .ResolveModelAsync(doc.FilePath, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            if (modelSymbol is null) return;

            var icon = GetMonikerImage(KnownMonikers.Method, 16, 16);
            var completions = BuildCompletions(modelSymbol, icon);
            if (completions.Count == 0) return;

            var trackingSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(
                triggerPoint.Value.Position, 0, SpanTrackingMode.EdgeInclusive);

            completionSets.Add(new CompletionSet(
                moniker: "liquidPageModel",
                displayName: "Model",
                applicableTo: trackingSpan,
                completions: completions,
                completionBuilders: Array.Empty<Completion>()));
        }

        /// <summary>
        /// Retrieves a WPF <see cref="ImageSource"/> for the given <see cref="ImageMoniker"/>
        /// via the Visual Studio image service.
        /// </summary>
        private static ImageSource GetMonikerImage(ImageMoniker moniker, int width, int height)
        {
            var imageService = Package.GetGlobalService(typeof(SVsImageService)) as IVsImageService2;
            if (imageService == null) return null;

            var attributes = new ImageAttributes
            {
                StructSize    = Marshal.SizeOf(typeof(ImageAttributes)),
                ImageType     = (uint)_UIImageType.IT_Bitmap,
                Format        = (uint)_UIDataFormat.DF_WPF,
                LogicalWidth  = width,
                LogicalHeight = height,
                Flags         = (uint)_ImageAttributesFlags.IAF_RequiredFlags
            };

            IVsUIObject uiObject = imageService.GetImage(moniker, attributes);
            if (uiObject == null) return null;

            uiObject.get_Data(out object data);
            return data as ImageSource;
        }

        private static List<Completion> BuildCompletions(INamedTypeSymbol modelSymbol, ImageSource icon)
        {
            return modelSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
                .Select(p => new Completion(
                    displayText: LiquidModelResolver.ToSnakeCase(p.Name),
                    insertionText: LiquidModelResolver.ToSnakeCase(p.Name),
                    description: $"({p.Type.ToDisplayString()}) {p.ContainingType.Name}.{p.Name}",
                    iconSource: icon,
                    iconAutomationText: null))
                .ToList();
        }

        /// <summary>
        /// Returns true when the caret sits inside a <c>{{ }}</c> or <c>{% %}</c> block.
        /// </summary>
        private static bool IsInsideLiquidExpression(SnapshotPoint point)
        {
            var snapshot = point.Snapshot;
            var text = snapshot.GetText();
            var pos = point.Position;

            int openDouble = text.LastIndexOf("{{", pos, StringComparison.Ordinal);
            int closeDouble = text.LastIndexOf("}}", pos, StringComparison.Ordinal);
            int openTag = text.LastIndexOf("{%", pos, StringComparison.Ordinal);
            int closeTag = text.LastIndexOf("%}", pos, StringComparison.Ordinal);

            bool insideOutput = openDouble > closeDouble;
            bool insideTag = openTag > closeTag;

            return insideOutput || insideTag;
        }

        public void Dispose() => _disposed = true;
    }
}