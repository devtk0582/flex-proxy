using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.HostedObjects
{
    public interface IContentApi : IApiHostObject
    {
        void Initialize(StringBuilder content);
        string Get();
        void Update(string value);
    }

    public class ContentApi : IContentApi
    {
        private StringBuilder _content;

        public ContentApi() { }

        public ContentApi(StringBuilder content)
        {
            _content = content;
        }

        public void Initialize(StringBuilder content)
        {
            _content = content;
        }

        public void CleanUp()
        {
            _content = null;
        }

        public string Get()
        {
            return _content.ToString();
        }

        public void Update(string value)
        {
            _content.Clear();
            _content.Append(value);
        }
    }
}
