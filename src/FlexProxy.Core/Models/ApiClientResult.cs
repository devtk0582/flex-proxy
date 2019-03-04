using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class ApiClientResult<ResType, ResErrorType>
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public ResType Result { get; set; }
        public ResErrorType ErrorResult { get; set; }
        public TimeSpan ResponseTime { get; set; }
    }
}
