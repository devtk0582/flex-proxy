using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware
{
    public static class ContentModificationExtensions
    {
        public static IApplicationBuilder UseContentModification(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ContentModificationMiddleware>();
        }
    }
}
