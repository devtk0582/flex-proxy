using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FlexProxy.RobotsMiddleware
{
    public class RobotsMiddleware
    {
        private readonly RequestDelegate _next;
        public RobotsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers[HeaderNames.CacheControl] = "no-cache";
            context.Response.Headers[HeaderNames.Expires] = "0";
            context.Response.Headers[HeaderNames.Pragma] = "no-cache";
            context.Response.Headers[HeaderNames.ContentType] = "text/plain";

            var preventBots = new StringBuilder();
            preventBots.AppendLine("User-agent: *");
            preventBots.AppendLine("Disallow: /");

            await context.Response.WriteAsync(preventBots.ToString());
        }
    }
}
