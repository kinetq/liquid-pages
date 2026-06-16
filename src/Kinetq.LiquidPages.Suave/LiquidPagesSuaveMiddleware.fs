namespace Kinetq.LiquidPages.Suave

open System
open System.Text.RegularExpressions
open Suave
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.Writers
open Kinetq.LiquidPages
open Kinetq.LiquidPages.Managers
open Kinetq.LiquidPages.Interfaces
open Kinetq.LiquidPages.Models
open Suave.Response

[<RequireQualifiedAccess>]
module LiquidPagesExtensions =
    /// Extension methods to add LiquidPages routes to a Suave app.
        let addLiquidPages (existing: WebPart) (routeManager: ILiquidRoutesManager, liquidResponseMiddleware: ILiquidResponseMiddleware) : WebPart =
            // Get all routes from the manager
            let routes = routeManager.LiquidRoutes  // returns IEnumerable<LiquidRoute>

            // Convert each route to a Suave WebPart
            let routeWebParts =
                routes
                |> Seq.map (fun route ->
                    // Use the route's pattern as a regex filter
                    let pathFilter =
                        // Handle special case for the home route (typically "^/$")
                        if route.RouteRegex = "^/$" then
                            path "/"
                        else
                            pathRegex route.RouteRegex

                    // Build the WebPart: when the pattern matches, invoke the route's handler
                    pathFilter >=> (fun ctx ->
                        async {
                            // Map Suave's HttpRequest to LiquidRequestModel
                            let requestModel =
                                LiquidRequestModel(
                                    Route = ctx.request.url.LocalPath,
                                    Method = ctx.request.method.ToString(),
                                    LiquidRoute = route
                                )

                            requestModel.Headers <-
                                let nvc = System.Collections.Specialized.NameValueCollection()
                                ctx.request.headers
                                |> List.iter (fun (k, v) -> nvc.Add(k, v))
                                nvc

                            requestModel.Body <-
                                match ctx.request.rawForm with
                                | null -> ""
                                | bytes -> System.Text.Encoding.UTF8.GetString(bytes)

                            requestModel.QueryParams <- 
                                ctx.request.query 
                                |> List.map (fun (k, v) -> k, v |> Option.defaultValue "")
                                |> dict

                            // Execute the route's handler (it returns a Task<LiquidResponseModel>)
                            let! responseModel =
                                liquidResponseMiddleware.HandleRequestAsync(requestModel)
                                |> Async.AwaitTask

                            // Convert the response to a Suave result
                            let statusCode =
                                match responseModel.StatusCode with
                                | 200 -> HTTP_200
                                | 404 -> HTTP_404
                                | 301 -> HTTP_301
                                | 302 -> HTTP_302
                                | _ -> HTTP_500

                            let responseWebPart =
                                setMimeType "text/html"
                                >=> response statusCode responseModel.Content

                            return! responseWebPart ctx
                        }
                    )
                )
                |> Seq.toList

            // Combine the existing webpart with all LiquidPages routes.
            // The original webpart takes precedence if it matches first.
            choose (existing :: routeWebParts)