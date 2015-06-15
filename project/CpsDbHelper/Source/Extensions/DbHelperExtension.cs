namespace CpsDbHelper.Extensions
{
    public static class DbHelperExtension
    {
        /// <summary>
        /// Continue to proceed with a date reader helper. the db connection and transaction will be passed over
        /// </summary>
        public static DataReaderHelper ContivnueWithReader<T>(this DbHelper<T> helper, string text) where T : DbHelper<T>
        {
            return new DataReaderHelper(text, helper.Connection, helper.Transaction);
        }

        /// <summary>
        /// Continue to proceed with a xml reader helper. the db connection and transaction will be passed over
        /// </summary>
        public static XmlReaderHelper ContivnueWithXmlReader<T>(this DbHelper<T> helper, string text) where T : DbHelper<T>
        {
            return new XmlReaderHelper(text, helper.Connection, helper.Transaction);
        }

        /// <summary>
        /// Continue to proceed with a non-query helper. the db connection and transaction will be passed over
        /// </summary>
        public static NonQueryHelper ContivnueWithNonQuery<T>(this DbHelper<T> helper, string text) where T : DbHelper<T>
        {
            return new NonQueryHelper(text, helper.Connection, helper.Transaction);
        }

        /// <summary>
        /// Continue to proceed with a scalar helper. the db connection and transaction will be passed over
        /// </summary>
        public static ScalarHelper<TS> ContivnueWithScalar<T, TS>(this DbHelper<T> helper, string text) where T : DbHelper<T>
        {
            return new ScalarHelper<TS>(text, helper.Connection, helper.Transaction);
        }
    }
}