using AllegroBricks.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AllegroBricks.Utilities
{
    public static class AllegroSetUpdater
    {
        private const int TimeoutMiliseconds = 10000;

        public async static Task UpdateSetsWithNumberBeingReminderOf(int remainder, int divisor)
        {
            using SqlConnection conn = ConnectionFactory.CreateConnection();
            conn.Open();
            List<LegoSet> sets = DbUtilities.GetSetsOfActiveSubscriptionsWithNumberBeingReminderOf(conn, remainder, divisor);
            foreach (LegoSet set in sets)
            {
                await UpdateSet(conn, set);
            }
        }
        private static async Task UpdateSet(SqlConnection conn, LegoSet set)
        {
            try
            {
                await AllegroHtmlParser.UpdateSetInfo(set).TimeoutAfter(TimeoutMiliseconds);
                DbUtilities.UpdateSetInDb(conn, set);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}