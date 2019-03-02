using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class TransportItems
    {
        public List<string> Headers { get; set; }
        public List<TransientCookie> Cookies { get; set; }

        public TransportItems()
        {
            Headers = new List<string>();
            Cookies = new List<TransientCookie>();
        }
    }
}
