using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.AspNetCore.Routing;

namespace Kinetq.LiquidPages.Managers;

public class LiquidRoutesManager : ILiquidRoutesManager
{
    private readonly ILogger<LiquidRoutesManager> _logger;

    private readonly Lazy<IList<LiquidRoute>> _liquidRoutes =
        new(() => new List<LiquidRoute>());

    private readonly Lazy<IDictionary<int, LiquidRoute>> _errorRoutes =
                new(() => new Dictionary<int, LiquidRoute>());

    public LiquidRoutesManager(ILogger<LiquidRoutesManager> logger)
    {
        _logger = logger;
    }

    public IList<LiquidRoute> LiquidRoutes => _liquidRoutes.Value;
    public IDictionary<int, LiquidRoute> ErrorRoutes => _errorRoutes.Value;

    public void RegisterRoute(LiquidRoute route)
    {
        if (LiquidRoutes.Any(r => r.RouteTemplate.Equals(route.RouteTemplate)))
        {
            _logger.LogWarning("Route already exists: {Route}", route.RouteTemplate);
            return;
        }

        LiquidRoutes.Add(route);
        _logger.LogDebug("Added route: {Route}", route);
    }

    public void RegisterErrorRoute(int statusCode, LiquidRoute route)
    {
        if (_errorRoutes.Value.ContainsKey(statusCode))
        {
            _logger.LogWarning("Error route already exists for status code {StatusCode}", statusCode);
            return;
        }

        _errorRoutes.Value[statusCode] = route;
        _logger.LogDebug("Added error route for status code {StatusCode}: {Route}", statusCode, route);
    }

    public LiquidRoute? GetRouteForStatusCode(HttpStatusCode statusCode)
    {
        _errorRoutes.Value.TryGetValue((int)statusCode, out var route);
        return route;
    }

    public LiquidRoute? GetRouteForPath(string path)
    {
        PathString requestPath = new PathString(path);
        LiquidRoute? liquidRoute = null;
        foreach (var route in LiquidRoutes)
        {
            RouteTemplate parsedTemplate = TemplateParser.Parse(route.RouteTemplate);
            var defaults = new RouteValueDictionary();
            var matcher = new TemplateMatcher(parsedTemplate, defaults);
            var routeValues = new RouteValueDictionary();
            if (matcher.TryMatch(requestPath, routeValues))
            {
                liquidRoute = route;
                liquidRoute.RouteValues = routeValues;
                break;
            }
        }

        return liquidRoute;
    }
}