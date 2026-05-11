using System.Net;

namespace Kinetq.LiquidPages.Pages;

public class LiquidErrorPageAttribute : Attribute
{
    public HttpStatusCode StatusCode { get; }
    public string TemplatePath { get; }

    public LiquidErrorPageAttribute(HttpStatusCode statusCode, string templatePath)
    {
        TemplatePath = templatePath;
        StatusCode = statusCode;
    }
}