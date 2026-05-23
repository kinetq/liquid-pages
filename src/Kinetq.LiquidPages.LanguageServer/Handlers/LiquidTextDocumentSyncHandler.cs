using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using System.Collections.Concurrent;

namespace Kinetq.LiquidPages.LanguageServer.Handlers;

/// <summary>
/// Handles text document sync and maintains an in-memory buffer of document contents
/// </summary>
public class LiquidTextDocumentSyncHandler : TextDocumentSyncHandlerBase
{
    private readonly ConcurrentDictionary<DocumentUri, string> _documents = new();

    public static readonly TextDocumentSelector DocumentSelector =
        TextDocumentSelector.ForLanguage("liquid");

    public bool TryGetContent(DocumentUri uri, out string content) =>
        _documents.TryGetValue(uri, out content!);

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) =>
        new(uri, "liquid");

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        _documents[request.TextDocument.Uri] = request.TextDocument.Text;
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        var changes = request.ContentChanges.ToList();
        if (changes.Count > 0)
        {
            // Full sync: take the last change's text
            _documents[request.TextDocument.Uri] = changes[^1].Text;
        }

        return Unit.Task;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken) =>
        Unit.Task;

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        _documents.TryRemove(request.TextDocument.Uri, out _);
        return Unit.Task;
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
        TextSynchronizationCapability capability,
        ClientCapabilities clientCapabilities) =>
        new()
        {
            DocumentSelector = DocumentSelector,
            Change = TextDocumentSyncKind.Full,
            Save = new SaveOptions { IncludeText = false }
        };
}
