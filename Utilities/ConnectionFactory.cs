using System;
using System.Data.SqlClient;

namespace AllegroBricks.Utilities
{
    public static class ConnectionFactory
    {
        public static SqlConnection CreateConnection()
        {
            string str = Environment.GetEnvironmentVariable("sqldb_connectionstring");
            return new SqlConnection(str);
        }
}
}
