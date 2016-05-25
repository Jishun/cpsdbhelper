using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CpsDbHelper.Utils;

namespace CpsDbHelper
{
    public class NonQueryHelper : DbHelper<NonQueryHelper>
    {
        public NonQueryHelper(string text, string connectionString)
            : base(text, connectionString)
        {
        }

        public NonQueryHelper(string text, IDbConnection connection, IDbTransaction transaction)
            : base(text, connection, transaction)
        {
        }

        protected override void BeginExecute(IDbCommand cmd)
        {
            cmd.ExecuteNonQuery();
        }

        protected override async Task BeginExecuteAsync(IDbCommand cmd)
        {
            var command = cmd as DbCommand;
            if (command != null)
            {
                await command.ExecuteNonQueryAsync();
            }
            else
            {
                throw new NotSupportedException("The async operation is not supported by this data provider");
            }
        }
    }

    public static partial class FactoryExtensions
    {
        public static NonQueryHelper BeginNonQuery(this DbHelperFactory factory, string text)
        {
            if (factory.CheckExistingConnection())
            {
                return new NonQueryHelper(text, factory.DbConnection, factory.DbTransaction);
            }
            return new NonQueryHelper(text, factory.ConnectionString);
        }
    }
}