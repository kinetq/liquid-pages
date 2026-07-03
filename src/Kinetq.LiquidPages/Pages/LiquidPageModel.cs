using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Pages;

public abstract class LiquidPageModel
{
    public virtual Task OnGetAsync(LiquidRequestModel request) => Task.CompletedTask;
    public virtual Task OnPostAsync(LiquidRequestModel request) => Task.CompletedTask;
    public ILiquidResponseBuilder ResponseBuilder { get; set; }
}