using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.WebProxyMiddleware
{
    public static class WebProxyExtensions
    {
        public static IApplicationBuilder UseWebProxy(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebProxyMiddleware>();
        }
    }
}
