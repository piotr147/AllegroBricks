﻿using AllegroBricks.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AllegroBricks.Utilities
{
    public static class DbUtilities
    {
        public static Subscriber GetOrCreateSubscriber(SqlConnection conn, string mail)
        {
            if (SubscriberExists(conn, mail))
            {
                return new Subscriber
                {
                    Id = GetExistingSubscriberId(conn, mail).Value,
                    Email = mail
                };
            }

            return new Subscriber
            {
                Id = CreateNewSubscriber(conn, mail),
                Email = mail
            };
        }

        public static List<Subscriber> GetAllSubscribersWithActiveSubscriptions(SqlConnection conn)
        {
            string query = @"
                select * from subscribers subers
                where subers.id in (
                    select subscriberId from subscriptions
                    group by subscriberId, isdeleted
                    having isdeleted = 0
                )
                and subers.isdeleted = 0;";

            SqlCommand cmd = new SqlCommand(query, conn);
            using SqlDataReader reader = cmd.ExecuteReader();

            return ReadAllSubscribers(reader);
        }

        public static List<LegoSet> GetAllSetsWithNotificationToSend(SqlConnection conn)
        {
            string query = @"
                select * from LegoSets
                where notificationToSend = 1";

            SqlCommand cmd = new SqlCommand(query, conn);
            using SqlDataReader reader = cmd.ExecuteReader();

            return ReadAllSets(reader);
        }

        public static List<Subscription> GetSubscriptionsOfSubscriberWithId(SqlConnection conn, int id)
        {
            string query = @"
                select * from subscriptions
                where subscriberId = @subscriberId
                and isdeleted = 0
                ;";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@subscriberId", SqlDbType.Int).Value = id;
            using SqlDataReader reader = cmd.ExecuteReader();

            return ReadAllSubscriptions(reader);
        }

        public static void UpdateSubscriptionsWithReportedPrices(SqlConnection conn, List<(Subscription subscription, LegoSet set)> subscriptionsWithSets)
        {
            foreach (var subSet in subscriptionsWithSets)
            {
                string query = @"
                    update Subscriptions
                    set lastReportedPrice = @lastReportedPrice,
                    lastUpdate = GETDATE()
                    where subscriberId = @subscriberId
                    and setNumber = @setNumber
                ;";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@lastReportedPrice", SqlDbType.Float).Value = subSet.set.LowestPrice;
                cmd.Parameters.Add("@subscriberId", SqlDbType.Int).Value = subSet.subscription.SubscriberId;
                cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = subSet.subscription.SetNumber;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateSetsWithNotifcationSent(SqlConnection conn)
        {
            string query = @"
                Update LegoSets
                set notificationToSend = 0
                ;";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.ExecuteNonQuery();
        }

        private static List<Subscription> ReadAllSubscriptions(SqlDataReader reader)
        {
            List<Subscription> result = new List<Subscription>();
            while (reader.Read())
            {
                result.Add(ReadSubscription(reader));
            }

            return result;
        }

        private static Subscription ReadSubscription(SqlDataReader reader) =>
            new Subscription
            {
                SubscriberId = reader.GetInt32(0),
                SetNumber = reader.GetInt32(1),
                LastReportedPrice = (decimal)reader.GetDouble(2),
                DiffPercent = reader.IsDBNull(3) ? null : (int?)reader.GetInt32(3),
                DiffPln = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4),
                LastUpdate = reader.GetDateTime(5),
                IsDeleted = reader.GetBoolean(6)
            };

        private static List<Subscriber> ReadAllSubscribers(SqlDataReader reader)
        {
            List<Subscriber> result = new List<Subscriber>();

            while (reader.Read())
            {
                result.Add(ReadSubscriber(reader));
            }

            return result;
        }

        private static Subscriber ReadSubscriber(SqlDataReader reader) =>
            new Subscriber
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1)
            };

        public static List<LegoSet> GetSetsOfActiveSubscriptionsWithNumberBeingReminderOf(SqlConnection conn, int remainder, int divisor)
        {
            string query = @"
                select * from LegoSets 
                where number in (
                    select setNumber from Subscriptions
                    group by setNumber, isDeleted
                    having isDeleted = 0 )
                and number % @divisor = @remainder
                ;";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@divisor", SqlDbType.Int).Value = divisor;
            cmd.Parameters.Add("@remainder", SqlDbType.Int).Value = remainder;
            using SqlDataReader reader = cmd.ExecuteReader();

            return ReadAllSets(reader);
        }

        private static List<LegoSet> ReadAllSets(SqlDataReader reader)
        {
            List<LegoSet> result = new List<LegoSet>();
            while (reader.Read())
            {
                result.Add(ReadSet(reader));
            }

            return result;
        }

        public static void UpdateSetInDb(SqlConnection conn, LegoSet set)
        {
            string query = @"
                Update LegoSets
                set allegroLowestUrl = @allegroLowestUrl,
                lowestPrice = @lowestPrice,
                notificationToSend = @notificationToSend,
                lastUpdate = GETDATE()
                ;";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@allegroLowestUrl", SqlDbType.VarChar).Value = set.AllegroLowestUrl;
            cmd.Parameters.Add("@lowestPrice", SqlDbType.Float).Value = set.LowestPrice;
            cmd.Parameters.Add("@notificationToSend", SqlDbType.Bit).Value = set.NotificationToSend;

            cmd.ExecuteNonQuery();
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

            return GetExistingSubscriberId(conn, mail).Value;
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
            using SqlDataReader reader = cmd.ExecuteReader();
            return ReadAllSubscriptionsToList(cmd.ExecuteReader());
        }

        private static List<SubscriptionToList> ReadAllSubscriptionsToList(SqlDataReader reader)
        {
            List<SubscriptionToList> subscriptions = new List<SubscriptionToList>();


            while (reader.Read())
            {
                subscriptions.Add(ReadAllSubscriptionToList(reader));
            }

            return subscriptions;
        }

        private static SubscriptionToList ReadAllSubscriptionToList(SqlDataReader reader) =>
            new SubscriptionToList
            {
                Mail = reader.GetString(0),
                CatalogNumber = reader.GetInt32(1),
                Name = reader.GetString(2),
                Series = reader.GetString(3),
                DiffPercent = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4),
                DiffPln = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5),
                LastReportedPrice = reader.IsDBNull(6) ? null : (decimal?)reader.GetDouble(6),
                LastUpdate = reader.GetDateTime(7),
            };

        private static int? GetExistingSubscriberId(SqlConnection conn, string mail)
        {
            string query = @"select id from Subscribers where email = @email and isdeleted = 0;";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = mail;
            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return reader.GetInt32(0);
            }

            return null;
        }

        public async static Task<LegoSet> GetOrCreateSet(SqlConnection conn, int number)
        {
            if (SetExists(conn, number))
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
            if (SubscriptionExists(conn, subscription))
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
                LastUpdate = reader.GetDateTime(10),
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
