using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class JavascriptModifier
    {
        public int Priority { get; set; }
        public string ModificationFunction { get; set; }
        public string DiscoveryFunction { get; set; }
        public string TargetContentType { get; set; }
        public RequestPhase RequestPhase { get; set; }
    }
}
