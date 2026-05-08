using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Extensibility.Editor;
using Kinetq.LiquidPages.Extension.ExtensionParts;

namespace Kinetq.LiquidPages.Extension.Classification;

/// <summary>
/// Tagger that provides classification tags for Liquid template syntax.
/// Implements all grammar rules from Grammars/liquid.tmLanguage in the new VS Extensibility model.
/// </summary>
#pragma warning disable VSEXTPREVIEW_TAGGERS // Type is for evaluation purposes only and is subject to change or removal in future updates.
[Experimental("VSEXTPREVIEW_TAGGERS")]
internal class LiquidTagger : TextViewTagger<ClassificationTag>
{
    private readonly LiquidTaggerProviderExtensionPart provider;
    private readonly Uri documentUri;

    // Regex patterns matching liquid.tmLanguage grammar rules

    // Front matter: ---...--- (at document start)
    private static readonly Regex FrontMatterRegex = new(@"^---\r?\n.*?\r?\n---\r?\n", RegexOptions.Singleline | RegexOptions.Compiled);

    // Comment blocks: {% comment %}...{% endcomment %}
    private static readonly Regex CommentBlockRegex = new(@"\{%\s*comment\s*%\}.*?\{%\s*endcomment\s*%\}", RegexOptions.Singleline | RegexOptions.Compiled);

    // Print tags: {{...}} or {{-...-}}
    private static readonly Regex PrintTagRegex = new(@"\{\{-?(?<content>.*?)-?\}\}", RegexOptions.Singleline | RegexOptions.Compiled);

    // Statement tags: {%...%} or {%-...-%}
    private static readonly Regex StatementTagRegex = new(@"\{%-?(?<content>.*?)-?%\}", RegexOptions.Singleline | RegexOptions.Compiled);

    // Keywords (control flow, loops, includes, etc.)
    private static readonly Regex KeywordRegex = new(@"\b(if|endif|unless|endunless|elsif|else|for|endfor|in|break|continue|case|endcase|when|capture|endcapture|raw|endraw|comment|endcomment|paginate|endpaginate|form|endform|tablerow|endtablerow|highlight|endhighlight|include|include_relative|with|cycle|layout|by|assign|increment|decrement|link|post_url|gist)\b", RegexOptions.Compiled);

