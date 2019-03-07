using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.HostedObjects
{
    public interface IJavascriptApi : IApiHostObject
    {
        void Initialize(StringBuilder content);
        string GetScript();
        void UpdateScript(string value);
    }

    public class JavascriptApi : IJavascriptApi
    {
        private StringBuilder _content;

        public JavascriptApi() { }

        public JavascriptApi(StringBuilder content)
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

        public string GetScript()
        {
            return _content.ToString();
        }

        public void UpdateScript(string value)
        {
            _content.Clear();
            _content.Append(value);
        }
    }
}
