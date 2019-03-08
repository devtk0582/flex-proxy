using FlexProxy.ContentModificationMiddleware.ContentAbstractionProviders;
using FlexProxy.ContentModificationMiddleware.HostedObjects;
using FlexProxy.ContentModificationMiddleware.ModificationEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware
{
    public static class ContentModificationServiceExtensions
    {
        public static IServiceCollection AddContentModification(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IContentAbstractionProvider, ContentAbstractionProvider>();
            services.TryAddSingleton<IRequestApi, RequestApi>();
            services.TryAddSingleton<IResponseApi, ResponseApi>();
            services.TryAddSingleton<IConsoleLogApi, ConsoleLogApi>();
            services.TryAddSingleton<IJSEnginePool, JSEnginePool>();
            services.TryAddTransient<IJSEngine, JSEngine>();

            return services;
        }
    }
}
