using AllegroBricks.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AllegroBricks.Utilities
{
    public static class AllegroHtmlParser
    {
        private static readonly Regex SetInfoRegex = new Regex(@"https://allegro(.|[\n\r])*?Liczba(.|[\n\r])*?zł");
        private static readonly Regex SetUrlRegex = new Regex(@"https://allegro.*?""");
        private static readonly Regex PriceWithTrashRegex = new Regex(@"\d*,<(.|[\n\r])*?zł");

        public static async Task<LegoSet> UpdateSetInfo(LegoSet legoSet)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(legoSet.AllegroSearchUrl);
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
                (decimal price, string url)  = FindLowestPriceAndUrl(data);

                response.Close();
                readStream.Close();

                return legoSet.UpdateLastUpdate()
                    .WithAllegroLowestUrl(url)
                    .WithLowestPrice(price);
            }

            throw new Exception("Info about set not found");
        }

        private static (decimal price, string url) FindLowestPriceAndUrl(string data)
        {
            int triesNumber = 0;
            List<(decimal price, string url)> pricesAndUrls = new List<(decimal price, string url)>();
            MatchCollection matches = SetInfoRegex.Matches(data);

            foreach (Match match in matches)
            {
                try
                {
                    pricesAndUrls.Add(TryGetPriceAndUrl(match.Value));
                }
                catch (Exception)
                {
                }

                if (++triesNumber >= 10) break;
            }

            return pricesAndUrls.OrderBy(p => p.price).FirstOrDefault();
        }

        private static (decimal price, string url) TryGetPriceAndUrl(string setInfo)
        {
            setInfo = setInfo.Replace(" ", string.Empty).Replace("<!---->", string.Empty);
            string url = SetUrlRegex.Match(setInfo).Value.Replace("\"", string.Empty);
            string priceWithTrash = PriceWithTrashRegex.Match(setInfo).Value;
            int startIndexToRemove = priceWithTrash.IndexOf('<');
            int endIndexToRemove = priceWithTrash.IndexOf('>');
            string priceString = priceWithTrash
                .Remove(startIndexToRemove, endIndexToRemove - startIndexToRemove + 1)
                .Replace("zł", string.Empty)
                .Replace(',', '.');

            return (decimal.Parse(priceString), url);
        }
    }
}
