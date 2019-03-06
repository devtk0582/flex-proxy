using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class HtmlAttributeCollection
    {
        private HtmlAgilityPack.HtmlAttributeCollection _attributeCollection;

        private List<HtmlAttribute> _attributes;

        public HtmlAttribute this[string name]
        {
            get
            {
                return _attributes.Where(attribute => attribute.Name == name).FirstOrDefault();
            }
        }

        public HtmlAttribute this[int index]
        {
            get
            {
                return _attributes[index];
            }
        }

        public int Count
        {
            get
            {
                return _attributes.Count;
            }
        }

        public HtmlAttributeCollection(HtmlAgilityPack.HtmlAttributeCollection attributeCollection)
        {
            _attributeCollection = attributeCollection;

            _attributes = attributeCollection.Select(attr => new HtmlAttribute(attr)).ToList();
        }

        public void Add(HtmlAttribute item)
        {
            _attributes.Add(item);
        }

        public bool Contains(string name)
        {
            return _attributes.Any(attr => attr.Name == name);
        }

        public void Insert(int index, HtmlAttribute item)
        {
            _attributes.Insert(index, item);
        }

        public void Remove()
        {
            _attributeCollection.Remove();
            _attributes.Clear();
        }

        public void Remove(string name)
        {
            _attributeCollection.Remove(name);
            _attributes.RemoveAll(attr => attr.Name == name);
        }

        public void RemoveAt(int index)
        {
            _attributeCollection.RemoveAt(index);
            _attributes.RemoveAt(index);
        }
    }
}
