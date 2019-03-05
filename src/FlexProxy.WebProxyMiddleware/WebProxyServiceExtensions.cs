using FlexProxy.WebProxyMiddleware.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.WebProxyMiddleware
{
    public static class WebProxyServiceExtensions
    {
        public static IServiceCollection AddWebProxy(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IRequestBuilder, RequestBuilder>();
            services.TryAddSingleton<IResponseBuilder, ResponseBuilder>();

            return services;
        }
    }
}
