using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FlexProxy.ContentModificationMiddleware.ModificationEngine
{
    public class ModificationContext
    {
        public string ContentType { get; set; }
        public Stream ContentStream { get; set; }
        public Encoding ContentEncoding { set; get; }
        public MediaTypeHeaderValue ContentTypeHeader { get; set; }

        public ModificationContext(Stream contentStream, string contentType)
        {
            ContentStream = contentStream;
            ContentType = contentType;
            ExtractHttpData();
        }

        public async Task<string> GetContentStringAsync()
        {
            using (var reader = new StreamReader(ContentStream, ContentEncoding, true, 1024, true))
            {
                var content = await reader.ReadToEndAsync();
                ContentStream.Position = 0;
                return content;
            }
        }

        private void ExtractHttpData()
        {
            ContentEncoding = Encoding.UTF8;
            MediaTypeHeaderValue parsedHeader;
            MediaTypeHeaderValue.TryParse(ContentType, out parsedHeader);
            ContentTypeHeader = parsedHeader;
            ContentEncoding = parsedHeader?.Encoding ?? ContentEncoding;
        }
    }
}
