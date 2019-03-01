using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FlexProxy.ExceptionHandlerMiddleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private ILogger _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, 
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ExceptionHandlerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                HandleError(ex, context);
            }
        }

        private void HandleError(Exception ex, HttpContext context)
        {
            _logger.LogError($"Proxy Exception: {ex}");
            var errorMessage = "<h1>Error processing the request</h1>";
            context.Response.WriteAsync(errorMessage);
        }
    }
}
