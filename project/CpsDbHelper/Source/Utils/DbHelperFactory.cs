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

        public bool CheckExistingConnection()
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