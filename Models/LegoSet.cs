using System;

namespace AllegroBricks.Models
{
    public class LegoSet
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string Series { get; set; }
        public decimal Price { get; set; }
        public string AllegroSearchUrl { get; set; }
        public string AllegroLowestUrl { get; set; }
        public decimal LowestPrice { get; set; }
        public string LowestSeller { get; set; }
        public decimal? LowestPriceEver { get; set; }
        public decimal? CatalogPrice { get; set; }
        public bool NotificationToSend { get; set; }
        public DateTime LastPriceUpdate { get; set; }
        public int ReleaseYear { get; set; }

        public LegoSet WithAllegroSearchUrl(string url)
        {
            AllegroSearchUrl = url;
            return this;
        }
    }
}
