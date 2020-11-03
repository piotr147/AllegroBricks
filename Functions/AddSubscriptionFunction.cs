using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AllegroBricks.Utilities;
using System.Data.SqlClient;
using AllegroBricks.Models;

namespace AllegroBricks
{
    public static class AddSubscriptionFunction
    {
        [FunctionName("AddSubscriptionFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);


            try
            {
                string mail = data?.mail;
                string number = data?.number;
                int? diffPercent = null;
                if (int.TryParse((string)data?.diffPercent, out int parsedDiffPercent))
                {
                    diffPercent = parsedDiffPercent;
                }

                int? diffPln = null;
                if (int.TryParse((string)data?.diffPln, out int parsedDiffPln))
                {
                    diffPln = parsedDiffPln;
                }

                await AddNewSubscription(mail, number, diffPercent, diffPln);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }


            return new OkObjectResult("Subscription has been added");
        }

        private async static Task AddNewSubscription(string mail, string number, int? diffPercent, int? diffPln)
        {
            string str = Environment.GetEnvironmentVariable("sqldb_connectionstring");
            using SqlConnection conn = new SqlConnection(str);
            conn.Open();
            Subscriber subscriber = SubscriptionDbUtilities.GetOrCreateSubscriber(conn, mail);
            LegoSet set = await SubscriptionDbUtilities.GetOrCreateSet(conn, int.Parse(number));
            Subscription subscription = CreateSubscription(subscriber, set, diffPercent, diffPln);
            SubscriptionDbUtilities.CreateOrUpdateSubscriptionInDb(conn, subscription);
        }

        private static Subscription CreateSubscription(
            Subscriber subscriber,
            LegoSet set,
            int? diffPercent,
            int? diffPln) =>
            new Subscription
            {
                SubscriberId = subscriber.Id,
                SetNumber = set.Number,
                LastReportedPrice = set.LowestPrice,
                DiffPercent = diffPercent,
                DiffPln = diffPln,
                LastUpdate = DateTime.Now,
                IsDeleted = false
            };
    }
}
