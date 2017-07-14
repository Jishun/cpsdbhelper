using CpsDbHelper.Utils;
using Npgsql;
using System;

namespace CpsDbHelper.Postgres
{
    public static class CpsPostgresDbExtensions
    {

        public static DbHelper<T> UsePostgressql<T>(this DbHelper<T> helper) where T : DbHelper<T>
        {
            helper.DbProvider = new SqlServerDataProvider();
            return helper;
        }

        public static DbHelperFactory UsePostgresql(this DbHelperFactory factory)
        {
            factory.Provider = new PostgresSqlDataProvider();
            return factory;
        }
    }
}
