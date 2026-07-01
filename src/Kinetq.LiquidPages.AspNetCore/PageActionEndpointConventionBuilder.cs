using Microsoft.AspNetCore.Builder;

namespace Kinetq.LiquidPages.AspNetCore;

public sealed class PageActionEndpointConventionBuilder : IEndpointConventionBuilder
{
    private readonly object _lock;
    private readonly List<Action<EndpointBuilder>> _conventions;
    private readonly List<Action<EndpointBuilder>> _finallyConventions;

    internal PageActionEndpointConventionBuilder(
        object lockObject,
        List<Action<EndpointBuilder>> conventions,
        List<Action<EndpointBuilder>> finallyConventions)
    {
        _lock = lockObject;
        _conventions = conventions;
        _finallyConventions = finallyConventions;
    }

    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

    public void Add(Action<EndpointBuilder> convention)
    {
        ArgumentNullException.ThrowIfNull(convention);

        lock (_lock)
        {
            _conventions.Add(convention);
        }
    }

    public void Finally(Action<EndpointBuilder> finalConvention)
    {
        ArgumentNullException.ThrowIfNull(finalConvention);

        lock (_lock)
        {
            _finallyConventions.Add(finalConvention);
        }
    }
}