using AllegroBricks.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AllegroBricks.Utilities
{
    public static class EmailSender
    {
        private const string SENDER_EMAIL = "piotr.koskiewicz@gmail.com";
        private const string SENDER_NAME = "Brick buddy";
        private static readonly string EMAIL_SUBJECT = $"Lego update {DateTime.Now.Day:D2}.{DateTime.Now.Month:D2}";

        public static Task<Response> PrepareAndSendEmail(string email, List<(Subscription subscription, LegoSet set)> subscriptionsWithSets)
        {
            (string htmlMessage, string plainMessage) = PrepareMessages(subscriptionsWithSets);

            if (string.IsNullOrEmpty(htmlMessage) || string.IsNullOrEmpty(plainMessage)) return null;

            var senderMail = new EmailAddress(SENDER_EMAIL, SENDER_NAME);
            var receiverMail = new EmailAddress(email, "Brick buddy");
            SendGridMessage msg = MailHelper.CreateSingleEmail(senderMail, receiverMail, EMAIL_SUBJECT, plainMessage, htmlMessage);
            return SendGridClientFactory.CreateClient().SendEmailAsync(msg);
        }

        private static (string htmlMessage, string plainMessage) PrepareMessages(List<(Subscription subscription, LegoSet set)> subscriptionsWithSets)
        {
            StringBuilder htmlSb = new StringBuilder();
            StringBuilder plainSb = new StringBuilder();
            foreach (var subSet in subscriptionsWithSets)
            {
                if (!ShouldNotifyAboutSet(subSet)) continue;

                htmlSb.Append(GetPartOfHtmlMessageForSet(subSet));
                plainSb.Append(GetPartOfPlainMessageForSet(subSet));
            }

            return (htmlSb.ToString(), plainSb.ToString());
        }

        private static bool ShouldNotifyAboutSet((Subscription subscription, LegoSet set) subSet) =>
            AbsoluteDifferenceIsBigEnough(subSet)
            || PercentDifferenceIsBigEnough(subSet);

        private static bool AbsoluteDifferenceIsBigEnough((Subscription subscription, LegoSet set) subSet) =>
            Math.Abs(subSet.set.LowestPrice - subSet.subscription.LastReportedPrice) >= (decimal)subSet.subscription.DiffPln;

        private static bool PercentDifferenceIsBigEnough((Subscription subscription, LegoSet set) subSet) =>
            Math.Abs((subSet.set.LowestPrice - subSet.subscription.LastReportedPrice) / subSet.set.LowestPrice)
                >= (decimal)subSet.subscription.DiffPercent / 100m;

        private static string GetPartOfHtmlMessageForSet((Subscription subscription, LegoSet set) subSet)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(NameLineHtml(subSet.set));
            sb.Append(PriceInfoLineHtml(subSet.subscription.LastReportedPrice, subSet.set.LowestPrice));
            sb.Append(LowestUrlHtml(subSet.set.AllegroLowestUrl));

            return sb.ToString();
        }

        private static string NameLineHtml(LegoSet set) =>
            $@"Lego {set.Series} - {set.Number} - <strong>{set.Name}</strong>";

        private static string PriceInfoLineHtml(decimal priceFrom, decimal priceTo)
        {
            string verbToUse = priceTo < priceFrom ? "decreased" : "increased";
            string color = priceTo < priceFrom
                ? "Red"
                : "GreenYellow";

            return @$"
                <p style=""color:{color};"">
                    <strong>Price {verbToUse} from {priceFrom:0.00} to {priceTo:0.00}</strong>
                </p>";
        }

        private static string LowestUrlHtml(string url) =>
            $@"<a href=""{url}"">{url}</a>";

        private static string GetPartOfPlainMessageForSet((Subscription subscription, LegoSet set) subSet)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(NameLinePlain(subSet.set));
            sb.Append(PriceInfoLinePlain(subSet.subscription.LastReportedPrice, subSet.set.LowestPrice));
            sb.Append(subSet.set.AllegroLowestUrl);

            return sb.ToString();
        }

        private static string NameLinePlain(LegoSet set) =>
            $@"Lego {set.Series} - {set.Number} - {set.Name}";

        private static string PriceInfoLinePlain(decimal priceFrom, decimal priceTo)
        {
            string verbToUse = priceTo < priceFrom ? "decreased" : "increased";

            return @$"Price {verbToUse} from {priceFrom:0.00} to {priceTo:0.00}";
        }
    }
}
