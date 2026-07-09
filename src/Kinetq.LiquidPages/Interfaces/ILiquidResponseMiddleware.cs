using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidResponseMiddleware
{
    Task<string?> HandleRequestAsync(LiquidRequestModel request, ILiquidResponseBuilder responseBuilder);
}