using FlexProxy.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.HostedObjects
{
    public interface IHtmlDocumentApi : IApiHostObject
    {
        void Initialize(HtmlDocument htmlDocument);
        bool IsLoaded();
        HtmlNode GetElementById(string Id);
        HtmlNode GetElementByXPath(string path);
        HtmlNode GetHiddenFieldElement(string name);
        HtmlNode GetHiddenFieldElement(string name, string value);
        HtmlNode GetElementByAttributes(string path, string[] attributeNames, string[] attributeValues);
        string GetOuterHtml();
        HtmlNode CreateElement(string name);
        HtmlNode AppendElementToPath(string path, string type);
        List<HtmlNode> GetElementsByName(string name);
        List<HtmlNode> GetElementsByTagName(string name);
        List<HtmlNode> GetElementsByClassName(string name);
        void Save();
    }

    public class HtmlDocumentApi : IHtmlDocumentApi
    {
        private HtmlDocument _htmlDocument;

        public HtmlDocumentApi() { }

        public HtmlDocumentApi(HtmlDocument htmlDocument)
        {
            _htmlDocument = htmlDocument;

            InitializeConfigurations();
        }

        public void Initialize(HtmlDocument htmlDocument)
        {
            _htmlDocument = htmlDocument;

            InitializeConfigurations();
        }

        public void CleanUp()
        {
            _htmlDocument = null;
        }

        private void InitializeConfigurations()
        {
            HtmlAgilityPack.HtmlNode.ElementsFlags["button"] = HtmlAgilityPack.HtmlElementFlag.Closed | HtmlAgilityPack.HtmlElementFlag.Empty;
        }

        public bool IsLoaded()
        {
            return _htmlDocument != null && _htmlDocument.DocumentNode.HasChildNodes;
        }

        public HtmlNode GetElementById(string Id)
        {
            if (!IsLoaded())
            {
                return null;
            }

            return _htmlDocument.GetElementById(Id);
        }

        public HtmlNode GetElementByXPath(string path)
        {
            if (!IsLoaded())
            {
                return null;
            }

            return _htmlDocument.DocumentNode.SelectSingleNode(path);
        }

        public HtmlNode GetHiddenFieldElement(string name)
        {
            if (!IsLoaded())
            {
                return null;
            }

            return GetElementByAttributes("input", new string[] { "name" }, new string[] { name });
        }

        public HtmlNode GetHiddenFieldElement(string name, string value)
        {
            if (!IsLoaded())
            {
                return null;
            }

            return GetElementByAttributes("input", new string[] { "name", "value" }, new string[] { name, value });
        }

        public HtmlNode GetElementByAttributes(string path, string[] attributeNames, string[] attributeValues)
        {
            if (!IsLoaded())
            {
                return null;
            }

            HtmlNode htmlNode = null;

            var root = _htmlDocument.DocumentNode;

            if (attributeNames == null && attributeValues == null)
            {
                htmlNode = root.SelectSingleNode(path);
            }
            else if (attributeNames.Length == attributeValues.Length)
            {
                var elements = root.Descendants(path).AsEnumerable();

                for (int i = 0; i < attributeNames.Length; i++)
                {
                    var attributeName = attributeNames[i];

                    var attributeValue = attributeValues[i];

                    elements = elements.Where(node => node.Attributes[attributeName] != null && node.Attributes[attributeName].Value == attributeValue);
                }

                htmlNode = elements.FirstOrDefault();
            }

            return htmlNode;
        }

        public string GetOuterHtml()
        {
            if (!IsLoaded())
            {
                return null;
            }

            return _htmlDocument.DocumentNode.OuterHtml;
        }

        public HtmlNode CreateElement(string name)
        {
            if (!IsLoaded())
            {
                return null;
            }

            return _htmlDocument.CreateElement(name);
        }

        public HtmlNode AppendElementToPath(string path, string type)
        {
            if (!IsLoaded())
            {
                return null;
            }

            var parentElement = _htmlDocument.DocumentNode.SelectSingleNode(path);

            if (parentElement != null)
            {
                var newElement = parentElement.AppendChild(type);

                return newElement;
            }

            return null;
        }

        public List<HtmlNode> GetElementsByName(string name)
        {
            if (!IsLoaded())
            {
                return null;
            }

            return _htmlDocument.DocumentNode.SelectNodes($"//*[@name=\"{name}\"]");
        }

        public List<HtmlNode> GetElementsByTagName(string name)
        {
            if (!IsLoaded())
            {
                return null;
            }

            return _htmlDocument.DocumentNode.SelectNodes($"//{name}");
        }

        public List<HtmlNode> GetElementsByClassName(string name)
        {
            if (!IsLoaded())
            {
                return null;
            }

            return _htmlDocument.DocumentNode.SelectNodes($"//*[@class=\"{name}\"]");
        }

        public void Save()
        {

        }
    }
}
