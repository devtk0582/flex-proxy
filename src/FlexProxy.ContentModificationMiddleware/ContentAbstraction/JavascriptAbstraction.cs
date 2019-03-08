using FlexProxy.ContentModificationMiddleware.ModificationEngine;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FlexProxy.ContentModificationMiddleware.ContentAbstraction
{
    public class JavascriptAbstraction : IContentAbstraction
    {
        private Encoding _originalEncoding;
        private string[] _contentTypes = { "text/javascript", "​text/x-javascript", "application/javascript", "application/x-javascript", "text/ecmascript", "application/ecmascript", "text/jscript" };
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
            string javascriptContent = string.Empty;

            using (StreamReader reader = new StreamReader(modificationContext.ContentStream, GetEncoding(modificationContext.ContentType)))
            {
                javascriptContent = await reader.ReadToEndAsync();
            }

            _content = new StringBuilder(javascriptContent);

            engine.InitializeJavascriptApi(_content);
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
