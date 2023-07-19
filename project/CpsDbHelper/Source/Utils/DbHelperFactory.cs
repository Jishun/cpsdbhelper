using System.Data;

namespace CpsDbHelper.Utils
{
    /// <summary>
    /// Provides the basic context of dbHelpers
    /// </summary>
    public class DbHelperFactory
    {
        public IDbConnection DbConnection;
        public IDbTransaction DbTransaction;
        public IAdoNetProviderFactory Provider;

        public readonly string ConnectionString;

        public DbHelperFactory(string connectionString, IAdoNetProviderFactory provider = null)
        {
            ConnectionString = connectionString;
            Provider = provider;
        }

        public void Connect(bool beginTran = false, IsolationLevel transactionLevel = IsolationLevel.ReadCommitted)
        {
            if (Provider == null)
            {
                Provider = new SqlServerDataProvider();
            }
            DbConnection = Provider.CreateConnection(ConnectionString);
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