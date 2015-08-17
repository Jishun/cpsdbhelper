using System.Data;
using System.Data.SqlClient;

namespace CpsDbHelper.Utils
{
    /// <summary>
    /// Provides the basic context of dbHelpers
    /// </summary>
    public class DbHelperFactory
    {
        public SqlConnection DbConnection;
        public SqlTransaction DbTransaction;

        public readonly string ConnectionString;

        public DbHelperFactory(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DataReaderHelper BeginReader(string text)
        {
            if (CheckExistingConnection())
            {
                return new DataReaderHelper(text, DbConnection, DbTransaction);
            }
            return new DataReaderHelper(text, ConnectionString);
        }

        public XmlReaderHelper BeginXmlReader(string text)
        {
            if (CheckExistingConnection())
            {
                return new XmlReaderHelper(text, DbConnection, DbTransaction);
            }
            return new XmlReaderHelper(text, ConnectionString);
        }

        public NonQueryHelper BeginNonQuery(string text)
        {
            if (CheckExistingConnection())
            {
                return new NonQueryHelper(text, DbConnection, DbTransaction);
            }
            return new NonQueryHelper(text, ConnectionString);
        }

        public ScalarHelper<T> BeginScalar<T>(string text)
        {
            if (CheckExistingConnection())
            {
                return new ScalarHelper<T>(text, DbConnection, DbTransaction);
            }
            return new ScalarHelper<T>(text, ConnectionString);
        }

        public void Connect(bool beginTran = false, IsolationLevel transactionLevel = IsolationLevel.ReadCommitted)
        {
            DbConnection = new SqlConnection(ConnectionString);
            DbConnection.Open();
            if (beginTran)
            {
                BeginTransaction(transactionLevel);
            }
        }

        public void EndConnection(bool commitTran = true)
        {
            if (commitTran && DbTransaction != null)
            {
                CommitTransaction();
            }
            DbTransaction = null;
            DbConnection.Dispose();
        }

        public void BeginTransaction(IsolationLevel transactionLevel = IsolationLevel.ReadCommitted)
        {
            if (!CheckExistingConnection())
            {
                Connect();
            }
            if (DbTransaction == null)
            {
                DbTransaction = DbConnection.BeginTransaction(transactionLevel);
            }
        }

        public void CommitTransaction()
        {
            DbTransaction.Commit();
            DbTransaction = null;
        }

        public void RollbackTransaction()
        {
            DbTransaction.Rollback();
            DbTransaction = null;
        }

        private bool CheckExistingConnection()
        {
            if (DbConnection != null && DbConnection.State == ConnectionState.Open)
            {
                return true;
            }
            if (DbConnection == null)
            {
                return false;
            }
            DbConnection.Dispose();
            DbTransaction = null;
            DbConnection = null;
            return false;
        }
    }
}