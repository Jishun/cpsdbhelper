using System.Data;
using System.Data.SqlClient;

namespace CpsDbHelper.Utils
{
    /// <summary>
    /// Provides the basic context of dbHelpers
    /// </summary>
    public class DbHelperFactory
    {
        private readonly string _connectionString;
        private SqlConnection _dbConnection;
        private SqlTransaction _dbTransaction;

        public DbHelperFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataReaderHelper BeginReader(string text)
        {
            if (CheckExistingConnection())
            {
                return new DataReaderHelper(text, _dbConnection, _dbTransaction);
            }
            return new DataReaderHelper(text, _connectionString);
        }

        public XmlReaderHelper BeginXmlReader(string text)
        {
            if (CheckExistingConnection())
            {
                return new XmlReaderHelper(text, _dbConnection, _dbTransaction);
            }
            return new XmlReaderHelper(text, _connectionString);
        }

        public NonQueryHelper BeginNonQuery(string text)
        {
            if (CheckExistingConnection())
            {
                return new NonQueryHelper(text, _dbConnection, _dbTransaction);
            }
            return new NonQueryHelper(text, _connectionString);
        }

        public ScalarHelper<T> BeginScalar<T>(string text)
        {
            if (CheckExistingConnection())
            {
                return new ScalarHelper<T>(text, _dbConnection, _dbTransaction);
            }
            return new ScalarHelper<T>(text, _connectionString);
        }

        public void Connect(bool beginTran = false, IsolationLevel transactionLevel = IsolationLevel.ReadCommitted)
        {
            _dbConnection = new SqlConnection(_connectionString);
            if (beginTran)
            {
                BeginTransaction(transactionLevel);
            }
        }

        public void EndConnection(bool commitTran = true)
        {
            if (commitTran && _dbTransaction != null)
            {
                CommitTransaction();
            }
            _dbConnection.Dispose();
        }

        public void BeginTransaction(IsolationLevel transactionLevel = IsolationLevel.ReadCommitted)
        {
            if (!CheckExistingConnection())
            {
                Connect();
            }
            _dbTransaction = _dbConnection.BeginTransaction(transactionLevel);
        }

        public void CommitTransaction()
        {
            _dbTransaction.Commit();
        }

        public void RollbackTransaction()
        {
            _dbTransaction.Rollback();
        }

        private bool CheckExistingConnection()
        {
            if (_dbConnection != null && _dbConnection.State == ConnectionState.Open)
            {
                return true;
            }
            if (_dbConnection == null)
            {
                return false;
            }
            _dbConnection.Dispose();
            _dbConnection = null;
            return false;
        }
    }
}