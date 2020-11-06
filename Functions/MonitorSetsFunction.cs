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

        [FunctionName("MonitorSets00")]
        public static async Task Run00([TimerTrigger("0 8/15 3-23 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                await AllegroSetUpdater.UpdateSetsWithNumberBeingReminderOf(0, NUMBER_OF_FUNCTIONS);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }

        [FunctionName("MonitorSets01")]
        public static async Task Run01([TimerTrigger("0 8/15 3-23 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                await AllegroSetUpdater.UpdateSetsWithNumberBeingReminderOf(1, NUMBER_OF_FUNCTIONS);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }

        [FunctionName("MonitorSets02")]
        public static async Task Run02([TimerTrigger("0 8/15 3-23 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                await AllegroSetUpdater.UpdateSetsWithNumberBeingReminderOf(2, NUMBER_OF_FUNCTIONS);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }

        [FunctionName("MonitorSets03")]
        public static async Task Run03([TimerTrigger("0 8/15 3-23 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                await AllegroSetUpdater.UpdateSetsWithNumberBeingReminderOf(3, NUMBER_OF_FUNCTIONS);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }

        [FunctionName("MonitorSets04")]
        public static async Task Run04([TimerTrigger("0 8/15 3-23 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                await AllegroSetUpdater.UpdateSetsWithNumberBeingReminderOf(4, NUMBER_OF_FUNCTIONS);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }

        [FunctionName("MonitorSets05")]
        public static async Task Run05([TimerTrigger("0 8/15 3-23 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                await AllegroSetUpdater.UpdateSetsWithNumberBeingReminderOf(5, NUMBER_OF_FUNCTIONS);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }

        [FunctionName("MonitorSets06")]
        public static async Task Run06([TimerTrigger("0 8/15 3-23 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                await AllegroSetUpdater.UpdateSetsWithNumberBeingReminderOf(6, NUMBER_OF_FUNCTIONS);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }

        [FunctionName("MonitorSets07")]
        public static async Task Run07([TimerTrigger("0 8/15 3-23 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                await AllegroSetUpdater.UpdateSetsWithNumberBeingReminderOf(7, NUMBER_OF_FUNCTIONS);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }

        [FunctionName("MonitorSets08")]
        public static async Task Run08([TimerTrigger("0 8/15 3-23 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                await AllegroSetUpdater.UpdateSetsWithNumberBeingReminderOf(8, NUMBER_OF_FUNCTIONS);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }

        [FunctionName("MonitorSets09")]
        public static async Task Run09([TimerTrigger("0 8/15 3-23 * * *")]TimerInfo myTimer, ILogger log)
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
