using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Primitives;

namespace Kinetq.LiquidPages.AspNetCore;

internal sealed class LiquidPagesEndpointDataSource : EndpointDataSource
{
    private readonly object _lock = new();
    private readonly ILiquidRoutesManager _routesManager;
    private readonly Func<HttpContext, LiquidRoute, Task> _requestHandler;
    private readonly List<Action<EndpointBuilder>> _conventions = new();
    private readonly List<Action<EndpointBuilder>> _finallyConventions = new();
    private readonly IChangeToken _changeToken = new CancellationChangeToken(CancellationToken.None);

    private List<Endpoint>? _endpoints;

    public LiquidPagesEndpointDataSource(
        ILiquidRoutesManager routesManager,
        Func<HttpContext, LiquidRoute, Task> requestHandler)
    {
        _routesManager = routesManager;
        _requestHandler = requestHandler;
        DefaultBuilder = new PageActionEndpointConventionBuilder(_lock, _conventions, _finallyConventions);
    }

    public PageActionEndpointConventionBuilder DefaultBuilder { get; }

    public override IReadOnlyList<Endpoint> Endpoints
    {
        get
        {
            if (_endpoints is null)
            {
                lock (_lock)
                {
                    _endpoints ??= CreateEndpoints();
                }
            }

            return _endpoints;
        }
    }

    public override IChangeToken GetChangeToken() => _changeToken;

    private List<Endpoint> CreateEndpoints()
    {
        var endpoints = new List<Endpoint>();
        var methods = new[] { "GET", "POST" };

        foreach (var route in _routesManager.LiquidRoutes)
        {
            if (string.IsNullOrWhiteSpace(route.RouteTemplate))
            {
                continue;
            }

            var routePattern = RoutePatternFactory.Parse(route.RouteTemplate);
            RequestDelegate requestDelegate = context => _requestHandler(context, route);

            var builder = new RouteEndpointBuilder(requestDelegate, routePattern, 0)
            {
                DisplayName = $"LiquidPage: {route.RouteTemplate}",
            };

            builder.Metadata.Add(route);
            builder.Metadata.Add(new HttpMethodMetadata(methods));

            for (var i = 0; i < _conventions.Count; i++)
            {
                _conventions[i](builder);
            }

            for (var i = 0; i < _finallyConventions.Count; i++)
            {
                _finallyConventions[i](builder);
            }

            endpoints.Add(builder.Build());
        }

        return endpoints;
    }
}