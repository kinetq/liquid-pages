namespace Kinetq.LiquidPages.Suave.Sample

open System
open System.IO
open System.Net
open System.Reflection
open System.Threading.Tasks
open Kinetq.LiquidPages.Helpers
open Kinetq.LiquidPages.Interfaces
open Kinetq.LiquidPages.Models
open Kinetq.LiquidPages.Pages
open Kinetq.LiquidPages.Suave
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.FileProviders
open Microsoft.Extensions.Logging
open Suave

[<LiquidPage("^/$", "Pages/Home.liquid")>]
type HomeModel(logger: ILogger<HomeModel>) =
    inherit LiquidPageModel()

    member val Title = "Welcome to Home" with get, set

    override _.OnGetAsync(_request: LiquidRequestModel) =
        logger.LogInformation("Serving home page")
        Task.CompletedTask

[<LiquidErrorPage(HttpStatusCode.NotFound, "ErrorPages/NotFound.liquid")>]
type NotFoundModel() =
    inherit LiquidPageModel()

    member val Title = "Page Not Found" with get, set
    member val NotFoundMessage = "The page you are looking for was not found." with get, set

    override _.OnGetAsync(_request: LiquidRequestModel) =
        Task.CompletedTask

module Program =
    [<EntryPoint>]
    let main _args =
        let services =
            ServiceCollection()
                .AddLogging(fun builder ->
                    builder.ClearProviders() |> ignore
                    builder
                        .AddSimpleConsole(fun options ->
                            options.IncludeScopes <- true
                            options.SingleLine <- true
                            options.TimestampFormat <- "hh:mm:ss ")
                        .SetMinimumLevel(LogLevel.Debug)
                    |> ignore)

        services.AddLiquidPages(Assembly.GetExecutingAssembly()) |> ignore

        use serviceProvider = services.BuildServiceProvider()
        let startup = serviceProvider.GetRequiredService<ILiquidStartup>()
        startup.RegisterPageModels()

        let workingDirectory = Directory.GetCurrentDirectory()
        let projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName
        startup.RegisterFileProvider("/", PhysicalFileProvider(projectDirectory))

        let middleware = serviceProvider.GetRequiredService<ILiquidResponseMiddleware>()
        let routesManager = serviceProvider.GetRequiredService<ILiquidRoutesManager>()

        let app = LiquidPagesExtensions.addLiquidPages (Successful.OK "") (routesManager, middleware)

        let config =
            { defaultConfig with
                bindings = [ HttpBinding.createSimple HTTP "0.0.0.0" 5662 ] }

        startWebServer config app
        0
