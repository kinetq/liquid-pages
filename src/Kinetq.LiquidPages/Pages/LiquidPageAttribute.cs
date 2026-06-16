using System.Text.RegularExpressions;

namespace Kinetq.LiquidPages.Pages;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class LiquidPageAttribute : Attribute
{
    public string? RouteTemplate { get; }
    public Regex? RouteRegex { get; }
    public string TemplatePath { get; }

    public LiquidPageAttribute(string routeTemplate, string templatePath)
    {
        RouteTemplate = routeTemplate;
        TemplatePath = templatePath;
    }

    public LiquidPageAttribute(Regex routeRegex, string templatePath)
    {
        RouteRegex = routeRegex;
        TemplatePath = templatePath;
    }

    public LiquidPageAttribute(string templatePath)
    {
        TemplatePath = templatePath;
    }
}