using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.WebProxyMiddleware
{
    public class WebProxyOptions
    {
        public IEnumerable<string> InternalCookies { get; set; }
    }
}
