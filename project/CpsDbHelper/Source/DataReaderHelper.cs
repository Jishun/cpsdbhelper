using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CpsDbHelper.Extensions;
using CpsDbHelper.Utils;

namespace CpsDbHelper
{
    public class DataReaderHelper : DbHelper<DataReaderHelper>
    {
        private const string DefaultKey = "__default";
        private readonly IDictionary<string, object> _results = new Dictionary<string, object>();
        private readonly IDictionary<string, Func<IDataReader, DataReaderHelper, object>> _processDelegates = new Dictionary<string, Func<IDataReader, DataReaderHelper, object>>();
        private readonly IDictionary<string, Action<IDataReader>> _preActions = new Dictionary<string, Action<IDataReader>>();

        public DataReaderHelper(string text, string connectionString, IAdoNetProviderFactory provider)
            : base(text, connectionString, provider)
        {

        }

        public DataReaderHelper(string text, IDbConnection connection, IDbTransaction transaction, IAdoNetProviderFactory provider)
            : base(text, connection, transaction, provider)
        {

        }

        protected override void BeginExecute(IDbCommand cmd)
        {
            using (var reader = cmd.ExecuteReader())
            {
                ProcessReader(reader);
            }
        }

        protected override async Task BeginExecuteAsync(IDbCommand cmd)
        {
            var command = cmd as DbCommand;
            if (command != null)
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    ProcessReader(reader);
                }
            }
            else
            {
                throw new NotSupportedException("The async operation is not supported by this data provider");
            }
        }

        /// <summary>
        /// Define an action that executes right after the sqlCommand.execute() and before looping through the DataReader
        /// </summary>
        /// <param name="action">the action to apply to the DataReader</param>
        /// <param name="resultKey">The result set key which matches the define/map result method. the action will be applied before the key specified result set</param>
        /// <returns></returns>
        public virtual DataReaderHelper PreProcessResult(Action<IDataReader> action, string resultKey = DefaultKey)
        {
            _preActions.Add(resultKey, action);
            return this;
        }

        /// <summary>
        /// Define a result, use the reader to process result and return it to helper, later use the result key to get the result
        /// </summary>
        /// <param name="fun">the function that applied to the DataReader to process the result. the return value is the final result</param>
        /// <param name="resultKey">the identifier to find the reuslt later</param>
        /// <returns></returns>
        public DataReaderHelper DefineResult(Func<IDataReader, DataReaderHelper, object> fun, string resultKey = DefaultKey)
        {
            _processDelegates.Add(resultKey, fun);
            return this;
        }

        /// <summary>
        /// Define a result, use the reader to process result and return it to helper, later use the result key to get the result
        /// </summary>
        /// <param name="fun">the function that applied to the DataReader to process the result. the return value is the final result</param>
        /// <param name="resultKey">the identifier to find the reuslt later</param>
        /// <returns></returns>
        public DataReaderHelper DefineResult(Func<IDataReader, object> fun, string resultKey = DefaultKey)
        {
            _processDelegates.Add(resultKey, (reader, helper) => fun(reader));
            return this;
        }

        /// <summary>
        /// Define a result, use the reader to process result and return it to helper, later use the result key to get the result
        /// </summary>
        /// <param name="fun">the function that applied to the DataReader to process the result. the return value is the item of a list, the final result is the list of the item</param>
        /// <param name="resultKey">the identifier to find the reuslt later</param>
        /// <returns></returns>
        public DataReaderHelper DefineListResult<T>(Func<IDataReader, DataReaderHelper, T> fun, string resultKey = DefaultKey)
        {
            _processDelegates.Add(resultKey, (reader, helper) => ProcessListResult(reader, fun));
            return this;
        }

        /// <summary>
        /// Define a result, use the reader to process result and return it to helper, later use the result key to get the result
        /// </summary>
        /// <param name="fun">the function that applied to the DataReader to process the result. the return value is the item of a list, the final result is the list of the item</param>
        /// <param name="resultKey">the identifier to find the reuslt later</param>
        /// <returns></returns>
        public DataReaderHelper DefineListResult<T>(Func<IDataReader, T> fun, string resultKey = DefaultKey)
        {
            _processDelegates.Add(resultKey, (reader, helper) => ProcessListResult(reader, (rea, hel) => fun(rea)));
            return this;
        }

        /// <summary>
        /// Start mapping TValue with the result set, the row will be aligned with TValue in later operations
        /// </summary>
        /// <typeparam name="TValue">The type of the result which will be produced after proceesing this set of reader result</typeparam>
        /// <param name="resultKey">the identifier to find the reuslt with helper.GetResult()</param>
        /// <returns></returns>
        public DataReaderMapper<TValue> BeginMapResult<TValue>(string resultKey = DefaultKey)
        {
            return new DataReaderMapper<TValue>(this, resultKey);
        }

        /// <summary>
        /// Auto map property names with column names to TValue from the result set
        /// </summary>
        /// <typeparam name="TValue">The type of the result which will be produced after proceesing this set of reader result</typeparam>
        /// <param name="resultKey">the identifier to find the reuslt with helper.GetResult()</param>
        /// <returns></returns>
        public DataReaderHelper AutoMapResult<TValue>(string resultKey = DefaultKey)
        {
            return new DataReaderMapper<TValue>(this, resultKey).AutoMap().FinishMap();
        }

        /// <summary>
        /// Use this method to map the only one column from the reuslt set to a simple type
        /// </summary>
        /// <typeparam name="TValue">a C# simple type which the column's value will be converted to</typeparam>
        /// <param name="columnName">the column name of the reuslt set</param>
        /// <param name="resultKey">the identifier to find the reuslt with helper.GetResult()</param>
        /// <returns></returns>
        public DataReaderHelper DefineBasicTypeListResult<TValue>(string columnName, string resultKey = DefaultKey)
        {
            _processDelegates.Add(resultKey, (reader, helper) =>
                {
                    var ordinal = reader.GetOrdinal(columnName);
                    var ret = new List<TValue>();
                    while (reader.Read())
                    {
                        ret.Add(reader.Get<TValue>(ordinal));
                    }
                    return ret;
                });
            return this;
        }

        /// <summary>
        /// Get the result after Execute() is called. 
        /// previous DefineResult will return the return value of the function passed in
        /// DefineListResult<TValue>/MapResult<TValue>/AutoMapResult<TValue> will need to use IList<TValue> to retrieve the result
        /// </summary>
        /// <typeparam name="T">the type of the result to cast to</typeparam>
        /// <param name="key">The key used to define result</param>
        /// <returns></returns>
        public DataReaderHelper GetResult<T>(out T result, string key = DefaultKey)
        {
            result = (T)_results[key];
            return this;
        }

        /// <summary>
        /// Get the result after Execute() is called. 
        /// previous DefineResult will return the return value of the function passed in
        /// DefineListResult<TValue>/MapResult<TValue>/AutoMapResult<TValue> will need to use IList<TValue> to retrieve the result
        /// </summary>
        /// <typeparam name="T">the type of the result to cast to</typeparam>
        /// <param name="key">The key used to define result</param>
        /// <returns></returns>
        public T GetResult<T>(string key = DefaultKey)
        {
            return (T)_results[key];
        }

        /// <summary>
        /// Get the result after Execute() is called. 
        /// returns List<T> previous defined by DefineListResult<T>/MapResult<T>/AutoMapResult<T>
        /// </summary>
        /// <typeparam name="T">the type of the result to cast to</typeparam>
        /// <param name="key">The key used to define result</param>
        /// <returns></returns>
        public DataReaderHelper GetResultCollection<T>(out IList<T> result, string key = DefaultKey)
        {
            result = (IList<T>)_results[key];
            return this;
        }

        /// <summary>
        /// Get the result after Execute() is called. 
        /// </summary>
        /// <typeparam name="T">the type of the result to cast to</typeparam>
        /// <param name="key">The key used to define result</param>
        /// <returns>List<T> previous defined by DefineListResult<T>/MapResult<T>/AutoMapResult<T></returns>
        public IList<T> GetResultCollection<T>(string key = DefaultKey)
        {
            return (IList<T>)_results[key];
        }

        private object ProcessListResult<T>(IDataReader reader, Func<IDataReader, DataReaderHelper, T> fun)
        {
            var ret = new List<T>();
            while (reader.Read())
            {
                ret.Add(fun(reader, this));
            }
            return ret;
        }

        private void ProcessReader(IDataReader reader)
        {
            var first = true;
            foreach (var del in _processDelegates)
            {
                if (!first)
                {
                    if (!reader.NextResult())
                    {
                        return;
                    }
                }
                if (_preActions.ContainsKey(del.Key))
                {
                    _preActions[del.Key](reader);
                }
                var res = del.Value(reader, this);
                _results.Add(del.Key, res);
                first = false;
            }
        }
    }

    public static partial class FactoryExtensions
    {
        public static DataReaderHelper BeginReader(this DbHelperFactory factory, string text)
        {
            if (factory.CheckExistingConnection())
            {
                return new DataReaderHelper(text, factory.DbConnection, factory.DbTransaction, factory.Provider);
            }
            return new DataReaderHelper(text, factory.ConnectionString, factory.Provider);
        }
    }
}