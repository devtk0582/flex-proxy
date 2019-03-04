using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.RequestTracerMiddleware
{
    public static class RequestTracerExtensions
    {
        public static IApplicationBuilder UseRequestTracer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestTracerMiddleware>();
        }
    }
}
