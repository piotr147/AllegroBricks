using System;
using System.Collections.Generic;
using System.Text;

namespace AllegroBricks.Models
{
    public class SubscriptionToList
    {
        public string Mail { get; set; }
        public int CatalogNumber { get; set; }
        public string Name { get; set; }
        public string Series { get; set; }
        public int? DiffPercent { get; set; }
        public int? DiffPln { get; set; }
        public decimal? LastReportedPrice { get; set; }
        public DateTime LastUpdate { get; set; }

    }
}
