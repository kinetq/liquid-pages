using System.Net;

namespace Kinetq.LiquidPages.Pages;

public class LiquidPageErrorAttribute : Attribute
{
    public HttpStatusCode StatusCode { get; }
    public string TemplatePath { get; }

    public LiquidPageErrorAttribute(HttpStatusCode statusCode, string templatePath)
    {
        TemplatePath = templatePath;
        StatusCode = statusCode;
    }
}