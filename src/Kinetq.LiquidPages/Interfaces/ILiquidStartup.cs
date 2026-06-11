using Kinetq.LiquidPages.Builders;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidStartup
{
    void RegisterFilters();
    void RegisterPageModels();
    void RegisterPageModels(Action<LiquidPagesOptionsBuilder> buildOptionsAction);
    void RegisterFileProvider(string prefix, IFileProvider fileProvider);
}