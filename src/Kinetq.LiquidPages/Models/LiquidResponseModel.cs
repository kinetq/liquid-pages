namespace Kinetq.LiquidPages.Models;

public class LiquidResponseModel
{ 
    public StreamWriter BodyWriter { get; set; }
    public Action<int> SetStatusCode { get; set; } = _ => { };
    public Action<string> SetContentType { get; set; } = _ => { };
    public Action StartResponse { get; set; } = () => { };
}