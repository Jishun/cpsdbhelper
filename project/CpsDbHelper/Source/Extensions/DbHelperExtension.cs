using CpsDbHelper.Utils;

namespace CpsDbHelper.Extensions
{
    public static class DbHelperExtension
    {
        /// <summary>
        /// Continue to proceed with a date reader helper. the db connection and transaction will be passed over
        /// </summary>
        public static DataReaderHelper ContinueWithReader<T>(this DbHelper<T> helper, string text) where T : DbHelper<T>
        {
            return new DataReaderHelper(text, helper.Connection, helper.Transaction, helper.DbProvider);
        }

        /// <summary>
        /// Continue to proceed with a xml reader helper. the db connection and transaction will be passed over
        /// </summary>
        public static XmlReaderHelper ContinueWithXmlReader<T>(this DbHelper<T> helper, string text) where T : DbHelper<T>
        {
            return new XmlReaderHelper(text, helper.Connection, helper.Transaction, helper.DbProvider);
        }

        /// <summary>
        /// Continue to proceed with a non-query helper. the db connection and transaction will be passed over
        /// </summary>
        public static NonQueryHelper ContinueWithNonQuery<T>(this DbHelper<T> helper, string text) where T : DbHelper<T>
        {
            return new NonQueryHelper(text, helper.Connection, helper.Transaction, helper.DbProvider);
        }

        /// <summary>
        /// Continue to proceed with a scalar helper. the db connection and transaction will be passed over
        /// </summary>
        public static ScalarHelper<TS> ContinueWithScalar<T, TS>(this DbHelper<T> helper, string text) where T : DbHelper<T>
        {
            return new ScalarHelper<TS>(text, helper.Connection, helper.Transaction, helper.DbProvider);
        }

        public static DbHelper<T> UseSqlServer<T>(this DbHelper<T> helper) where T : DbHelper<T>
        {
            helper.DbProvider = new SqlServerDataProvider();
            return helper;
        }

        public static DbHelperFactory UseSqlServer(this DbHelperFactory factory)
        {
            factory.Provider = new SqlServerDataProvider();
            return factory;
        }
    }
}