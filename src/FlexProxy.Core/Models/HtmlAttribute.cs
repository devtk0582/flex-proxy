using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class HtmlAttribute
    {
        private HtmlAgilityPack.HtmlAttribute _htmlAttribute;

        public string Name
        {
            get
            {
                return _htmlAttribute.Name;
            }
            set
            {
                _htmlAttribute.Name = value;
            }
        }

        public string Value
        {
            get
            {
                return _htmlAttribute.Value;
            }
            set
            {
                _htmlAttribute.Value = value;
            }
        }

        public int IndexOf(string value)
        {
            return this.Value.IndexOf(value);
        }

        public void Replace(string oldVal, string newVal)
        {
            this.Value = this.Value.Replace(oldVal, newVal);
        }

        public HtmlAttribute(HtmlAgilityPack.HtmlAttribute htmlAttribute)
        {
            _htmlAttribute = htmlAttribute;
        }

        public void Remove()
        {
            _htmlAttribute.Remove();
        }
    }
}
