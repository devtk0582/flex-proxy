using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class RequestTraceLoggerServiceOptions
    {
        public string LoggerConfigName { get; set; }
        public int Interval { get; set; }
        public string ProcessName { get; set; }
    }
}
