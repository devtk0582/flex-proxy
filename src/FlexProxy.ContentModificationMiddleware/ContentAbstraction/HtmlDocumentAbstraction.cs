using FlexProxy.ContentModificationMiddleware.ModificationEngine;
using FlexProxy.Core.Models;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FlexProxy.ContentModificationMiddleware.ContentAbstraction
{
    public class HtmlDocumentAbstraction : IContentAbstraction
    {
        private HtmlDocument _htmlDocument;
        private Encoding _originalEncoding;
        private string[] _contentTypes = { "text/html" };

        public string[] ContentTypes => _contentTypes;

        public async Task Register(ModificationContext modificationContext, IJSEngine engine)
        {
            string html = string.Empty;

            using (StreamReader reader = new StreamReader(modificationContext.ContentStream, GetEncoding(modificationContext.ContentType)))
            {
                html = await reader.ReadToEndAsync();
            }

            if (!string.IsNullOrEmpty(html))
            {
                _htmlDocument = new HtmlDocument();

                _htmlDocument.LoadHtml(html);
            }

            engine.InitializeHtmlDocumentApi(_htmlDocument);
        }

        public async Task<bool> ValidateAsync(ModificationContext context)
        {
            var content = await context.GetContentStringAsync();

            if (content.Length == 0)
            {
                return false;
            }

            var length = content.Length >= 100 ? 100 : content.Length;

            var htmlStart = content
                .TrimStart()
                .Substring(0, length)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

            return htmlStart.StartsWith("<");
        }

        private Encoding GetEncoding(string contentType)
        {
            if (_originalEncoding != null)
            {
                return _originalEncoding;
            }

            _originalEncoding = Encoding.UTF8;

            MediaTypeHeaderValue parsedHeader;

            if (!MediaTypeHeaderValue.TryParse(contentType, out parsedHeader))
            {
                throw new InvalidOperationException("ContentType header could not be parsed");
            }

            return _originalEncoding = parsedHeader?.Encoding ?? _originalEncoding;
        }


        public async Task<Stream> ReadAsStream()
        {
            var resultStream = new MemoryStream();

            StreamWriter writer = new StreamWriter(resultStream, _originalEncoding);

            await writer.WriteAsync(_htmlDocument?.DocumentNode.OuterHtml);

            await writer.FlushAsync();

            resultStream.Position = 0;

            return resultStream;
        }
    }
}
