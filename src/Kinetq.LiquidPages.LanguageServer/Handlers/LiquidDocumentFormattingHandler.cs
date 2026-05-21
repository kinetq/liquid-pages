using Kinetq.LiquidPages.LanguageServer.Helpers;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Kinetq.LiquidPages.LanguageServer.Handlers;

/// <summary>
/// Handles document formatting requests for Liquid files
/// </summary>
public class LiquidDocumentFormattingHandler : IDocumentFormattingHandler
{
    public async Task<TextEditContainer?> Handle(DocumentFormattingParams request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the document text
            var documentUri = request.TextDocument.Uri;
            var documentPath = documentUri.GetFileSystemPath();

            if (string.IsNullOrEmpty(documentPath) || !File.Exists(documentPath))
            {
                return null;
            }

            var content = await File.ReadAllTextAsync(documentPath, cancellationToken);

            // Format the content
            var formattedContent = await LiquidFormatter.FormatAsync(content, cancellationToken);

            // If content hasn't changed, return null
            if (content == formattedContent)
            {
                return null;
            }

            // Calculate line count for the range
            var lines = content.Split(["\r\n", "\n"], StringSplitOptions.None);
            var endLine = Math.Max(0, lines.Length - 1);
            var endCharacter = lines.Length > 0 ? lines[^1].Length : 0;

            // Return a text edit that replaces the entire document
            var textEdit = new TextEdit
            {
                Range = new LspRange
                {
                    Start = new Position(0, 0),
                    End = new Position(endLine, endCharacter)
                },
                NewText = formattedContent
            };

            return new TextEditContainer(textEdit);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public DocumentFormattingRegistrationOptions GetRegistrationOptions(DocumentFormattingCapability capability, ClientCapabilities clientCapabilities)
    {
        return new DocumentFormattingRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("liquid")
        };
    }
}
