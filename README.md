# Liquid Pages
[![CI](https://github.com/kinetq/liquid-middleware/actions/workflows/main.yml/badge.svg)](https://github.com/kinetq/liquid-middleware/actions/workflows/main.yml)
![NuGet Downloads](https://img.shields.io/nuget/dt/Kinetq.LiquidPages)
![GitHub commit activity](https://img.shields.io/github/commit-activity/m/kinetq/liquid-pages)
![GitHub last commit](https://img.shields.io/github/last-commit/kinetq/liquid-pages)
[![View - Documentation](https://img.shields.io/badge/view-Documentation-AB54FF)](https://www.kinetq.com/docs/open-source-software/liquid-pages)
<p align="center">
  <img src="src/Kinetq.LiquidPages.Extension/Images/Logo.png" alt="App Dashboard" width="150">
</p>

If you find this project helpful, please consider giving it a ⭐!

LiquidPages is an open-source C# library that brings a Razor Pages–style MVVM framework to [Liquid](https://shopify.github.io/liquid/) templates. It uses [Fluid](https://github.com/sebastienros/fluid) under the hood and is designed to plug into virtually any .NET web server.

## Table of Contents

- [Why?](#why)
- [Setup](#setup)
  - [1. Install the NuGet package](#1-install-the-nuget-package)
  - [2. Register services](#2-register-services)
  - [3. Register routes, filters, and template file providers at startup](#3-register-routes-filters-and-template-file-providers-at-startup)
  - [4. Create a page model](#4-create-a-page-model)
  - [5. Create the Liquid template](#5-create-the-liquid-template)
  - [6. Wire up middleware](#6-wire-up-middleware)
    - [ASP.NET Core middleware](#aspnet-core-middleware)
    - [SimpleW middleware](#simplew-middleware)
    - [GenHTTP middleware](#genhttp-middleware)
    - [EmbedIO middleware](#embedio-middleware)
- [Visual Studio Extension](#visual-studio-extension)
  - [Without the extension](#without-the-extension)
- [Performance](#performance)
- [Documentation](#documentation)

## Why?

Most .NET templating solutions are tightly coupled to a specific web server or framework. LiquidPages was built to solve two problems:

- **Web-server agnostic** — the middleware can hook into any web server (EmbedIO, a custom HTTP listener, or even a MAUI WebView!), so you aren't locked into a particular host.
- **Modular by design** — Liquid templates and their page models can live in any C# project across a solution. Each route defines its own `IFileProvider`, making it straightforward to split pages across multiple projects and compose them at runtime.

The result is a lightweight, portable templating layer that stays out of your way regardless of how your application is structured.

## Setup

### 1. Install the NuGet package

```powershell
nuget add Kinetq.LiquidPages
```

### 2. Register services

```csharp
services.AddLiquidPages();
```

### 3. Register routes, filters, and template file providers at startup

Inject `ILiquidStartup` and call the registration methods during your application's startup phase:

```csharp
_liquidStartup.RegisterPageModels();
_liquidStartup.RegisterFilters();
_liquidStartup.RegisterFileProvider("/", fileProvider);
```

`RegisterFileProvider` is how template options are now registered for each route prefix. This is required so LiquidPages can resolve templates from the correct source (physical files, embedded files, etc.).
**It should always be AFTER `RegisterPageModels` so page model types are available when the provider is registered.**

See the sample projects for concrete startup usage:

- `src/Kinetq.LiquidPages.AspNetCore.Sample/Program.cs`
- `src/Kinetq.LiquidPages.EmbedIO.Sample/Program.cs`
- `src/Kinetq.LiquidPages.GenHTTP.Sample/Program.cs`
- `src/Kinetq.LiquidPages.SimpleW.Sample/Program.cs`

### 4. Create a page model

Decorate your page model with `[LiquidPage]`, providing a route regex and template path:

```csharp
[LiquidPage("/", "Pages/Home.liquid")]
public class HomeModel : LiquidPageModel
{
    public string Title { get; set; } = "Welcome to Home";

    public override Task OnGetAsync(LiquidRequestModel request)
    {
        return Task.CompletedTask;
    }
}
```

### 5. Create the Liquid template

Properties on the page model are available in the template via the `view_model` object:

```liquid
{% capture page_content %}
    <h1>{{ view_model.title }}</h1>
{% endcapture %}

{% include 'Layouts/default.liquid' %}
```

### 6. Wire up middleware

#### ASP.NET Core middleware

Install the ASP.NET Core companion package:

```powershell
dotnet add package Kinetq.LiquidPages.AspNetCore
```

Register LiquidPages services, initialize startup registrations, and map LiquidPages middleware directly on the app:

```csharp
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

app.MapStaticAssets();
app.UseLiquidPages();

await app.RunAsync();
```

#### SimpleW middleware

Install the SimpleW companion package:

```powershell
dotnet add package Kinetq.LiquidPages.SimpleW
```

Resolve `ILiquidRoutesManager`, `ILiquidResponseMiddleware`, and `ILiquidStartup` from your container, then register page models/file providers before attaching `LiquidPagesModule`:

```csharp
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Microsoft.Extensions.FileProviders;
using SimpleW;

var liquidRoutesManager = serviceProvider.GetRequiredService<ILiquidRoutesManager>();
var liquidResponseMiddleware = serviceProvider.GetRequiredService<ILiquidResponseMiddleware>();
var liquidStartup = serviceProvider.GetRequiredService<ILiquidStartup>();

liquidStartup.RegisterFileProvider("/", new EmbeddedFileProvider(typeof(Program).Assembly));
liquidStartup.RegisterPageModels();

var server = new SimpleWServer(IPAddress.Any, 2015);
server.UseStaticFilesModule(options => {
    options.Path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Static");
    options.Prefix = "/Static";
    options.CacheTimeout = TimeSpan.FromDays(1);
    options.AutoIndex = true;
});

server.UseModule(new LiquidPagesModule(liquidRoutesManager, liquidResponseMiddleware)
{
    MapFallback404 = true
});

await server.RunAsync();
```

#### GenHTTP middleware

Install the GenHTTP companion package:

```powershell
dotnet add package Kinetq.LiquidPages.GenHTTP
```

Resolve `ILiquidStartup`, `ILiquidResponseMiddleware`, and `ILiquidRoutesManager` from your container, register routes/file providers, then attach `LiquidHandlerBuilder` to your layout:

```csharp
var middleware = serviceProvider.GetRequiredService<ILiquidResponseMiddleware>();
var routesManager = serviceProvider.GetRequiredService<ILiquidRoutesManager>();
var startup = serviceProvider.GetRequiredService<ILiquidStartup>();

startup.RegisterPageModels();
startup.RegisterFileProvider("/", new EmbeddedFileProvider(typeof(Program).Assembly));

var staticResources = Resources.From(ResourceTree.FromDirectory(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Static")));
var app = Layout.Create()
    .Add("Static", staticResources)
    .Add(new LiquidHandlerBuilder(middleware, routesManager));

await Host.Create()
          .Handler(app)
          .Bind(IPAddress.Any, 8080)
          .RunAsync();
```

`LiquidHandlerBuilder` implements `IHandlerBuilder<LiquidHandlerBuilder>`, so you can attach any GenHTTP concern (compression, caching, CORS, etc.) before the handler is built. See the [full GenHTTP documentation](docs/genhttp-liquid-pages.md) for a complete walkthrough.

#### EmbedIO middleware

Install the EmbedIO companion package and attach LiquidPages to your `WebServer`:

```powershell
dotnet add package Kinetq.LiquidPages.EmbedIO
```

```csharp
var startup = serviceProvider.GetRequiredService<ILiquidStartup>();
startup.RegisterFileProvider("/", new EmbeddedFileProvider(typeof(Program).Assembly));
startup.RegisterPageModels();

var middleware = serviceProvider.GetRequiredService<ILiquidResponseMiddleware>();
var routesManager = serviceProvider.GetRequiredService<ILiquidRoutesManager>();

webServer.WithLiquidPages(middleware, routesManager);
```

If you need lower-level control, `LiquidWebModule` now takes `ILiquidRoutesManager` in its constructor:

```csharp
webServer.WithModule(new LiquidWebModule("/", routesManager)
{
    LiquidResponseMiddleware = middleware
});
```

For other web servers, call `HandleRequestAsync` on `ILiquidResponseMiddleware` from within your own request handler.

## Visual Studio Extension

Install the `Kinetq.LiquidPages.Extension` from the Visual Studio Marketplace for syntax highlighting, a Prettier-based formatter (`Ctrl+Shift+X`), and quick commands to scaffold new pages.

Before using the **Add LiquidPage** command, install the templates package:

```powershell
dotnet new install Kinetq.LiquidPages.Templates
```

The extension automatically adds a `.filenesting.json` to your project, which nests `.liquid.cs` code-behind files under their corresponding `.liquid` template in Solution Explorer.

### Without the extension

If you choose not to use the extension, you will need to configure the following manually.

**`.filenesting.json`** — add this to your project root to nest `.liquid.cs` files under their `.liquid` counterparts:

```json
{
  "root": true,
  "dependentFileProviders": {
    "add": {
      "extensionToExtension": {
        "add": {
          ".liquid": [ ".liquid.cs" ]
        }
      }
    }
  }
}
```

**`.vs/VSWorkspaceSettings.json`** — add this to suppress HTML validation warnings inside `.liquid` files and enable HTML syntax highlighting for them in Visual Studio:

```json
{
  "HtmlValidation.IgnorePatterns": [
    "**/*.liquid"
  ]
}
```

## Performance
Detailed performance for each server is available <a href="https://kinetqprodeastus2.blob.core.windows.net/assets/perf-results.html" target="_blank" rel="noopener noreferrer">here</a>.

Performance is baselined against AspNetCore/Razor Pages. Each sample project was run with the same page model and template.

## Documentation

Full documentation: https://www.kinetq.com/docs/open-source-software/liquid-pages