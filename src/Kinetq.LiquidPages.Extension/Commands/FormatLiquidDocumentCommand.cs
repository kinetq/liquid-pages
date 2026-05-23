using Kinetq.LiquidPages.Extension.Helpers;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using System.Text;

namespace Kinetq.LiquidPages.Extension.Commands;

/// <summary>
/// Command to format Liquid template files.
/// This command is specifically designed to handle Liquid syntax mixed with HTML.
/// </summary>
[VisualStudioContribution]
public class FormatLiquidDocummentCommand : Command
{
    public override CommandConfiguration CommandConfiguration => new("%LiquidPages.FormatCommand.DisplayName%")
    {
        // Make the command available in the Extensions menu
        Placements = new[]
        {
            CommandPlacement.KnownPlacements.ExtensionsMenu
        },
        // Only enable this command when a liquid file is active
        EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorContentType, "liquid"),
        Shortcuts = new[]
        {
            new CommandShortcutConfiguration(mod1: ModifierKey.ControlShift, key1: Key.X)
        }
    };

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var textView = await context.GetActiveTextViewAsync(cancellationToken);
        if (textView == null) return;

        var documentText = new StringBuilder();
        foreach (var line in textView.Document.Lines)
        {
            string lineText = string.Empty;
            foreach (var character in line.Text)
            {
                lineText += character;
            }

            documentText.AppendLine(lineText);
        }

        string formattedText = await LiquidFormatter.FormatAsync(documentText.ToString(), cancellationToken);
        if (documentText.ToString() == formattedText)
        {
            // No changes needed, skip replacing the document content
            return;
        }

        await Extensibility.Editor().EditAsync(batch =>
        {
            var document = textView.Document.AsEditable(batch);
            // Replace the entire document content with the formatted version
            document.Replace(textView.Document.Text, formattedText);
        }, cancellationToken);
    }   
}
