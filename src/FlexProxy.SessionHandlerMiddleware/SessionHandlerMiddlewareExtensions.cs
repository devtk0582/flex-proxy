using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.SessionHandlerMiddleware
{
    public static class SessionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionHandlerMiddleware>();
        }
    }
}
