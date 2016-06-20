using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CpsDbHelper.Extensions;

namespace CpsDbHelper.Utils
{
    /// <summary>
    /// An utility class used to construct a table-valued type parameter, will produce a DataTable
    /// </summary>
    /// <typeparam name="T">The dbhelper</typeparam>
    /// <typeparam name="TValue">the type of the item of which properties are supposed to match the column defination of the user-defined type </typeparam>
    public class StructParameterConstructor<T, TValue> : Mapper<StructParameterConstructor<T, TValue>> where T : DbHelper<T>
    {
        private readonly string _parameterName;
        private readonly DbHelper<T> _dbHelper;
        private readonly IEnumerable<TValue> _source;
        private readonly IDictionary<string, Func<TValue, object>> _mapper = new Dictionary<string, Func<TValue, object>>();
        private readonly HashSet<string> _skip = new HashSet<string>();

        public StructParameterConstructor(string parameterName, DbHelper<T> dbHelper, IEnumerable<TValue> source = null)
        {
            _parameterName = parameterName;
            _dbHelper = dbHelper;
            _source = source;
        }

        /// <summary>
        /// Define a column name mapping
        /// </summary>
        /// <param name="columnName">the column name in the udt</param>
        /// <param name="filedSelector">the value selector of the item from the collection to map the table</param>
        /// <returns></returns>
        public StructParameterConstructor<T, TValue> MapField(string columnName, Func<TValue, object> filedSelector)
        {
            _mapper.Add(columnName, filedSelector);
            return this;
        }


        /// <summary>
        /// Auto map the TValue's properties and use the properties' names as columns' names
        /// </summary>
        /// <param name="enumToInt">convert enum to int, otherwise string</param>
        /// <param name="skips">the properties' names to skip</param>
        /// <returns></returns>
        public StructParameterConstructor<T, TValue> AutoMap(bool enumToInt = true, params string[] skips)
        {
            var type = (_source != null && _source.Any()) ? _source.First().GetType() : typeof (TValue);
            var properties = type.GetProperties(BindingFlag).Where(p => p.CanRead && !skips.Any(s => s.EndsWith(p.Name, StringComparison.OrdinalIgnoreCase)));

            foreach (var p in properties)
            {
                _mapper.Add(p.Name, obj => (enumToInt && p.PropertyType.IsEnum) ? (int)p.GetValue(obj) : p.GetValue(obj));
            }
            return this;
        }

        /// <summary>
        /// Auto map the TValue's properties and use the properties' names as columns' names
        /// </summary>
        /// <param name="tValue"></param>
        /// <param name="enumToInt">convert enum to int, otherwise string</param>
        /// <param name="skips">the properties' names to skip</param>
        /// <returns></returns>
        public StructParameterConstructor<T, TValue> AutoMap(Type tValue, bool enumToInt = true, params string[] skips)
        {
            var properties = tValue.GetProperties(BindingFlag).Where(p => p.CanRead && !skips.Any(s => s.EndsWith(p.Name, StringComparison.OrdinalIgnoreCase)));

            foreach (var p in properties)
            {
                _mapper.Add(p.Name, obj => (enumToInt && p.PropertyType.IsEnum) ? (int)p.GetValue(obj) : p.GetValue(obj));
            }
            return this;
        }
        
        /// <summary>
        /// if the item is a simple type. will produce a DataTable with 1 column and put the items' value in the column
        /// </summary>
        /// <param name="columnName">need to specify the column name</param>
        /// <param name="enumToInt">convert enum to int, otherwise string</param>
        /// <returns></returns>
        public StructParameterConstructor<T, TValue> MapBasicType(string columnName, bool enumToInt = true)
        {
            _mapper.Add(columnName, obj => (enumToInt && typeof(TValue).IsEnum) ? (int)(object)obj : (object)obj);
            return this;
        }

        /// <summary>
        /// Skip PropertName from automap, if dont want to specify them in the automap method
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public StructParameterConstructor<T, TValue> Skip(string propertyName)
        {
            if (!_skip.Contains(propertyName))
            {
                _skip.Add(propertyName);
            }
            return this;
        }

        /// <summary>
        /// Finish the mapping, put the DataTable to dbhelper with the parameter name, and return the context to the dbhelper
        /// </summary>
        /// <param name="source">the source colletion used to fill the data table, can be null if provided when constructing this mapper</param>
        /// <returns></returns>
        public DbHelper<T> FinishMap(IEnumerable<TValue> source = null)
        {
            source = source ?? _source;
            var t = new DataTable();
            foreach (var func in _mapper.Where(func => !_skip.Contains(func.Key)))
            {
                t.Columns.Add(new DataColumn(func.Key));
            }
            if (source != null)
            {
                foreach (var s in source)
                {
                    var row = t.NewRow();
                    foreach (var func in _mapper.Where(func => !_skip.Contains(func.Key)))
                    {
                        row[func.Key] = func.Value(s);
                    }
                    t.Rows.Add(row);
                }
            }
            return _dbHelper.AddStructParam(_parameterName, t);
        }
    }
}