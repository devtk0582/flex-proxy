using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class RangeHeader
    {
        public long? Start { get; set; }
        public long? End { get; set; }

        public RangeHeader(long? start, long? end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}
