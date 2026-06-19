using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidResponseMiddleware
{
    Task HandleRequestAsync(LiquidRequestModel request, LiquidResponseModel response);
}