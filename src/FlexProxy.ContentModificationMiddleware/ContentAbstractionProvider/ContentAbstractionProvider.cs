using FlexProxy.ContentModificationMiddleware.ContentAbstraction;
using FlexProxy.ContentModificationMiddleware.ModificationEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FlexProxy.ContentModificationMiddleware.ContentAbstractionProvider
{
    public interface IContentAbstractionProvider
    {
        Task<IContentAbstraction> GetAbstractionProviderAsync(ModificationContext content);
    }

    public class ContentAbstractionProvider : IContentAbstractionProvider
    {
        private IOptions<ContentModificationOptions> _options;
        private IServiceProvider _services;

        public ContentAbstractionProvider(IServiceProvider services, IOptions<ContentModificationOptions> options)
        {
            _options = options;
            _services = services;
        }

        public async Task<IContentAbstraction> GetAbstractionProviderAsync(ModificationContext context)
        {
            var defaultAbstractionType = typeof(DefaultAbstraction);

            var providerType = _options.Value.ContentProviders.GetProviderType(GetMediaType(context.ContentType));
            providerType = providerType ?? defaultAbstractionType;

            var instance = ActivateAbstraction(providerType);

            var isContentValidForType = await instance.ValidateAsync(context);

            return isContentValidForType ? instance : ActivateAbstraction(defaultAbstractionType);
        }

        private IContentAbstraction ActivateAbstraction(Type type)
        {
            var abstraction = (IContentAbstraction)ActivatorUtilities.CreateInstance(_services, type);
            return abstraction;
        }

        private string GetMediaType(string contentTypeHeader)
        {
            MediaTypeHeaderValue parsedHeader;

            if (!MediaTypeHeaderValue.TryParse(contentTypeHeader, out parsedHeader))
            {
                return string.Empty;
            }

            return parsedHeader.MediaType ?? string.Empty;
        }
    }
}
