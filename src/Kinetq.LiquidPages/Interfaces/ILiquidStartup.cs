using Kinetq.LiquidPages.Builders;

namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidStartup
{
    Task RegisterRoutes();
    Task RegisterFilters();
    Task RegisterPageModels();
    Task RegisterPageModels(Action<LiquidPagesOptionsBuilder> buildOptionsAction);
}