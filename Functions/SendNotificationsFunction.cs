using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AllegroBricks.Models;
using AllegroBricks.Utilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid;

namespace AllegroBricks.Functions
{
    public static class SendNotificationsFunction
    {
        [FunctionName("SendNotifications")]
        public static async Task Run([TimerTrigger("0 0/15 4-23 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                await TrySendNotificationsAndUpdateSubscriptions(log);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }

        private static async Task TrySendNotificationsAndUpdateSubscriptions(ILogger log)
        {
            using SqlConnection conn = ConnectionFactory.CreateConnection();
            conn.Open();
            List<Subscriber> subscribers = DbUtilities.GetAllSubscribersWithActiveSubscriptions(conn);
            List<LegoSet> sets = DbUtilities.GetAllSetsWithNotificationToSend(conn);

            foreach (Subscriber subscriber in subscribers)
            {
                List<Subscription> subscriptions = DbUtilities.GetSubscriptionsOfSubscriberWithId(conn, subscriber.Id);
                List<(Subscription subscription, LegoSet set)> subscriptionsWithSets =
                    subscriptions.Join(
                        sets,
                        sub => sub.SetNumber,
                        set => set.Number,
                        (sub, set) => (sub, set))
                    .ToList();

                try
                {
                    Response response = await EmailSender.PrepareAndSendEmail(subscriber.Email, subscriptionsWithSets);
                    log.LogInformation(response?.StatusCode.ToString() ?? "Email has not been sent");
                    if (response == null
                        || response.StatusCode != HttpStatusCode.Accepted
                        && response.StatusCode != HttpStatusCode.OK)
                    {
                        continue;
                    }

                    DbUtilities.UpdateSubscriptionsWithReportedPrices(conn, subscriptionsWithSets);
                }
                catch (Exception e)
                {
                    log.LogError(e.Message);
                    throw;
                }
            }

            DbUtilities.UpdateSetsWithNotifcationSent(conn);
        }
    }
}
