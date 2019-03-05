using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.RequestTracerMiddleware
{
    public static class RequestTracerMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestTracer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestTracerMiddleware>();
        }
    }
}
