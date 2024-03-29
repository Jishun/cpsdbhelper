using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using CpsDbHelper.Support;
using CpsDbHelper.Utils;

namespace CpsDbHelper
{
    public class NonQueryHelper : DbHelper<NonQueryHelper>
    {
        public NonQueryHelper(string text, string connectionString, IAdoNetProviderFactory provider)
            : base(text, connectionString, provider)
        {
        }

        public NonQueryHelper(string text, IDbConnection connection, IDbTransaction transaction, IAdoNetProviderFactory provider)
            : base(text, connection, transaction, provider)
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
                var stub = cmd as DbCommandStub;
                if (stub != null)
                {
                    await stub.ExecuteNonQueryAsync();
                    return;
                }
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
                return new NonQueryHelper(text, factory.DbConnection, factory.DbTransaction, factory.Provider);
            }
            return new NonQueryHelper(text, factory.ConnectionString, factory.Provider);
        }
    }
}