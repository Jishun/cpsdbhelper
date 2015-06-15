namespace CpsDbHelper.Utils
{
    /// <summary>
    /// Provides the basic context of dbHelpers
    /// </summary>
    public class DbHelperFactory
    {
        private readonly string _connectionString;

        public DbHelperFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataReaderHelper BeginReader(string text)
        {
            return new DataReaderHelper(text, _connectionString);
        }

        public XmlReaderHelper BeginXmlReader(string text)
        {
            return new XmlReaderHelper(text, _connectionString);
        }

        public NonQueryHelper BeginNonQuery(string text)
        {
            return new NonQueryHelper(text, _connectionString);
        }

        public ScalarHelper<T> BeginScalar<T>(string text)
        {
            return new ScalarHelper<T>(text, _connectionString);
        }
    }
}