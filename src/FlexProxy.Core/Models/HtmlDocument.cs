using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class HtmlDocument
    {
        private HtmlAgilityPack.HtmlDocument _htmlDocument;

        public HtmlNode DocumentNode { get; private set; }

        private List<HtmlParseError> _parseErrors;
        public List<HtmlParseError> ParseErrors
        {
            get
            {
                if (_parseErrors == null && _htmlDocument.ParseErrors != null)
                {
                    _parseErrors = new List<HtmlParseError>();
                    foreach (var parseError in _htmlDocument.ParseErrors)
                    {
                        _parseErrors.Add(new HtmlParseError(parseError));
                    }
                }

                return _parseErrors;
            }
        }

        public HtmlDocument()
        {
            _htmlDocument = new HtmlAgilityPack.HtmlDocument();

        }

        public HtmlDocument(HtmlAgilityPack.HtmlDocument htmlDocument)
        {
            _htmlDocument = htmlDocument;

        }

        public void Load(Stream stream)
        {
            _htmlDocument.Load(stream);

            DocumentNode = new HtmlNode(_htmlDocument.DocumentNode);
        }

        public void LoadHtml(string html)
        {
            _htmlDocument.LoadHtml(html);

            DocumentNode = new HtmlNode(_htmlDocument.DocumentNode);
        }

        public void Save(Stream stream)
        {
            _htmlDocument.Save(stream);
        }

        public void Save(StreamWriter writer)
        {
            _htmlDocument.Save(writer);
        }

        public HtmlNode GetElementById(string id)
        {
            var htmlNode = _htmlDocument.GetElementbyId(id);

            return htmlNode == null ? null : new HtmlNode(htmlNode);
        }

        public HtmlNode CreateElement(string name)
        {
            var newElement = _htmlDocument.CreateElement(name);

            return newElement == null ? null : new HtmlNode(newElement);
        }
    }
}
