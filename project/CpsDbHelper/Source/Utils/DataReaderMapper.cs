using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace CpsDbHelper.Utils
{
    public class DataReaderMapper<TValue> : Mapper<DataReaderMapper<TValue>>
    {
        private readonly DataReaderHelper _helper;
        private readonly string _resultKey;
        private readonly IDictionary<string, Action<object, object>> _mapper = new Dictionary<string, Action<object, object>>();
        private readonly IList<Action<TValue, IDataReader>> _customMapper = new List<Action<TValue, IDataReader>>();

        protected override bool ForGet => false;

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
            var properties = typeof(TValue).GetProperties(BindingFlag).Where(p => p.CanWrite && !skips.Contains(p.Name));
            foreach (var p in properties)
            {
                if (_mapper.ContainsKey(p.Name.ToLower()))
                {
                    continue;
                }
                var u = Nullable.GetUnderlyingType(p.PropertyType);
                if ((u != null) && u.GetTypeInfo().IsEnum)
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
                        else if(value is string)
                        {
                            p.SetValue(item, Enum.Parse(u, (string)value));
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
                foreach (var action in _customMapper)
                {
                    action(item, reader);
                }
                foreach (var ordinal in ordinals)
                {
                    ordinal.Value(item, reader.IsDBNull(ordinal.Key) ? null : reader.GetValue(ordinal.Key));
                }
                ret.Add(item);
            }
            return ret;
        }
    }
}