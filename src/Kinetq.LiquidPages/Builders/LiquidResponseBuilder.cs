using System.IO.Pipelines;

namespace Kinetq.LiquidPages.Builders;

public class LiquidResponseBuilder
{ 
    public TextWriter BodyWriter { get; set; }
    public Action<int> SetStatusCode { get; set; } = _ => { };
    public Action<string> SetContentType { get; set; } = _ => { };
}