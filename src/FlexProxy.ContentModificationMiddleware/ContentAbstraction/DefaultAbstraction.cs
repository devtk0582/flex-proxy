using FlexProxy.ContentModificationMiddleware.ModificationEngine;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FlexProxy.ContentModificationMiddleware.ContentAbstraction
{
    public class DefaultAbstraction : IContentAbstraction
    {
        private Encoding _originalEncoding;
        private string[] _contentTypes = { "text/*" };
        private StringBuilder _content;

        public string[] ContentTypes => _contentTypes;

        public async Task<Stream> ReadAsStream()
        {
            var resultStream = new MemoryStream();

            StreamWriter writer = new StreamWriter(resultStream, _originalEncoding);

            await writer.WriteAsync(_content?.ToString());

            await writer.FlushAsync();

            resultStream.Position = 0;

            return resultStream;
        }

        public async Task Register(ModificationContext modificationContext, IJSEngine engine)
        {
            string content = string.Empty;

            var encoding = GetEncoding(modificationContext.ContentType);

            if (encoding == null)
            {
                return;
            }

            using (StreamReader reader = new StreamReader(modificationContext.ContentStream, encoding))
            {
                content = await reader.ReadToEndAsync();
            }

            _content = new StringBuilder(content);

            engine.InitializeContentApi(_content);
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
                return null;
            }

            return _originalEncoding = parsedHeader?.Encoding ?? _originalEncoding;
        }
    }
}
