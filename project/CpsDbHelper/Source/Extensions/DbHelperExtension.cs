namespace CpsDbHelper.Extensions
{
    public static class DbHelperExtension
    {
        /// <summary>
        /// Continue to proceed with a date reader helper. the db connection and transaction will be passed over
        /// </summary>
        public static DataReaderHelper ContinueWithReader<T>(this DbHelper<T> helper, string text) where T : DbHelper<T>
        {
            return new DataReaderHelper(text, helper.Connection, helper.Transaction);
        }

        /// <summary>
        /// Continue to proceed with a xml reader helper. the db connection and transaction will be passed over
        /// </summary>
        public static XmlReaderHelper ContinueWithXmlReader<T>(this DbHelper<T> helper, string text) where T : DbHelper<T>
        {
            return new XmlReaderHelper(text, helper.Connection, helper.Transaction);
        }

        /// <summary>
        /// Continue to proceed with a non-query helper. the db connection and transaction will be passed over
        /// </summary>
        public static NonQueryHelper ContinueWithNonQuery<T>(this DbHelper<T> helper, string text) where T : DbHelper<T>
        {
            return new NonQueryHelper(text, helper.Connection, helper.Transaction);
        }

        /// <summary>
        /// Continue to proceed with a scalar helper. the db connection and transaction will be passed over
        /// </summary>
        public static ScalarHelper<TS> ContinueWithScalar<T, TS>(this DbHelper<T> helper, string text) where T : DbHelper<T>
        {
            return new ScalarHelper<TS>(text, helper.Connection, helper.Transaction);
        }
    }
}