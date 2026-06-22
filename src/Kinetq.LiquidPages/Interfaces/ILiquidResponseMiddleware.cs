using Kinetq.LiquidPages.Builders;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidResponseMiddleware
{    
    Task HandleRequestAsync<T>(LiquidRequestModel request, LiquidResponseBuilder<T> response);
}