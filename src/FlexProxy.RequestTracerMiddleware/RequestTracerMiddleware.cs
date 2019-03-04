using FlexProxy.Core.Models;
using FlexProxy.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FlexProxy.RequestTracerMiddleware
{
    public class RequestTracerMiddleware
    {
        private readonly RequestDelegate _next;
        private IRequestTraceLoggerService _requestTraceLoggerService;

        public RequestTracerMiddleware(RequestDelegate next, 
            IRequestTraceLoggerService requestTraceLoggerService)
        {
            _next = next;
            _requestTraceLoggerService = requestTraceLoggerService;
        }

        public async Task Invoke(HttpContext context)
        {
            var watch = Stopwatch.StartNew();
            await _next.Invoke(context);
            watch.Stop();

            LogTrace(context, watch.ElapsedMilliseconds);
        }

        private void LogTrace(HttpContext context, long timeTaken)
        {
            StringValues userAgent;
            context.Request.Headers.TryGetValue("User-Agent", out userAgent);
            StringValues referer;
            context.Request.Headers.TryGetValue("Referer", out referer);
            var message = $"\"s-ip\":\"{context.Connection.LocalIpAddress.ToString()}\", \"cs-method\":\"{context.Request.Method}\", \"cs-uri-stem\":\"{context.Request.Path.Value}\", \"cs-uri-query\":\"{context.Request.QueryString.Value}\", \"s-port\":\"{context.Connection.LocalPort}\", \"cs-username\":\"{context.User.Identity.Name}\", \"c-ip\":\"{context.Connection.RemoteIpAddress.ToString()}\", \"cs(User-Agent\":\"{userAgent}\", \"cs(Referer)\":\"{referer}\", \"sc-status\":\"{context.Response.StatusCode}\", \"time-taken(Milliseconds)\":\"{timeTaken}\" ";
            _requestTraceLoggerService.Log(message);
        }
    }
}
