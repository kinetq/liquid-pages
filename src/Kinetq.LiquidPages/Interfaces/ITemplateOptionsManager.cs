using Fluid;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages.Interfaces;

public interface ITemplateOptionsManager
{
    IDictionary<string, TemplateOptions> TemplateOptionsMap { get; }
    TemplateOptions GetTemplateOptions(string path);
    void RegisterTemplateOptions(string prefix, IFileProvider fileProvider);
}