using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Specialized;
using System.Text;

namespace Kinetq.LiquidPages.AspNetCore
{
    public class LiquidPagesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILiquidResponseMiddleware _liquidResponseMiddleware;

        public LiquidPagesMiddleware(RequestDelegate next, ILiquidResponseMiddleware liquidResponseMiddleware)
        {
            _next = next;
            _liquidResponseMiddleware = liquidResponseMiddleware;
        }

        public Task Invoke(HttpContext context) => InvokeAsync(context); // Stops VS from nagging about async method without ...Async suffix.

        async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                var headers = new NameValueCollection();
                foreach (var header in request.Headers)
                {
                    headers.Add(header.Key, header.Value.ToString());
                }

                var liquidRequest = new LiquidRequestModel()
                {
                    Route = request.Path.Value ?? "/",
                    QueryParams = (request.QueryString.Value ?? string.Empty).GetQueryParams(),
                    Headers = headers,
                    Method = request.Method
                };

                if (request.ContentLength > 0)
                {
                    using var reader = new StreamReader(request.Body, Encoding.UTF8);
                    liquidRequest.Body = await reader.ReadToEndAsync();
                }

                var responseModel =
                    await _liquidResponseMiddleware.HandleRequestAsync(liquidRequest);

                response.ContentLength = responseModel.Content.Length;
                response.ContentType = responseModel.ContentType;
                response.StatusCode = responseModel.StatusCode;

                await response.Body.WriteAsync(responseModel.Content);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                byte[] errorBuffer = Encoding.UTF8.GetBytes($"Internal Server Error: {ex.Message}");
                response.ContentLength = errorBuffer.Length;
                response.ContentType = "text/html";
                await response.Body.WriteAsync(errorBuffer);
            }
        }
    }
}
