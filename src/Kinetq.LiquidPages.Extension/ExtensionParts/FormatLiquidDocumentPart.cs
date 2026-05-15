using Kinetq.LiquidPages.Extension.Helpers;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;

namespace Kinetq.LiquidPages.Extension.ExtensionParts;

/// <summary>
/// Command to format Liquid template files.
/// This command is specifically designed to handle Liquid syntax mixed with HTML.
/// </summary>
[VisualStudioContribution]
public class FormatLiquidDocumentExtensionPart : ExtensionPart, ITextViewChangedListener
{
    public TextViewExtensionConfiguration TextViewExtensionConfiguration => new()
    {
        AppliesTo =
        [
            DocumentFilter.FromDocumentType(LiquidDocumentTypeConfiguration.LiquidDocumentType)
        ]
    };
    public async Task TextViewChangedAsync(TextViewChangedArgs args, CancellationToken cancellationToken)
    {
        var textView = args.AfterTextView;
        if (textView == null) return;

        await Extensibility.Editor().EditAsync(async batch =>
        {
            var document = textView.Document.AsEditable(batch);
            var documentText = textView.Document.Text.ToString();
            string formattedText = await LiquidFormatter.FormatAsync(documentText, cancellationToken);

            // Replace the entire document content with the formatted version
            document.Replace(textView.Document.Text, formattedText);
        }, cancellationToken);
    }
}