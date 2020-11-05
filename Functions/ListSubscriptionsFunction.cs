using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using AllegroBricks.Utilities;
using System.Net.Http;
using System.Collections.Generic;
using AllegroBricks.Models;
using System.Net;
using System.Text;

namespace AllegroBricks.Functions
{
    public static class ListSubscriptionsFunction
    {
        [FunctionName("ListSubscriptions")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string mail = req.Query["mail"];

            try
            {
                return TryListSubscriptions(mail);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }

        private static HttpResponseMessage TryListSubscriptions(string mail)
        {
            using SqlConnection conn = ConnectionFactory.CreateConnection();
            conn.Open();

            if (!DbUtilities.SubscriberExists(conn, mail))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            List<SubscriptionToList> subscribedSets = DbUtilities.GetActiveSubscriptionsOfUser(conn, mail);
            var json = JsonConvert.SerializeObject(new { subscriptions = subscribedSets }, Formatting.Indented);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
