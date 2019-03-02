using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class TransientCookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public TransientCookieAction Action { get; set; }
    }
}
