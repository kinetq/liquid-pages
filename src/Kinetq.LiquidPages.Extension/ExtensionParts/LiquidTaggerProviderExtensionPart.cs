using Kinetq.LiquidPages.Extension.Classification;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace Kinetq.LiquidPages.Extension.ExtensionParts;

/// <summary>
/// Provider for Liquid template language taggers. Creates ClassificationTag taggers
/// that provide syntax highlighting for .liquid files based on the grammar rules
/// from Grammars/liquid.tmLanguage.
/// </summary>
#pragma warning disable VSEXTPREVIEW_TAGGERS // Type is for evaluation purposes only and is subject to change or removal in future updates.
[VisualStudioContribution]
[Experimental("VSEXTPREVIEW_TAGGERS")]
internal class LiquidTaggerProviderExtensionPart : ExtensionPart, ITextViewTaggerProvider<ClassificationTag>, ITextViewChangedListener
{
    private readonly object lockObject = new();
    private readonly Dictionary<Uri, List<LiquidTagger>> _taggers = new();

    /// <summary>
    /// Document type configuration for Liquid template files.
    /// </summary>
    [VisualStudioContribution]
    public static DocumentTypeConfiguration LiquidDocumentType => new("liquid")
    {
        FileExtensions = new[] { ".liquid", ".html.liquid" },
        BaseDocumentType = DocumentType.KnownValues.Code
    };

    public TextViewExtensionConfiguration TextViewExtensionConfiguration => new()
    {
        AppliesTo =
        [
            DocumentFilter.FromDocumentType(LiquidDocumentType)
        ]
    };

    public async Task TextViewChangedAsync(TextViewChangedArgs args, CancellationToken cancellationToken)
    {
        List<Task> tasks = new();
        lock (lockObject)
        {
            if (_taggers.TryGetValue(args.AfterTextView.Uri, out var taggers))
            {
                foreach (var tagger in taggers)
                {
                    tasks.Add(tagger.TextViewChangedAsync(args.AfterTextView, args.Edits, cancellationToken));
                }
            }
        }

        await Task.WhenAll(tasks);
    }

    public Task<TextViewTagger<ClassificationTag>> CreateTaggerAsync(ITextViewSnapshot textView, CancellationToken cancellationToken)
    {
        var tagger = new LiquidTagger(this, textView.Document.Uri);
        lock (lockObject)
        {
            if (!_taggers.TryGetValue(textView.Document.Uri, out var taggers))
            {
                taggers = new();
                _taggers[textView.Document.Uri] = taggers;
            }

            taggers.Add(tagger);
        }

        return Task.FromResult<TextViewTagger<ClassificationTag>>(tagger);
    }

    internal void RemoveTagger(Uri documentUri, LiquidTagger toBeRemoved)
    {
        lock (lockObject)
        {
            if (_taggers.TryGetValue(documentUri, out var taggers))
            {
                taggers.Remove(toBeRemoved);
                if (taggers.Count == 0)
                {
                    _taggers.Remove(documentUri);
                }
            }
        }
    }
}
#pragma warning restore VSEXTPREVIEW_TAGGERS