using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class HtmlNodeCollection
    {
        private HtmlAgilityPack.HtmlNodeCollection _htmlNodeCollection;

        private List<HtmlNode> _htmlNodes;

        public HtmlNode this[string name]
        {
            get
            {
                return _htmlNodes.Where(node => node.Name == name).FirstOrDefault();
            }
        }

        public HtmlNode this[int index]
        {
            get
            {
                return _htmlNodes[index];
            }
        }

        public int Count
        {
            get
            {
                return _htmlNodes.Count;
            }
        }

        public HtmlNodeCollection(HtmlAgilityPack.HtmlNodeCollection htmlNodeCollection)
        {
            _htmlNodeCollection = htmlNodeCollection;

            _htmlNodes = htmlNodeCollection.Select(node => new HtmlNode(node)).ToList();
        }

        public void Add(HtmlNode item)
        {
            _htmlNodes.Add(item);
        }

        public bool Contains(string name)
        {
            return _htmlNodes.Any(attr => attr.Name == name);
        }

        public void Insert(int index, HtmlNode item)
        {
            _htmlNodes.Insert(index, item);
        }

        public void Remove(string name)
        {
            var elements = _htmlNodeCollection.Elements(name);

            foreach (var element in elements)
            {
                _htmlNodeCollection.Remove(element);
            }

            _htmlNodes.RemoveAll(attr => attr.Name == name);
        }

        public void RemoveAt(int index)
        {
            _htmlNodeCollection.RemoveAt(index);
            _htmlNodes.RemoveAt(index);
        }
    }
}
