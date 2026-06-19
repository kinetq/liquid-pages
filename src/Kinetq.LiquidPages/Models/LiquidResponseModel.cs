using System.IO.Pipelines;

namespace Kinetq.LiquidPages.Models;

public class LiquidResponseModel
{ 
    public TextWriter BodyWriter { get; set; }
    public Action<int> SetStatusCode { get; set; } = _ => { };
    public Action<string> SetContentType { get; set; } = _ => { };
    public Action<CancellationToken> StartResponse { get; set; } = _ => { };
}