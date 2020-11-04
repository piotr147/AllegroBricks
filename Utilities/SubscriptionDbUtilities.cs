using AllegroBricks.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AllegroBricks.Utilities
{
    public static class SubscriptionDbUtilities
    {
        public static Subscriber GetOrCreateSubscriber(SqlConnection conn, string mail)
        {
            if(SubscriberExists(conn, mail))
            {
                return new Subscriber
                {
                    Id = ExistingSubscriber(conn, mail).Value,
                    Email = mail
                };
            }

            return new Subscriber
            {
                Id = CreateNewSubscriber(conn, mail),
                Email = mail
            };
        }

        public static bool SubscriberExists(SqlConnection conn, string mail)
        {
            string query = @"select count(*) from Subscribers where email = @email and isdeleted = 0;";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = mail;

            return (int)cmd.ExecuteScalar() > 0;
        }

        private static int CreateNewSubscriber(SqlConnection conn, string mail)
        {
            string query = @"insert into Subscribers values(@email, 0);";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = mail;
            cmd.ExecuteNonQuery();

            return ExistingSubscriber(conn, mail).Value;
        }

        internal static List<SubscriptionToList> GetActiveSubscriptionsOfUser(SqlConnection conn, string mail)
        {
            string query = @"
                Select s2.email, st.number, st.name, st.series, s1.diffPercent, s1.diffPln, s1.lastReportedPrice, s1.lastUpdate
                from LegoSets st
                join subscriptions s1 on s1.setnumber = st.number
                join subscribers s2 on s2.id = s1.subscriberid
                where s2.email = @email and s1.isdeleted = 0";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = mail;

            return ReadSubscriptionsToList(cmd.ExecuteReader());
        }

        private static List<SubscriptionToList> ReadSubscriptionsToList(SqlDataReader reader)
        {
            List<SubscriptionToList> subscriptions = new List<SubscriptionToList>();

            using (reader)
            {
                while (reader.Read())
                {
                    subscriptions.Add(new SubscriptionToList
                    {
                        Mail = reader.GetString(0),
                        CatalogNumber = reader.GetInt32(1),
                        Name = reader.GetString(2),
                        Series = reader.GetString(3),
                        DiffPercent = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4),
                        DiffPln = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5),
                        LastReportedPrice = reader.IsDBNull(6) ? null : (decimal?)reader.GetDouble(6),
                        LastUpdate = reader.GetDateTime(7),
                    });
                }
            }

            return subscriptions;
        }

        private static int? ExistingSubscriber(SqlConnection conn, string mail)
        {
            string query = @"select id from Subscribers where email = @email and isdeleted = 0;";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = mail;
            using SqlDataReader reader = cmd.ExecuteReader();

            if(reader.Read())
            {
                return reader.GetInt32(0);
            }

            return null;
        }

        public async static Task<LegoSet> GetOrCreateSet(SqlConnection conn, int number)
        {
            if(SetExists(conn, number))
            {
                return GetExistingSet(conn, number);
            }

            LegoSet newSet = await PromoklockiHtmlParser.GetSetInfo(number);
            newSet.WithAllegroSearchUrl(AllegroService.GetSearchUrl(newSet));
            AddSetToDb(conn, newSet);

            return newSet;
        }

        private static void AddSetToDb(SqlConnection conn, LegoSet newSet)
        {
            string query = @"
                insert into LegoSets values(
                @number,
                @name,
                @series,
                @allegroSearchUrl,
                null,
                @lowestPrice,
                null,
                @lowestPriceEver,
                @catalogPrice,
                0,
                GETDATE(),
                @releaseYear
                );";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@number", SqlDbType.Int).Value = newSet.Number;
            cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = newSet.Name;
            cmd.Parameters.Add("@series", SqlDbType.VarChar).Value = newSet.Series;
            cmd.Parameters.Add("@allegroSearchUrl", SqlDbType.VarChar).Value = newSet.AllegroSearchUrl;
            cmd.Parameters.Add("@lowestPrice", SqlDbType.Float).Value = newSet.LowestPrice;
            cmd.Parameters.Add("@lowestPriceEver", SqlDbType.Float).Value = newSet.LowestPriceEver;
            cmd.Parameters.Add("@catalogPrice", SqlDbType.Float).Value = (object)newSet.CatalogPrice ?? DBNull.Value;
            cmd.Parameters.Add("@releaseYear", SqlDbType.Int).Value = newSet.ReleaseYear;
            cmd.ExecuteNonQuery();
        }

        private static LegoSet GetExistingSet(SqlConnection conn, int number)
        {
            string query = @"select * from LegoSets where number = @number;";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@number", SqlDbType.Int).Value = number;
            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return ReadSet(reader);
            }

            return null;
        }

        public static void CreateOrUpdateSubscriptionInDb(SqlConnection conn, Subscription subscription)
        {
            if(SubscriptionExists(conn, subscription))
            {
                UpdateExistingSubscription(conn, subscription);
                return;
            }

            AddNewSubscription(conn, subscription);
        }

        private static bool SubscriptionExists(SqlConnection conn, Subscription subscription)
        {
            string query = @"
                select count(*) from Subscriptions
                where subscriberId = @subscriberId and setNumber = @setNumber;";
            
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@subscriberId", SqlDbType.Int).Value = subscription.SubscriberId;
            cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = subscription.SetNumber;

            return (int)cmd.ExecuteScalar() > 0;
        }

        private static void UpdateExistingSubscription(SqlConnection conn, Subscription subscription)
        {
            string query = @"
                update Subscriptions set
                lastReportedPrice = @lastReportedPrice,
                diffPercent = @diffPercent,
                diffPln = @diffPln,
                lastUpdate = GETDATE(),
                isdeleted = 0
                where subscriberId = @subscriberId and setNumber = @setNumber;";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@subscriberId", SqlDbType.Int).Value = subscription.SubscriberId;
            cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = subscription.SetNumber;
            cmd.Parameters.Add("@lastReportedPrice", SqlDbType.Float).Value = subscription.LastReportedPrice;
            cmd.Parameters.Add("@diffPercent", SqlDbType.Int).Value = subscription.DiffPercent;
            cmd.Parameters.Add("@diffPln", SqlDbType.Int).Value = subscription.DiffPln;

            cmd.ExecuteNonQuery();
        }

        public static void AddNewSubscription(SqlConnection conn, Subscription subscription)
        {
            string query = @"
                insert into Subscriptions values
                (@subscriberId, @setNumber, @lastReportedPrice, @diffPercent, @diffPln, GETDATE(), 0);";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@subscriberId", SqlDbType.Int).Value = subscription.SubscriberId;
            cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = subscription.SetNumber;
            cmd.Parameters.Add("@lastReportedPrice", SqlDbType.Float).Value = subscription.LastReportedPrice;
            cmd.Parameters.Add("@diffPercent", SqlDbType.Int).Value = subscription.DiffPercent;
            cmd.Parameters.Add("@diffPln", SqlDbType.Int).Value = subscription.DiffPln;

            cmd.ExecuteNonQuery();
        }

        private static LegoSet ReadSet(SqlDataReader reader)
        {
            return new LegoSet
            {
                Number = reader.GetInt32(0),
                Name = reader.GetString(1),
                Series = reader.GetString(2),
                AllegroSearchUrl = reader.GetString(3),
                AllegroLowestUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                LowestPrice = (decimal)reader.GetDouble(5),
                LowestSeller = reader.IsDBNull(6) ? null : reader.GetString(6),
                LowestPriceEver = reader.IsDBNull(7) ? null : (decimal?)reader.GetDouble(7),
                CatalogPrice = reader.IsDBNull(8) ? null : (decimal?)reader.GetDouble(8),
                NotificationToSend = reader.GetBoolean(9),
                LastPriceUpdate = reader.GetDateTime(10),
                ReleaseYear = reader.GetInt32(11)
            };
        }

        private static bool SetExists(SqlConnection conn, int number)
        {
            string query = @"select count(*) from LegoSets where number = @number;";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@number", SqlDbType.Int).Value = number;

            return (int)cmd.ExecuteScalar() > 0;
        }
    }
}
