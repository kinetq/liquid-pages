using System.IO.Pipelines;

namespace Kinetq.LiquidPages.Builders;

public sealed class LiquidResponseBuilder
{ 
    public TextWriter BodyWriter { get; init; }
    public Action<int> SetStatusCode { get; init; } = _ => { };
    public Action<string> SetContentType { get; init; } = _ => { };
}