namespace Kinetq.LiquidPages.Pages;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class LiquidPageAttribute : Attribute
{
    public string? RoutePattern { get; }
    public string TemplatePath { get; }

    public LiquidPageAttribute(string routePattern, string templatePath)
    {
        RoutePattern = routePattern;
        TemplatePath = templatePath;
    }

    public LiquidPageAttribute(string templatePath)
    {
        TemplatePath = templatePath;
    }
}