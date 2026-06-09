using Kinetq.LiquidPages.Builders;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidStartup
{
    Task RegisterFilters();
    Task RegisterPageModels();
    Task RegisterPageModels(Action<LiquidPagesOptionsBuilder> buildOptionsAction);
    void RegisterFileProvider(string prefix, IFileProvider fileProvider);
}