using Kinetq.LiquidPages.Models;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages.Pages;

public abstract class LiquidPageModel
{
    public virtual IFileProvider GetFileProvider()
    {
#if DEBUG
        string workingDirectory = Directory.GetCurrentDirectory();
        string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
        return new PhysicalFileProvider(projectDirectory);
#endif

#if !DEBUG
        return new EmbeddedFileProvider(GetType().Assembly, GetType().Namespace);
#endif
    }
    public virtual Task OnGetAsync(LiquidRequestModel request) => Task.CompletedTask;
    public virtual Task OnPostAsync(LiquidRequestModel request) => Task.CompletedTask;
}