using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidPartialsManager
{
    IDictionary<string, LiquidPartial> Partials { get; }
    void RegisterTemplate(string key, LiquidPartial partial);
    LiquidPartial GetPartial(string key);
}