    // Constants - Language (true, false, nil)
    private static readonly Regex ConstantLanguageRegex = new(@"\b(true|false|nil)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Constants - Numeric
    private static readonly Regex ConstantNumericRegex = new(@"\b\d+(?:\.\d+)?\b", RegexOptions.Compiled);

    // Operators
    private static readonly Regex ComparisonOperatorRegex = new(@"(==|!=|<=?|>=?|contains)\b", RegexOptions.Compiled);
    private static readonly Regex LogicalOperatorRegex = new(@"\b(and|or)\b", RegexOptions.Compiled);
    private static readonly Regex AssignmentOperatorRegex = new(@"(=|~)", RegexOptions.Compiled);
    private static readonly Regex RangeOperatorRegex = new(@"\.\.", RegexOptions.Compiled);
    private static readonly Regex FilterOperatorRegex = new(@"\|", RegexOptions.Compiled);

    // Filters (after pipe operator) - comprehensive list from liquid.tmLanguage
    private static readonly Regex FilterRegex = new(@"\|\s*(?<filter>join|first|last|concat|index|map|reverse|size|sort|uniq|img_tag|script_tag|stylesheet_tag|abs|ceil|divided_by|floor|minus|plus|round|times|modulo|money(?:_with_currency|_without_trailing_zeros|_without_currency)?|append|prepend|capitalize|pluralize|handleize|camelcase|downcase|upcase|(?:hmac_)?sha(?:1|256)|remove(?:_first)?|replace(?:_first)?|lstrip|rstrip|strip(?:_html|_newlines)?|truncate(?:words)?|url_(?:encode|(?:param_)?escape)|md5|newline_to_br|slice|split|(?:file_url|(?:global_|shopify_)?asset_url|(?:payment_type_|product_|collection_|file_|asset_)?img_url)|within|hex_to_rgba|json|weight_with_unit|customer_login_link|link_to(?:_vendor|_type|(?:_add|_remove)_tag)?|url_for_(?:type|vendor)|default(?:_errors|_pagination)?|highlight(?:_active_tag)?|(?:json|markdown|slug|smart|scss|sass)ify|date(?:_to_(?:xmlschema|rfc822|(?:long_)?string))?|where(?:_exp)?|(?:xml_|cgi_|uri_)?escape|group_by|number_of_words|array_to_sentence_string|normalize_whitespace|sample|to_integer|inspect|push|pop|(?:un)?shift)\b", RegexOptions.Compiled);

    // Properties - dot notation: object.property
    private static readonly Regex PropertyDotRegex = new(@"(?<=[a-zA-Z0-9_\]])\.(?<property>[a-zA-Z_][a-zA-Z0-9_]*)\b", RegexOptions.Compiled);

    // Properties - array notation: object['property'] or object["property"] or object[property]
    private static readonly Regex PropertyArrayRegex = new(@"\[(?<quote>['""]?)(?<property>[a-zA-Z_][a-zA-Z0-9_]*)\k<quote>\]", RegexOptions.Compiled);

    // Variables/Objects
    private static readonly Regex VariableRegex = new(@"\b(?<variable>[a-zA-Z_][a-zA-Z0-9_]*)\b", RegexOptions.Compiled);

    // Strings
    private static readonly Regex StringSingleRegex = new(@"'(?:[^'\\]|\\.)*'", RegexOptions.Compiled);
    private static readonly Regex StringDoubleRegex = new(@"""(?:[^""\\]|\\.)*""", RegexOptions.Compiled);

    // Arrays
    private static readonly Regex ArrayRegex = new(@"\[(?<content>[^\]]*)\]", RegexOptions.Compiled);

    // Punctuation
    private static readonly Regex TagDelimiterRegex = new(@"(\{\{-?|-?\}\}|\{%-?|-?%\})", RegexOptions.Compiled);
    private static readonly Regex ParenthesesRegex = new(@"[()]", RegexOptions.Compiled);

    public LiquidTagger(LiquidTaggerProviderExtensionPart provider, Uri documentUri)
    {
        this.provider = provider;
        this.documentUri = documentUri;
    }

    public override void Dispose()
    {
        this.provider.RemoveTagger(this.documentUri, this);
        base.Dispose();
    }

    /// <summary>
    /// Called when text view changes. Updates tags for affected ranges.
    /// </summary>
    public async Task TextViewChangedAsync(ITextViewSnapshot textView, IReadOnlyList<TextEdit> edits, CancellationToken cancellationToken)
    {
        if (edits.Count == 0)
        {
            return;
        }

        // Get previously requested ranges and intersect with edited ranges
        var allRequestedRanges = await GetAllRequestedRangesAsync(textView.Document, cancellationToken);
        await CreateTagsAsync(
            textView.Document,
            allRequestedRanges.Intersect(
                edits.Select(e =>
                    EnsureNotEmpty(
                        e.Range.TranslateTo(textView.Document, TextRangeTrackingMode.ExtendForwardAndBackward)))));
    }

    protected override async Task RequestTagsAsync(NormalizedTextRangeCollection requestedRanges, bool recalculateAll, CancellationToken cancellationToken)
    {
        if (requestedRanges.Count == 0)
        {
            return;
        }

        await CreateTagsAsync(requestedRanges.TextDocumentSnapshot!, requestedRanges);
    }

    private static TextRange EnsureNotEmpty(TextRange range)
    {
        if (range.Length > 0)
        {
            return range;
        }

        int start = Math.Max(0, range.Start - 1);
        int end = Math.Min(range.Document.Length, range.Start + 1);

        return new(range.Document, start, end - start);
    }

    private async Task CreateTagsAsync(ITextDocumentSnapshot document, IEnumerable<TextRange> requestedRanges)
    {
        List<TaggedTrackingTextRange<ClassificationTag>> tags = new();
        List<TextRange> ranges = new();

        // Process each requested range
        foreach (var range in requestedRanges)
        {
            var text = range.CopyToString();
            var rangeStart = range.Start;

            // Track classified positions to avoid overlaps
            var classifiedRanges = new List<(int start, int end)>();

            // 1. Front matter (only at document start)
            if (rangeStart == 0)
            {
                ProcessFrontMatter(document, text, rangeStart, tags, classifiedRanges);
            }

            // 2. Comment blocks (highest priority - skip everything inside)
            ProcessCommentBlocks(document, text, rangeStart, tags, classifiedRanges);

            // 3. Print tags {{...}}
            ProcessPrintTags(document, text, rangeStart, tags, classifiedRanges);

            // 4. Statement tags {%...%}
            ProcessStatementTags(document, text, rangeStart, tags, classifiedRanges);

            ranges.Add(range);
        }

        await UpdateTagsAsync(ranges, tags, CancellationToken.None);
    }

    private void ProcessFrontMatter(ITextDocumentSnapshot document, string text, int rangeStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> classifiedRanges)
    {
        var match = FrontMatterRegex.Match(text);
        if (match.Success)
        {
            AddTag(tags, document, rangeStart + match.Index, match.Length, ClassificationType.KnownValues.Comment);
            classifiedRanges.Add((match.Index, match.Index + match.Length));
        }
    }

    private void ProcessCommentBlocks(ITextDocumentSnapshot document, string text, int rangeStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> classifiedRanges)
    {
        foreach (Match match in CommentBlockRegex.Matches(text))
        {
            if (!IsInClassifiedRange(match.Index, classifiedRanges))
            {
                AddTag(tags, document, rangeStart + match.Index, match.Length, ClassificationType.KnownValues.Comment);
                classifiedRanges.Add((match.Index, match.Index + match.Length));
            }
        }
    }

    private void ProcessPrintTags(ITextDocumentSnapshot document, string text, int rangeStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> classifiedRanges)
    {
        foreach (Match match in PrintTagRegex.Matches(text))
        {
            if (IsInClassifiedRange(match.Index, classifiedRanges))
                continue;

            // Tag delimiters
            var openDelim = match.Value.StartsWith("{{-") ? "{{-" : "{{";
            var closeDelim = match.Value.EndsWith("-}}") ? "-}}" : "}}";

            AddTag(tags, document, rangeStart + match.Index, openDelim.Length, ClassificationType.KnownValues.Punctuation);
            AddTag(tags, document, rangeStart + match.Index + match.Length - closeDelim.Length, closeDelim.Length, ClassificationType.KnownValues.Punctuation);

            // Process content inside the tag
            var content = match.Groups["content"].Value;
            var contentStart = rangeStart + match.Index + openDelim.Length;
            ProcessLiquidContent(document, content, contentStart, tags, classifiedRanges, isStatement: false);

            classifiedRanges.Add((match.Index, match.Index + match.Length));
        }
    }

    private void ProcessStatementTags(ITextDocumentSnapshot document, string text, int rangeStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> classifiedRanges)
    {
        foreach (Match match in StatementTagRegex.Matches(text))
        {
            if (IsInClassifiedRange(match.Index, classifiedRanges))
                continue;

            // Tag delimiters
            var openDelim = match.Value.StartsWith("{%-") ? "{%-" : "{%";
            var closeDelim = match.Value.EndsWith("-%}") ? "-%}" : "%}";

            AddTag(tags, document, rangeStart + match.Index, openDelim.Length, ClassificationType.KnownValues.Punctuation);
            AddTag(tags, document, rangeStart + match.Index + match.Length - closeDelim.Length, closeDelim.Length, ClassificationType.KnownValues.Punctuation);

            // Process content inside the tag
            var content = match.Groups["content"].Value;
            var contentStart = rangeStart + match.Index + openDelim.Length;
            ProcessLiquidContent(document, content, contentStart, tags, classifiedRanges, isStatement: true);

            classifiedRanges.Add((match.Index, match.Index + match.Length));
        }
    }

    private void ProcessLiquidContent(ITextDocumentSnapshot document, string content, int contentStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> classifiedRanges, bool isStatement)
    {
        var localClassified = new List<(int start, int end)>();

        // 1. Strings (highest priority for content)
        ProcessStrings(document, content, contentStart, tags, localClassified);

        // 2. Keywords (only in statement tags)
        if (isStatement)
        {
            ProcessKeywords(document, content, contentStart, tags, localClassified);
        }

        // 3. Constants - language and numeric
        ProcessConstants(document, content, contentStart, tags, localClassified);

        // 4. Filters
        ProcessFilters(document, content, contentStart, tags, localClassified);

        // 5. Properties (dot and array notation)
        ProcessProperties(document, content, contentStart, tags, localClassified);

        // 6. Operators
        ProcessOperators(document, content, contentStart, tags, localClassified);

        // 7. Arrays
        ProcessArrays(document, content, contentStart, tags, localClassified);

        // 8. Variables (remaining identifiers)
        ProcessVariables(document, content, contentStart, tags, localClassified);
    }

    private void ProcessStrings(ITextDocumentSnapshot document, string content, int contentStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> localClassified)
    {
        foreach (Match match in StringSingleRegex.Matches(content))
        {
            if (!IsInLocalClassified(match.Index, localClassified))
            {
                AddTag(tags, document, contentStart + match.Index, match.Length, ClassificationType.KnownValues.String);
                localClassified.Add((match.Index, match.Index + match.Length));
            }
        }

        foreach (Match match in StringDoubleRegex.Matches(content))
        {
            if (!IsInLocalClassified(match.Index, localClassified))
            {
                AddTag(tags, document, contentStart + match.Index, match.Length, ClassificationType.KnownValues.String);
                localClassified.Add((match.Index, match.Index + match.Length));
            }
        }
    }

    private void ProcessKeywords(ITextDocumentSnapshot document, string content, int contentStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> localClassified)
    {
        foreach (Match match in KeywordRegex.Matches(content))
        {
            if (!IsInLocalClassified(match.Index, localClassified))
            {
                AddTag(tags, document, contentStart + match.Index, match.Length, ClassificationType.KnownValues.Keyword);
                localClassified.Add((match.Index, match.Index + match.Length));
            }
        }
    }

    private void ProcessConstants(ITextDocumentSnapshot document, string content, int contentStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> localClassified)
    {
        foreach (Match match in ConstantLanguageRegex.Matches(content))
        {
            if (!IsInLocalClassified(match.Index, localClassified))
            {
                AddTag(tags, document, contentStart + match.Index, match.Length, ClassificationType.KnownValues.Keyword);
                localClassified.Add((match.Index, match.Index + match.Length));
            }
        }

        foreach (Match match in ConstantNumericRegex.Matches(content))
        {
            if (!IsInLocalClassified(match.Index, localClassified))
            {
                AddTag(tags, document, contentStart + match.Index, match.Length, ClassificationType.KnownValues.Number);
                localClassified.Add((match.Index, match.Index + match.Length));
            }
        }
    }

    private void ProcessFilters(ITextDocumentSnapshot document, string content, int contentStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> localClassified)
    {
        foreach (Match match in FilterRegex.Matches(content))
        {
            var pipeIndex = match.Index;
            var filterGroup = match.Groups["filter"];

            if (!IsInLocalClassified(pipeIndex, localClassified))
            {
                // Classify pipe operator
                AddTag(tags, document, contentStart + pipeIndex, 1, ClassificationType.KnownValues.Operator);
                localClassified.Add((pipeIndex, pipeIndex + 1));
            }

            if (!IsInLocalClassified(filterGroup.Index, localClassified))
            {
                // Classify filter name as a function
                AddTag(tags, document, contentStart + filterGroup.Index, filterGroup.Length, ClassificationType.KnownValues.SymbolDefinition);
                localClassified.Add((filterGroup.Index, filterGroup.Index + filterGroup.Length));
            }
        }
    }

    private void ProcessProperties(ITextDocumentSnapshot document, string content, int contentStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> localClassified)
    {
        // Dot notation
        foreach (Match match in PropertyDotRegex.Matches(content))
        {
            var dotIndex = match.Index;
            var propertyGroup = match.Groups["property"];

            if (!IsInLocalClassified(dotIndex, localClassified))
            {
                // Classify dot separator
                AddTag(tags, document, contentStart + dotIndex, 1, ClassificationType.KnownValues.Punctuation);
                localClassified.Add((dotIndex, dotIndex + 1));
            }

            if (!IsInLocalClassified(propertyGroup.Index, localClassified))
            {
                // Classify property name
                AddTag(tags, document, contentStart + propertyGroup.Index, propertyGroup.Length, ClassificationType.KnownValues.SymbolReference);
                localClassified.Add((propertyGroup.Index, propertyGroup.Index + propertyGroup.Length));
            }
        }

        // Array notation
        foreach (Match match in PropertyArrayRegex.Matches(content))
        {
            var propertyGroup = match.Groups["property"];

            if (!IsInLocalClassified(propertyGroup.Index, localClassified))
            {
                // Classify brackets
                AddTag(tags, document, contentStart + match.Index, 1, ClassificationType.KnownValues.Punctuation);
                AddTag(tags, document, contentStart + match.Index + match.Length - 1, 1, ClassificationType.KnownValues.Punctuation);

                // Classify property name
                AddTag(tags, document, contentStart + propertyGroup.Index, propertyGroup.Length, ClassificationType.KnownValues.SymbolReference);
                localClassified.Add((propertyGroup.Index, propertyGroup.Index + propertyGroup.Length));
            }
        }
    }

    private void ProcessOperators(ITextDocumentSnapshot document, string content, int contentStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> localClassified)
    {
        var operatorRegexes = new[] { ComparisonOperatorRegex, LogicalOperatorRegex, AssignmentOperatorRegex, RangeOperatorRegex };

        foreach (var regex in operatorRegexes)
        {
            foreach (Match match in regex.Matches(content))
            {
                if (!IsInLocalClassified(match.Index, localClassified))
                {
                    AddTag(tags, document, contentStart + match.Index, match.Length, ClassificationType.KnownValues.Operator);
                    localClassified.Add((match.Index, match.Index + match.Length));
                }
            }
        }
    }

    private void ProcessArrays(ITextDocumentSnapshot document, string content, int contentStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> localClassified)
    {
        foreach (Match match in ArrayRegex.Matches(content))
        {
            if (!IsInLocalClassified(match.Index, localClassified))
            {
                // Only classify brackets, content will be classified separately
                AddTag(tags, document, contentStart + match.Index, 1, ClassificationType.KnownValues.Punctuation);
                AddTag(tags, document, contentStart + match.Index + match.Length - 1, 1, ClassificationType.KnownValues.Punctuation);
            }
        }
    }

    private void ProcessVariables(ITextDocumentSnapshot document, string content, int contentStart, List<TaggedTrackingTextRange<ClassificationTag>> tags, List<(int start, int end)> localClassified)
    {
        foreach (Match match in VariableRegex.Matches(content))
        {
            var varGroup = match.Groups["variable"];
            if (!IsInLocalClassified(varGroup.Index, localClassified))
            {
                AddTag(tags, document, contentStart + varGroup.Index, varGroup.Length, ClassificationType.KnownValues.Identifier);
                localClassified.Add((varGroup.Index, varGroup.Index + varGroup.Length));
            }
        }
    }

    private static bool IsInClassifiedRange(int position, List<(int start, int end)> ranges)
    {
        return ranges.Any(r => position >= r.start && position < r.end);
    }

    private static bool IsInLocalClassified(int position, List<(int start, int end)> ranges)
    {
        return ranges.Any(r => position >= r.start && position < r.end);
    }

    private static void AddTag(List<TaggedTrackingTextRange<ClassificationTag>> tags, ITextDocumentSnapshot document, int start, int length, ClassificationType classificationType)
    {
        if (length > 0 && start >= 0 && start + length <= document.Length)
        {
            tags.Add(new(
                new(document, start, length, TextRangeTrackingMode.ExtendNone),
                new(classificationType)));
        }
    }
}
#pragma warning restore VSEXTPREVIEW_TAGGERS