using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace FlexProxy.HealthCheckMiddleware
{
    public class HealthCheckOptions
    {
        public IPAddress Localhost { get; set; }
        public int MaxResponseTimeInSeconds { get; set; }
    }
}
