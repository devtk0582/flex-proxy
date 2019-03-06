using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class ConsoleLog
    {
        public Guid Id { get; set; }
        public LogType LogType { get; set; }
        public string Data { get; set; }
        public string Message { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
