using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.RobotsMiddleware
{
    public static class RobotsExtensions
    {
        public static void UseRobots(this IApplicationBuilder builder)
        {
            builder.Map("/robots.txt", appBuilder => appBuilder.UseMiddleware<RobotsMiddleware>());
        }
    }
}
