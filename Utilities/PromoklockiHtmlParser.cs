using AllegroBricks.Models;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AllegroBricks.Utilities
{
    public static class PromoklockiHtmlParser
    {
        private static readonly Regex TitleElementRegex = new Regex(@"LEGO<sup>&reg;</sup> \d{3,8}.*</h1>");
        private static readonly Regex TitleRegex = new Regex(@"\d{3,8}.*<");
        private static readonly Regex SeriesWithBorderRegex = new Regex(@"\d{3,8}.*-");
        private static readonly Regex CatalogNumberRegex = new Regex(@"\d{3,8}");
        private static readonly Regex CatalogPriceRegex = new Regex(@"Cena katalogowa.*?\d*,\d*");
        private static readonly Regex ReleaseYearRegex = new Regex(@"Rok wydania.*?\d{4}");
        private static readonly Regex LowestPriceEverRegex = new Regex(@"Najniższa cena</dt><dd class=""col-12 col-sm-8 col-md-6 col-lg-8"">\d*,\d*");
        private const string NajnizszaCenaElement = @"Najniższa cena</dt><dd class=""col-12 col-sm-8 col-md-6 col-lg-8"">";
        private const string PromoklockiSetsUrl = "https://promoklocki.pl/";

        public async static Task<LegoSet> GetSetInfo(int number)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GetUrl(number));
            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream;

                if (string.IsNullOrWhiteSpace(response.CharacterSet))
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                string data = readStream.ReadToEnd();
                string title = GetTitle(data);
                int catalogNumber = GetCatalogNumber(title);
                decimal? catalogPrice = GetCatalogPrice(data);
                int releaseYear = GetReleaseYear(data);
                string name = GetName(title);
                string series = GetSeries(title);
                decimal lowestPriceEver = GetLowestPriceEver(data);

                response.Close();
                readStream.Close();

                return new LegoSet
                {
                    Number = catalogNumber,
                    Name = name,
                    Series = series,
                    LowestPrice = catalogPrice ?? 99999,
                    LowestPriceEver = lowestPriceEver,
                    CatalogPrice = catalogPrice,
                    NotificationToSend = false,
                    LastUpdate = DateTime.Now,
                    ReleaseYear = releaseYear
                };
            }

            throw new Exception("Info about set not found");
        }

        private static string GetUrl(int number) =>
            $"{PromoklockiSetsUrl}{number}";

        private static int GetReleaseYear(string data)
        {
            string releaseYearWithTrash = ReleaseYearRegex.Match(data).Value;
            string releaseYearString = new Regex(@"\d{4}").Match(releaseYearWithTrash).Value;
            return int.Parse(releaseYearString);
        }

        private static decimal? GetCatalogPrice(string data)
        {
            try
            {
                string catalogPriceWithTrash = CatalogPriceRegex.Match(data).Value;
                int lastIndexBeforePrice = catalogPriceWithTrash.LastIndexOf('>');
                string catalogPriceString = catalogPriceWithTrash.Substring(lastIndexBeforePrice + 1).Replace(',', '.');
                return decimal.Parse(catalogPriceString);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetTitle(string doc)
        {
            string titleElement = TitleElementRegex.Match(doc).Value;
            string titleWithTrash = TitleRegex.Match(titleElement).Value;
            return titleWithTrash.Remove(titleWithTrash.Length - 1);
        }

        private static int GetCatalogNumber(string title) => int.Parse(CatalogNumberRegex.Match(title).Value);

        private static string GetName(string title)
        {
            int firstDashIndex = title.IndexOf('-');
            return title.Substring(firstDashIndex + 2);
        }

        private static string GetSeries(string title)
        {
            int firstDashIndex = title.IndexOf('-');
            string seriesWtihTrash = SeriesWithBorderRegex.Match(title.Remove(firstDashIndex + 1)).Value;
            return seriesWtihTrash.Remove(seriesWtihTrash.Length - 2, 2).Remove(0, 6);
        }

        private static decimal GetLowestPriceEver(string doc)
        {
            string priceWithTrash = LowestPriceEverRegex.Match(doc).Value;
            string price = priceWithTrash.Remove(0, NajnizszaCenaElement.Length).Replace(',', '.');
            return decimal.Parse(price);
        }
    }
}
