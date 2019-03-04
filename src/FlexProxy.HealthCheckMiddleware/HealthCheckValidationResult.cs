using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.HealthCheckMiddleware
{
    public class HealthCheckValidationResult
    {
        public HealthCheckReason Reason { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public bool Success { get; set; }
    }

    public enum HealthCheckReason
    {
        HealthCheckText = 0,
        InvalidHtml = 1,
        SlowResponseTime = 2,
        ErrorCallingUrl = 3
    }
}
