using AllegroBricks.Models;
using System.Text;

namespace AllegroBricks.Utilities
{
    public static class AllegroService
    {
        private const string AllegroUrl = "https://allegro.pl/kategoria/klocki-lego-17865?";
        private static StringBuilder builder = new StringBuilder(AllegroUrl);

        public static string GetSearchUrl(LegoSet set)
        {
            builder = new StringBuilder(AllegroUrl);

            AppendSearchPhrase(set);
            AppendCondition();
            AppendPriceFrom(set);
            AppendSorting();

            string url = builder.ToString();
            builder.Clear();
            return url;
        }

        private static void AppendSearchPhrase(LegoSet set)
        {
            builder.Append($"&string=lego%20{set.Number}");
        }

        private static void AppendCondition()
        {
            builder.Append($"&stan=nowe");
        }

        private static void AppendPriceFrom(LegoSet set)
        {
            builder.Append($"&price_from={GetPriceFrom(set)}");
        }

        private static int GetPriceFrom(LegoSet set)
        {
            if(!set.CatalogPrice.HasValue)
            {
                return 100;
            }

            decimal basePrice = set.CatalogPrice.Value > set.LowestPrice
                ? set.CatalogPrice.Value
                : set.LowestPrice;
            decimal priceFrom = 0.4m * basePrice;

            return (int)priceFrom;
        }

        private static void AppendSorting()
        {
            builder.Append($"&order=p");
        }
    }
}
