using System;
using System.Collections.Generic;
using System.Text;

namespace AllegroBricks.Models
{
    public class Subscription
    {
        public int SubscriberId { get; set; }
        public int SetNumber { get; set; }
        public decimal LastReportedPrice { get; set; }
        public int? DiffPercent { get; set; }
        public int? DiffPln { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
