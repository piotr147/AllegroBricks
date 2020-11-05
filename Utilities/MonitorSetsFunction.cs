using AllegroBricks.Utilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AllegroBricks.Functions
{
    public static class MonitorSetsFunction
    {
        private const int NUMBER_OF_FUNCTIONS = 10;

        [FunctionName("MonitorSets0")]
        public static async Task Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                await AllegroSetUpdater.UpdateSetsWithNumberBeingReminderOf(9, NUMBER_OF_FUNCTIONS);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }
    }
}
