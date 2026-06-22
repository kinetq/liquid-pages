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
    startup.RegisterPageModels();
    
    startup.RegisterFileProvider("/", new EmbeddedFileProvider(typeof(Program).Assembly));
}

app.UseLiquidPagesErrorHandling();
app.UseStaticFiles();
app.UseLiquidPages();

await app.RunAsync();
