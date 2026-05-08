namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidStartup
{
    Task RegisterRoutes();
    Task RegisterFilters();
    Task RegisterPageModels();
}