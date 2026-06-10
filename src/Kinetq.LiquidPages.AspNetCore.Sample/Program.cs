using Kinetq.LiquidPages.AspNetCore;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLiquidPages(typeof(Program).Assembly);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var startup = scope.ServiceProvider.GetRequiredService<ILiquidStartup>();
    await startup.RegisterPageModels();

    string workingDirectory = Directory.GetCurrentDirectory();
    startup.RegisterFileProvider("/", new PhysicalFileProvider(workingDirectory));
}

app.UseLiquidPagesErrorHandling();
app.UseStaticFiles();
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapLiquidPages();
});

await app.RunAsync();
