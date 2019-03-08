using FlexProxy.ContentModificationMiddleware.ContentAbstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.ContentAbstractionProviders
{
    public class ContentAbstractionProviderCollection
    {

        private Dictionary<string, Type> _collection = new Dictionary<string, Type>();

        public Type GetProviderType(string mediaType)
        {
            return _collection.ContainsKey(mediaType) ? _collection[mediaType] : null;
        }


        public Type this[string key]
        {
            get
            {
                return _collection[key];
            }
        }


        public void Add<TProvider>(IEnumerable<string> mediaTypes) where TProvider : IContentAbstraction
        {
            Add(typeof(TProvider), mediaTypes);
        }


        public void Add(Type providerType, IEnumerable<string> mediaTypes)
        {
            if (providerType == null)
            {
                throw new ArgumentNullException(nameof(providerType));
            }

            if (!typeof(IContentAbstraction).IsAssignableFrom(providerType))
            {
                throw new ArgumentException($"The provider must implement {nameof(IContentAbstraction)}.", nameof(providerType));
            }

            foreach (var mediaType in mediaTypes)
            {
                _collection[mediaType] = providerType;
            }
        }
    }
}
