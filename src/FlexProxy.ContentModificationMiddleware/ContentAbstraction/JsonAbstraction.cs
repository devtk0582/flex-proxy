using FlexProxy.ContentModificationMiddleware.ModificationEngine;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FlexProxy.ContentModificationMiddleware.ContentAbstraction
{
    public class JsonAbstraction : IContentAbstraction
    {
        private Encoding _originalEncoding;
        private string[] _contentTypes = { "application/json" };
        private StringBuilder _content;

        public string[] ContentTypes => _contentTypes;

        public async Task<Stream> ReadAsStream()
        {
            var resultStream = new MemoryStream();

            var bytes = _originalEncoding.GetBytes(_content?.ToString());

            await resultStream.WriteAsync(bytes, 0, bytes.Length);

            await resultStream.FlushAsync();

            resultStream.Position = 0;

            return resultStream;
        }

        public async Task Register(ModificationContext modificationContext, IJSEngine engine)
        {
            string jsonContent = string.Empty;

            using (StreamReader reader = new StreamReader(modificationContext.ContentStream, GetEncoding(modificationContext.ContentType)))
            {
                jsonContent = await reader.ReadToEndAsync();
            }

            _content = new StringBuilder(jsonContent);

            engine.InitializeJsonApi(_content);
        }

        public async Task<bool> ValidateAsync(ModificationContext context)
        {
            return true;
        }

        private Encoding GetEncoding(string contentType)
        {
            if (_originalEncoding != null) return _originalEncoding;

            _originalEncoding = Encoding.UTF8;

            MediaTypeHeaderValue parsedHeader;

            if (!MediaTypeHeaderValue.TryParse(contentType, out parsedHeader))
            {
                throw new InvalidOperationException("ContentType header could not be parsed");
            }

            return _originalEncoding = parsedHeader?.Encoding ?? _originalEncoding;
        }
    }
}
