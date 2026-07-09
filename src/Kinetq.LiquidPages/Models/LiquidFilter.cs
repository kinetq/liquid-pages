using Fluid;

namespace Kinetq.LiquidPages.Models
{
    public sealed class LiquidFilter
    {
        public string Name { get; init; } = null!;
        public FilterDelegate FilterDelegate { get; init; } = null!;
    }
}
