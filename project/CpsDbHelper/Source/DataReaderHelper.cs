using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CpsDbHelper.Extensions;

namespace CpsDbHelper
{
    public class DataReaderHelper : DbHelper<DataReaderHelper>
    {
        private const string DefaultKey = "__default";
        private readonly IDictionary<string, object> _results = new Dictionary<string, object>();
        private readonly IDictionary<string, Func<IDataReader, DataReaderHelper, object>> _processDelegates = new Dictionary<string, Func<IDataReader, DataReaderHelper, object>>();
        private readonly IDictionary<string, Action<IDataReader>> _preActions = new Dictionary<string, Action<IDataReader>>();

        public DataReaderHelper(string text, string connectionString)
            : base(text, connectionString)
        {

        }

        public DataReaderHelper(string text, SqlConnection connection, SqlTransaction transaction)
            : base(text, connection, transaction)
        {

        }

        protected override void BeginExecute(SqlCommand cmd)
        {
            using (var reader = cmd.ExecuteReader())
            {
                ProcessReader(reader);
            }
        }

        protected override async Task BeginExecuteAsync(SqlCommand cmd)
        {
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                ProcessReader(reader);
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
        public DataReaderHelper DefineListResult<T>(Func<IDataReader, T> fun, string resultKey)
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

        private void ProcessReader(SqlDataReader reader)
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

        /// <summary>
        /// Utility class used to map columns with type's properties to fill a list of result
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        public class DataReaderMapper<TValue>
        {
            private readonly DataReaderHelper _helper;
            private readonly string _resultKey;
            private readonly IDictionary<string, Action<object, object>> _mapper = new Dictionary<string, Action<object, object>>();
            private readonly IList<Action<TValue, IDataReader>> _customMapper = new List<Action<TValue, IDataReader>>();

            public DataReaderMapper(DataReaderHelper helper, string resultKey)
            {
                _helper = helper;
                _resultKey = resultKey;
            }

            /// <summary>
            /// Put the value from specified column into the item's property value with func
            /// </summary>
            /// <typeparam name="TField">The type of the property</typeparam>
            /// <param name="columnName">The column's name from the reader result set</param>
            /// <param name="func">the function to set value using the column's value</param>
            /// <returns></returns>
            public DataReaderMapper<TValue> MapField<TField>(string columnName, Action<TValue, TField, DataReaderHelper> func)
            {
                _mapper.Add(columnName.ToLower(), (value, field) => func((TValue)value, (TField)field, _helper));
                return this;
            }

            /// <summary>
            /// Put the value from specified column into the item's property value with func
            /// </summary>
            /// <typeparam name="TField">The type of the property</typeparam>
            /// <param name="columnName">The column's name from the reader result set</param>
            /// <param name="func">the function to set value using the column's value</param>
            /// <returns></returns>
            public DataReaderMapper<TValue> MapField<TField>(string columnName, Action<TValue, TField> func)
            {
                _mapper.Add(columnName.ToLower(), (value, field) => func((TValue)value, (TField)field));
                return this;
            }

            /// <summary>
            /// Manually operate the DataReader and set the property value with func
            /// </summary>
            /// <param name="func">the function to operate on the current row's corresponding TValue item with DataReader</param>
            /// <returns></returns>
            public DataReaderMapper<TValue> MapField(Action<TValue, IDataReader> func)
            {
                _customMapper.Add(func);
                return this;
            }

            public DataReaderMapper<TValue> MapField(Action<TValue> func)
            {
                _customMapper.Add((value, reader) => func(value));
                return this;
            }

            /// <summary>
            /// Auto map the property names of TValue with columns names
            /// </summary>
            /// <param name="skips">property name's to skip</param>
            /// <returns></returns>
            public DataReaderMapper<TValue> AutoMap(params string[] skips)
            {
                var properties = typeof(TValue).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty).Where(p => p.CanWrite && !skips.Contains(p.Name));
                foreach (var p in properties)
                {
                    var u = Nullable.GetUnderlyingType(p.PropertyType);
                    if ((u != null) && u.IsEnum)
                    {
                        _mapper.Add(p.Name.ToLower(), (item, value) =>
                        {
                            if (value is int)
                            {
                                p.SetValue(item, Enum.ToObject(u, (int)value));
                            }
                            else if (value is short)
                            {
                                p.SetValue(item, Enum.ToObject(u, (short)value));
                            }
                            else if (value is long)
                            {
                                p.SetValue(item, Enum.ToObject(u, (long)value));
                            }
                            else if (value is byte)
                            {
                                p.SetValue(item, Enum.ToObject(u, (byte)value));
                            }
                        });
                    }
                    else
                    {
                        _mapper.Add(p.Name.ToLower(), p.SetValue);
                    }
                }
                return this;
            }

            /// <summary>
            /// Finsh the mapping  and return the context to the dbhelper
            /// </summary>
            /// <returns></returns>
            public DataReaderHelper FinishMap()
            {
                return _helper.DefineResult(MapperDelegate, _resultKey);
            }

            private object MapperDelegate(IDataReader reader, DataReaderHelper helper)
            {
                var ordinals = new Dictionary<int, Action<object, object>>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i).ToLower();
                    if (_mapper.ContainsKey(name))
                    {
                        var ordinal = reader.GetOrdinal(name);
                        if (!ordinals.ContainsKey(ordinal))
                        {
                            ordinals.Add(ordinal, _mapper[name]);
                        }
                    }
                }
                var ret = new List<TValue>();
                while (reader.Read())
                {
                    var item = Activator.CreateInstance<TValue>();
                    foreach (var ordinal in ordinals)
                    {

                        ordinal.Value(item, reader.IsDBNull(ordinal.Key) ? null : reader.GetValue(ordinal.Key));
                    }
                    foreach (var action in _customMapper)
                    {
                        action(item, reader);
                    }
                    ret.Add(item);
                }
                return ret;
            }
        }
    }
}