using System;
using System.Data.SqlClient;

namespace CpsDbHelper
{
    public class ScalarHelper<T> : DbHelper<ScalarHelper<T>>
    {
        private T _result;
        public ScalarHelper(string text, string connectionString)
            : base(text, connectionString)
        {
        }

        public ScalarHelper(string text, SqlConnection connection, SqlTransaction transaction)
            : base(text, connection, transaction)
        {
        }

        protected override void BeginExecute(SqlCommand cmd)
        {
            var ret = cmd.ExecuteScalar();
            _result = ret == DBNull.Value ? default(T) : (T)(dynamic)ret;
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
}