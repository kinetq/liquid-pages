using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Editor;

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
            CommandPlacement.KnownPlacements.ExtensionsMenu,
        },
        // Only enable this command when a liquid file is active
        EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorContentType, "liquid")
    };

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var textView = await context.GetActiveTextViewAsync(cancellationToken);
        if (textView == null) return;

        await context.Extensibility.Editor().EditAsync(batch =>
        {
            var document = textView.Document.AsEditable(batch);
            var documentText = textView.Document.Text.ToString();
            string formattedText = MyFormattingLogic(documentText);

            // Replace the entire document content with the formatted version
            document.Replace(textView.Document.Text, formattedText);
        }, cancellationToken);
    }

    private string MyFormattingLogic(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return content;

        var lines = new List<string>();
        var currentIndent = 0;
        const string indentString = "    "; // 4 spaces
        var i = 0;
        var result = new System.Text.StringBuilder();

        while (i < content.Length)
        {
            // Skip whitespace
            while (i < content.Length && char.IsWhiteSpace(content[i]))
                i++;

            if (i >= content.Length) break;

            // Check for Liquid or HTML tag
            if (content[i] == '<' || (content[i] == '{' && i + 1 < content.Length && (content[i + 1] == '%' || content[i + 1] == '{' || content[i + 1] == '#')))
            {
                var tagStart = i;
                var isClosingTag = false;
                var isSelfClosing = false;
                var isLiquidTag = content[i] == '{';

                // Read the full tag
                if (isLiquidTag)
                {
                    // Handle Liquid tags: {%, {{, {#
                    while (i < content.Length && !(content[i] == '}' && i > 0 && (content[i - 1] == '%' || content[i - 1] == '}' || content[i - 1] == '#')))
                        i++;
                    if (i < content.Length) i++; // Skip the final }

                    var tag = content.Substring(tagStart, i - tagStart);
                    isClosingTag = tag.Contains("end") || tag.Contains("else") || tag.Contains("elsif") || tag.Contains("elif") || tag.Contains("when");

                    if (isClosingTag || tag.Contains("else") || tag.Contains("elsif") || tag.Contains("elif") || tag.Contains("when"))
                        currentIndent = Math.Max(0, currentIndent - 1);

                    result.Append(new string(' ', currentIndent * indentString.Length));
                    result.AppendLine(tag);

                    if (!isClosingTag && (tag.Contains("if") || tag.Contains("for") || tag.Contains("block") || tag.Contains("case") || tag.Contains("unless") || tag.Contains("capture") || tag.Contains("tablerow")))
                        currentIndent++;
                    else if (tag.Contains("else") || tag.Contains("elsif") || tag.Contains("elif") || tag.Contains("when"))
                        currentIndent++;
                }
                else
                {
                    // Handle HTML tags
                    i++; // Skip <
                    isClosingTag = content[i] == '/';
                    if (isClosingTag) i++;

                    while (i < content.Length && content[i] != '>')
                    {
                        if (content[i] == '/' && i + 1 < content.Length && content[i + 1] == '>')
                            isSelfClosing = true;
                        i++;
                    }
                    if (i < content.Length) i++; // Skip >

                    var tag = content.Substring(tagStart, i - tagStart);

                    if (isClosingTag)
                        currentIndent = Math.Max(0, currentIndent - 1);

                    result.Append(new string(' ', currentIndent * indentString.Length));
                    result.AppendLine(tag);

                    if (!isClosingTag && !isSelfClosing && !tag.Contains("br") && !tag.Contains("hr") && !tag.Contains("img") && !tag.Contains("input") && !tag.Contains("meta") && !tag.Contains("link"))
                        currentIndent++;
                }
            }
            else
            {
                // Handle text content
                var textStart = i;
                while (i < content.Length && content[i] != '<' && content[i] != '{')
                    i++;

                var text = content.Substring(textStart, i - textStart).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    result.Append(new string(' ', currentIndent * indentString.Length));
                    result.AppendLine(text);
                }
            }
        }

        return result.ToString().TrimEnd();
    }
}