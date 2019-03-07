using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.HostedObjects
{
    public interface IJsonApi : IApiHostObject
    {
        void Initialize(StringBuilder content);
        string GetData();
        void UpdateData(string jsonObject);
    }

    public class JsonApi : IJsonApi
    {
        private StringBuilder _content;

        public JsonApi() { }

        public JsonApi(StringBuilder content)
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

        public string GetData()
        {
            return _content.ToString();
        }

        public void UpdateData(string jsonObject)
        {
            _content.Clear();
            _content.Append(jsonObject);
        }
    }
}
