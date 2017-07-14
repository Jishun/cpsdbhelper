using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CpsDbHelper.Utils;

namespace CpsDbHelper
{
    public class ScalarHelper<T> : DbHelper<ScalarHelper<T>>
    {
        private T _result;
        public ScalarHelper(string text, string connectionString, IAdoNetProviderFactory provider)
            : base(text, connectionString, provider)
        {
        }

        public ScalarHelper(string text, IDbConnection connection, IDbTransaction transaction, IAdoNetProviderFactory provider)
            : base(text, connection, transaction, provider)
        {
        }

        protected override void BeginExecute(IDbCommand cmd)
        {
            var ret = cmd.ExecuteScalar();
            _result = ret == DBNull.Value ? default(T) : (T)(dynamic)ret;
        }

        protected override async Task BeginExecuteAsync(IDbCommand cmd)
        {
            var command = cmd as DbCommand;
            if (command != null)
            {

                var ret = await command.ExecuteScalarAsync();
                _result = ret == DBNull.Value ? default(T) : (T)(dynamic)ret;
            }
            else
            {
                throw new NotSupportedException("The async operation is not supported by this data provider");
            }
        }

        /// <summary>
        /// The get the scalar result and cast to T
        /// </summary>
        public ScalarHelper<T> GetResult(out T result)
        {
            result = _result;
            return this;
        }

        /// <summary>
        /// The get the scalar result and cast to T
        /// </summary>
        public T GetResult()
        {
            return _result;
        }
    }

    public static partial class FactoryExtensions
    {
        public static ScalarHelper<T> BeginScalar<T>(this DbHelperFactory factory,  string text)
        {
            if (factory.CheckExistingConnection())
            {
                return new ScalarHelper<T>(text, factory.DbConnection, factory.DbTransaction, factory.Provider);
            }
            return new ScalarHelper<T>(text, factory.ConnectionString, factory.Provider);
        }
    }
}