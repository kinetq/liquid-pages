using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Routing;

namespace Kinetq.LiquidPages.AspNetCore;

internal sealed class OrderedEndpointsSequenceProviderCache
{
    private readonly ConditionalWeakTable<IEndpointRouteBuilder, OrderedEndpointsSequenceProvider> _cache = new();

    public OrderedEndpointsSequenceProvider GetOrCreateOrderedEndpointsSequenceProvider(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return _cache.GetValue(endpoints, static _ => new OrderedEndpointsSequenceProvider());
    }
}