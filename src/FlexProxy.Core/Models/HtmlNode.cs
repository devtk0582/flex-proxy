using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class HtmlNode
    {
        private HtmlAgilityPack.HtmlNode _htmlNode;

        private HtmlAttributeCollection _attributes;
        public HtmlAttributeCollection Attributes
        {
            get
            {
                if (_attributes == null && _htmlNode.Attributes != null)
                {
                    _attributes = new HtmlAttributeCollection(_htmlNode.Attributes);
                }

                return _attributes;
            }
        }

        private HtmlNodeCollection _childNodes;
        public HtmlNodeCollection ChildNodes
        {
            get
            {
                if (_childNodes == null && _htmlNode.ChildNodes != null)
                {
                    _childNodes = new HtmlNodeCollection(_htmlNode.ChildNodes);
                }

                return _childNodes;
            }
        }

        private HtmlNode _parentNode;
        public HtmlNode ParentNode
        {
            get
            {
                if (_parentNode == null && _htmlNode.ParentNode != null)
                {
                    _parentNode = new HtmlNode(_htmlNode.ParentNode);
                }

                return _parentNode;
            }
        }

        public string Id
        {
            get
            {
                return _htmlNode.Id;
            }
            set
            {
                _htmlNode.Id = value;
            }
        }

        public string Name
        {
            get
            {
                return _htmlNode.Name;
            }
            set
            {
                _htmlNode.Name = value;
            }
        }

        public string InnerHtml
        {
            get
            {
                return _htmlNode.InnerHtml;
            }
            set
            {
                _htmlNode.InnerHtml = value;
            }
        }

        public string InnerText
        {
            get
            {
                return _htmlNode.InnerText;
            }
        }

        public string OuterHtml
        {
            get
            {
                return _htmlNode.OuterHtml;
            }
        }

        public bool HasChildNodes
        {
            get
            {
                return _htmlNode.HasChildNodes;
            }
        }

        public bool HasAttributes
        {
            get
            {
                return _htmlNode.HasAttributes;
            }
        }

        public HtmlNode(HtmlAgilityPack.HtmlNode htmlNode)
        {
            _htmlNode = htmlNode;
        }

        public HtmlNode SelectSingleNode(string xpath)
        {
            var node = _htmlNode.SelectSingleNode(xpath);

            return node == null ? null : new HtmlNode(node);
        }

        public List<HtmlNode> SelectNodes(string xpath)
        {
            return _htmlNode.SelectNodes(xpath)?.Select(node => new HtmlNode(node)).ToList();
        }

        public HtmlAttribute GetAttributeByName(string name)
        {
            return Attributes[name];
        }

        public void SetAttributeValue(string name, string value)
        {
            var attribute = Attributes[name];

            if (attribute == null)
            {
                HtmlAgilityPack.HtmlAttribute newAttribute = _htmlNode.OwnerDocument.CreateAttribute(name, value);
                _htmlNode.Attributes.Add(newAttribute);
                Attributes.Add(new HtmlAttribute(newAttribute));
            }
            else
            {
                attribute.Value = value;
            }
        }

        public List<HtmlNode> Descendants(string name)
        {
            return _htmlNode.Descendants(name)?.Select(node => new HtmlNode(node)).ToList();
        }

        public List<HtmlNode> Descendants()
        {
            return _htmlNode.Descendants()?.Select(node => new HtmlNode(node)).ToList();
        }

        public HtmlNode Element(string name)
        {
            var element = _htmlNode.Element(name);

            return element == null ? null : new HtmlNode(element);
        }

        public List<HtmlNode> Elements(string name)
        {
            return _htmlNode.Elements(name)?.Select(element => new HtmlNode(element)).ToList();
        }

        public HtmlNode PrependChild(string type)
        {
            var newElement = _htmlNode.OwnerDocument.CreateElement(type);

            _htmlNode.PrependChild(newElement);

            return new HtmlNode(newElement);
        }

        public HtmlNode AppendChild(string type)
        {
            var newElement = _htmlNode.OwnerDocument.CreateElement(type);

            _htmlNode.AppendChild(newElement);

            return new HtmlNode(newElement);
        }

        public HtmlNode InsertChild(int index, string type)
        {
            var newElement = _htmlNode.OwnerDocument.CreateElement(type);

            _htmlNode.ChildNodes.Insert(index, newElement);

            var newNode = new HtmlNode(newElement);

            ChildNodes.Insert(index, newNode);

            return newNode;
        }

        public void Remove()
        {
            _htmlNode.Remove();
        }

        public void RemoveAll()
        {
            _htmlNode.RemoveAll();
        }

        public void RemoveAllChildren()
        {
            _htmlNode.RemoveAllChildren();
        }

        public void Replace(string oldValue, string newValue)
        {
            if (oldValue != null && newValue != null)
            {
                _htmlNode.InnerHtml = _htmlNode.InnerHtml.Replace(oldValue, newValue);
            }
        }
    }
}
