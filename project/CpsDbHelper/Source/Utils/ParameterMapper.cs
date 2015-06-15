using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace CpsDbHelper.Utils
{
    /// <summary>
    /// Mapper to automatically map item properties into stored procedure paramters with propert names
    /// Untested code
    /// </summary>
    /// <typeparam name="T">The target derived dbhelper type</typeparam>
    /// <typeparam name="TEntity">the item type used to provide param value</typeparam>
    public class ParameterMapper<T, TEntity> where T : DbHelper<T>
    {
        /// <summary>
        /// The mapping between c# types and sql db type. 
        /// </summary>
        public static readonly IDictionary<Type, SqlDbType> TypeMapping= new Dictionary<Type, SqlDbType>()
            {
                {typeof(int), SqlDbType.Int},
                {typeof(string), SqlDbType.NVarChar},
                {typeof(double), SqlDbType.Float},
                {typeof(decimal), SqlDbType.Decimal},
                //{typeof(Single), SqlDbType.Real},
                {typeof(float), SqlDbType.Decimal},
                {typeof(long), SqlDbType.Int},
                {typeof(short), SqlDbType.Int},
                {typeof(byte), SqlDbType.TinyInt},
                {typeof(byte[]), SqlDbType.Binary},
                {typeof(char), SqlDbType.TinyInt},
                {typeof(DateTime), SqlDbType.Date},
                {typeof(bool), SqlDbType.Bit},
                {typeof(Guid), SqlDbType.UniqueIdentifier},
            };

        private readonly DbHelper<T> _dbHelper;
        private readonly bool _enumToInt;

        public ParameterMapper(DbHelper<T> dbHelper, bool enumToInt = true)
        {
            _enumToInt = enumToInt;
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// Map a value into a sql parameter, the type of value must be mapable. otherwise this method go through without doing anything
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value">the parameter value, will try to auto detect the type</param>
        /// <returns></returns>
        public ParameterMapper<T, TEntity> MapProperty(string parameterName, object value)
        {
            var param = new SqlParameter {ParameterName = parameterName};
            if (value == null)
            {
                param.Value = DBNull.Value;
                _dbHelper.AddParam(param);
                return this;
            }
            SqlDbType? type = null;
            var valueType = value.GetType();
            if (valueType.IsGenericType &&
                valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                valueType = valueType.GetGenericArguments()[0];
            }
            if (valueType == typeof(XElement))
            {
                type = SqlDbType.Xml;
                value = value.ToString();
            }
            else if (TypeMapping.ContainsKey(valueType))
            {
                type = TypeMapping[valueType];
            }
            if (type.HasValue)
            {
                if (valueType == typeof(string))
                {
                    param.Size = ((string) value).Length;
                }

                if(valueType == typeof(Guid))
                {
                    param.Value = new System.Data.SqlTypes.SqlGuid((Guid)value);
                }

                param.Value = value;
                _dbHelper.AddParam(param);
            }
            return this;
        }
        
        /// <summary>
        /// Try auto map all the mapable properties of item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public ParameterMapper<T, TEntity> AutoMap(TEntity item, params string[] skips)
        {
            var properties = typeof(TEntity).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty).Where(p => p.CanRead);

            foreach (var p in properties)
            {
                if (skips.Any(s => s.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                var u = Nullable.GetUnderlyingType(p.PropertyType);
                var v = p.GetValue(item);

                var isCollection = (!(typeof(String) == p.PropertyType) && typeof(IEnumerable).IsAssignableFrom(p.PropertyType));

                if (isCollection)
                {
                    var type = p.PropertyType.GetElementType() ?? p.PropertyType.GetGenericArguments()[0];
                    new StructParameterConstructor<T, object>(p.Name, _dbHelper)
                        .AutoMap(type, _enumToInt)
                        .FinishMap(((IEnumerable)v).Cast<object>());                  
                }
                else
                {

                    MapProperty(p.Name, _enumToInt
                        ? u == null
                            ? p.PropertyType.IsEnum ? (int) v : v
                            : (u.IsEnum
                                ? v == null ? null : (object) (int) v
                                : v)
                        : v);
                }
            }
            return this;
        }

        /// <summary>
        /// Finish the mapping and return the context to the dbhelper
        /// </summary>
        /// <returns></returns>
        public T FinishMap()
        {
            return (T)_dbHelper;
        }
    }
}
