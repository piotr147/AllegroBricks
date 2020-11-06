using System;
using System.Collections.Generic;
using System.Text;

namespace AllegroBricks.Models
{
    public class EmailMessage
    {
        public string ReceiverEmail { get; set; }
        public string Html { get; set; }
        public string Plain { get; set; }
    }
}
