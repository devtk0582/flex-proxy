using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.WebProxyMiddleware
{
    public class HostMappingOptions
    {
        public string ServingHost { get; set; }

        public string ServingScheme { get; set; }

        public string DownstreamHost { get; set; }

        public string DownstreamScheme { get; set; }

        public string ServingHostWithScheme
        {
            get
            {
                return $"{ServingScheme}{ServingHost}";
            }
        }

        public string DownstreamHostWithScheme
        {
            get
            {
                return $"{DownstreamScheme}{DownstreamHost}";
            }
        }
    }
}
