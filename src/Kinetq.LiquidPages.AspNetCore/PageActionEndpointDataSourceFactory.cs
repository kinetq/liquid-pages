using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Kinetq.LiquidPages.AspNetCore;

internal sealed class PageActionEndpointDataSourceFactory
{
    private readonly PageActionEndpointDataSourceIdProvider _dataSourceIdProvider;
    private readonly IActionDescriptorCollectionProvider _actions;
    private readonly ActionEndpointFactory _endpointFactory;

    public PageActionEndpointDataSourceFactory(
        PageActionEndpointDataSourceIdProvider dataSourceIdProvider,
        IActionDescriptorCollectionProvider actions,
        ActionEndpointFactory endpointFactory)
    {
        _dataSourceIdProvider = dataSourceIdProvider;
        _actions = actions;
        _endpointFactory = endpointFactory;
    }

    public PageActionEndpointDataSource Create(OrderedEndpointsSequenceProvider orderedEndpoints)
    {
        return new PageActionEndpointDataSource(
            _dataSourceIdProvider,
            _actions,
            _endpointFactory,
            orderedEndpoints);
    }
}