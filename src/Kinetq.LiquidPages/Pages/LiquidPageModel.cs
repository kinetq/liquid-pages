using Kinetq.LiquidPages.Models;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages.Pages;

public abstract class LiquidPageModel
{
    public abstract IFileProvider GetFileProvider();
    public virtual Task OnGetAsync(LiquidRequestModel request) => Task.CompletedTask;
    public virtual Task OnPostAsync(LiquidRequestModel request) => Task.CompletedTask;
}