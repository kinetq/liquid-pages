using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Kinetq.LiquidPages.AspNetCore;

internal sealed class PageActionEndpointDataSource : ActionEndpointDataSourceBase
{
    private readonly ActionEndpointFactory _endpointFactory;
    private readonly OrderedEndpointsSequenceProvider _orderSequence;

    public PageActionEndpointDataSource(
        PageActionEndpointDataSourceIdProvider dataSourceIdProvider,
        IActionDescriptorCollectionProvider actions,
        ActionEndpointFactory endpointFactory,
        OrderedEndpointsSequenceProvider orderedEndpoints)
        : base(actions)
    {
        DataSourceId = dataSourceIdProvider.CreateId();
        _endpointFactory = endpointFactory;
        _orderSequence = orderedEndpoints;
        DefaultBuilder = new PageActionEndpointConventionBuilder(Lock, Conventions, FinallyConventions);

        // IMPORTANT: this needs to be the last thing we do in the constructor.
        // Change notifications can happen immediately!
        Subscribe();
    }

    public int DataSourceId { get; }

    public PageActionEndpointConventionBuilder DefaultBuilder { get; }

    // Used to control whether we create 'inert' (non-routable) endpoints for use in dynamic
    // selection. Set to true by builder methods that do dynamic/fallback selection.
    public bool CreateInertEndpoints { get; set; }

    protected override List<Endpoint> CreateEndpoints(
        RoutePattern? groupPrefix,
        IReadOnlyList<ActionDescriptor> actions,
        IReadOnlyList<Action<EndpointBuilder>> conventions,
        IReadOnlyList<Action<EndpointBuilder>> groupConventions,
        IReadOnlyList<Action<EndpointBuilder>> finallyConventions,
        IReadOnlyList<Action<EndpointBuilder>> groupFinallyConventions)
    {
        var endpoints = new List<Endpoint>();
        var routeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < actions.Count; i++)
        {
            if (actions[i] is PageActionDescriptor action)
            {
                _endpointFactory.AddEndpoints(endpoints,
                    routeNames,
                    action,
                    Array.Empty<ConventionalRouteEntry>(),
                    conventions: conventions,
                    groupConventions: groupConventions,
                    finallyConventions: finallyConventions,
                    groupFinallyConventions: groupFinallyConventions,
                    CreateInertEndpoints,
                    groupPrefix);
            }
        }

        return endpoints;
    }
}