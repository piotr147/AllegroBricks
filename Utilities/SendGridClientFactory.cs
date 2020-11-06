using SendGrid;
using System;

namespace AllegroBricks.Utilities
{
    public static class SendGridClientFactory
    {
        public static SendGridClient CreateClient()
        {
            string key = Environment.GetEnvironmentVariable("sendgrid_key");
            return new SendGridClient(key);
        }
    }
}
