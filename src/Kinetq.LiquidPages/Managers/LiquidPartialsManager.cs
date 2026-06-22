using System.Collections.Concurrent;
using Fluid;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Managers;

public class LiquidPartialsManager : ILiquidPartialsManager
{
    private readonly Lazy<IDictionary<string, LiquidPartial>> _partials =
        new(() => new ConcurrentDictionary<string, LiquidPartial>());

    public IDictionary<string, LiquidPartial> Partials => _partials.Value;

    public void RegisterTemplate(string key, LiquidPartial partial)
    {
        if (!Partials.TryGetValue(key, out var value))
        {
            _partials.Value[key] = partial;
        }
    }

    public LiquidPartial GetPartial(string key)
    {
        if (Partials.TryGetValue(key, out var partial))
        {
            return partial;
        }

        partial = new LiquidPartial();
        Partials.Add(key, partial);
        return partial;
    }
}