using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class HtmlParseError
    {
        private HtmlAgilityPack.HtmlParseError _htmlParseError;

        public string Code
        {
            get
            {
                return _htmlParseError.Code.ToString();
            }
        }

        public int Line
        {
            get
            {
                return _htmlParseError.Line;
            }
        }

        public int LinePosition
        {
            get
            {
                return _htmlParseError.LinePosition;
            }
        }

        public string Reason
        {
            get
            {
                return _htmlParseError.Reason;
            }
        }

        public HtmlParseError(HtmlAgilityPack.HtmlParseError htmlParseError)
        {
            _htmlParseError = htmlParseError;
        }
    }
}
