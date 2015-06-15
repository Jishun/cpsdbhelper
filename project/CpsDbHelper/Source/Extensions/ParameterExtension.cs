using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using CpsDbHelper.Utils;

namespace CpsDbHelper.Extensions
{
    public static class ParameterExtension
    {
        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType) where T : DbHelper<T>
        {
            return helper.AddParam(new SqlParameter(parameterName, dbType));
        }

        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, object value) where T : DbHelper<T>
        {
            return helper.AddParam(new SqlParameter(parameterName, value));
        }

        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType, object value) where T : DbHelper<T>
        {
            return helper.AddParam(new SqlParameter(parameterName, dbType) { Value = value });
        }

        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType, int size, object value) where T : DbHelper<T>
        {
            return helper.AddParam(new SqlParameter(parameterName, dbType, size) { Value = value });
        }

        /// <summary>
        /// Add an outbound sql parameter
        /// </summary>
        public static T AddOutParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType) where T : DbHelper<T>
        {
            return helper.AddParam(new SqlParameter(parameterName, dbType) { Direction = ParameterDirection.Output });
        }

        /// <summary>
        /// Add an outbound sql parameter
        /// </summary>
        public static T AddOutParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType, int size) where T : DbHelper<T>
        {
            return helper.AddParam(new SqlParameter(parameterName, dbType, size) { Direction = ParameterDirection.Output });
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType) where T : DbHelper<T>
        {
            return helper.AddParam(new SqlParameter(parameterName, dbType) { Direction = ParameterDirection.InputOutput });
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, object value) where T : DbHelper<T>
        {
            return helper.AddParam(new SqlParameter(parameterName, value) { Direction = ParameterDirection.InputOutput });
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType, object value) where T : DbHelper<T>
        {
            return helper.AddParam(new SqlParameter(parameterName, dbType) { Value = value, Direction = ParameterDirection.InputOutput });
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType, int size, object value) where T : DbHelper<T>
        {
            return helper.AddParam(new SqlParameter(parameterName, dbType, size) { Value = value, Direction = ParameterDirection.InputOutput });
        }

        /// <summary>
        /// Start mapping a structured parameter, will turn the call context into the mapper. and map the table-valued type's columns with the item's properties
        /// </summary>
        /// <typeparam name="T">the dbhelper</typeparam>
        /// <typeparam name="TValue">The type of the items which is to be mapped with each row of the data table</typeparam>
        /// <param name="helper"></param>
        /// <param name="parameterName">the structured param name</param>
        /// <returns></returns>
        public static StructParameterConstructor<T, TValue> BeginAddStructParam<T, TValue>(this DbHelper<T> helper, string parameterName) where T : DbHelper<T>
        {
            return new StructParameterConstructor<T, TValue>(parameterName, helper);
        }

        /// <summary>
        /// auto map a collection of type TValue and construct a data table which uses the property names as column name and put values in
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper"></param>
        /// <param name="parameterName"></param>
        /// <param name="value">the item collection</param>
        /// <param name="enumToInt">convert enum to int, set false then to string</param>
        /// <param name="skips">the propert names to skip</param>
        /// <returns></returns>
        public static T AutoMapStructParam<T, TValue>(this DbHelper<T> helper, string parameterName, IEnumerable<TValue> value, bool enumToInt = true, params string[] skips) where T : DbHelper<T>
        {
            return (T)(new StructParameterConstructor<T, TValue>(parameterName, helper).AutoMap(enumToInt, skips).FinishMap(value));
        }

        /// <summary>
        /// Begin map an item to stored procedure parameters, will turn the context into the mapper till the finishMap() is called
        /// </summary>
        /// <typeparam name="T">The dbhelper</typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="helper"></param>
        /// <param name="item"></param>
        /// <param name="enumToInt">convert enum to int, otherwise string</param>
        /// <returns></returns>
        public static ParameterMapper<T, TEntity> BeginMapParam<T, TEntity>(this DbHelper<T> helper, TEntity item, bool enumToInt = true) where T : DbHelper<T>
        {
            return new ParameterMapper<T, TEntity>(helper, enumToInt);
        }

        /// <summary>
        /// Try automatically map the item's propertis into stored procedure's parameters, will try to detect the types, skip those which are not mappable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TEntity">the item's type</typeparam>
        /// <param name="helper"></param>
        /// <param name="item">the object instance which provides propertoes</param>
        /// <param name="enumToInt">convert enum to int, otherwise string</param>
        /// <returns></returns>
        public static T AutoMapParam<T, TEntity>(this DbHelper<T> helper, TEntity item, bool enumToInt = true) where T : DbHelper<T>
        {
            return (new ParameterMapper<T,TEntity>(helper, enumToInt)).AutoMap(item).FinishMap();
        }

        #region parameters detailed
        public static T AddIntInParam<T>(this DbHelper<T> helper, string parameterName, int? value)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Int, value);
        }

        public static T AddBigIntInParam<T>(this DbHelper<T> helper, string parameterName, long? value)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.BigInt, value);
        }

        public static T AddVarcharInParam<T>(this DbHelper<T> helper, string parameterName, string value)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.VarChar, value == null ? 0 : value.Length, (object)value);
        }

        public static T AddNvarcharInParam<T>(this DbHelper<T> helper, string parameterName, string value)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.NVarChar, value == null ? 0 : value.Length, (object)value);
        }

        public static T AddXmlInParam<T>(this DbHelper<T> helper, string parameterName, XElement value)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Xml, value == null ? null : value.ToString());
        }

        public static T AddBitInParam<T>(this DbHelper<T> helper, string parameterName, bool? value)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Bit, value);
        }

        public static T AddDateTimeInParam<T>(this DbHelper<T> helper, string parameterName, DateTime? value)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.DateTime, value);
        }

        public static T AddDecimalInParam<T>(this DbHelper<T> helper, string parameterName, decimal? value)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Decimal, value);
        }

        public static T AddFloatInParam<T>(this DbHelper<T> helper, string parameterName, float? value)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Float, value);
        }

        public static T AddSmallIntInParam<T>(this DbHelper<T> helper, string parameterName, short? value)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.SmallInt, value);
        }

        public static T AddTinyIntInParam<T>(this DbHelper<T> helper, string parameterName, char? value)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.TinyInt, value);
        }

        public static T AddGuidInParam<T>(this DbHelper<T> helper, string parameterName, Guid? value) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.UniqueIdentifier, value == null ? null : (object)new System.Data.SqlTypes.SqlGuid(value.Value));
        }

        public static T AddBinaryInParam<T>(this DbHelper<T> helper, string parameterName, Byte[] value)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.VarBinary, value );
        }

        public static T AddStructParam<T>(this DbHelper<T> helper, string parameterName, DataTable value) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Structured, value);
        }

        //TODO: tobe defined
        #endregion parameters detailed
    }
}
