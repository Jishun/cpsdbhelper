using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using CpsDbHelper.Utils;

namespace CpsDbHelper.Extensions
{
    public static class SqlParameterExtension
    {
        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, DbType dbType) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, dbType);
            p.DbType = dbType;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddParam(helper.CreateParameter(parameterName, value ?? DBNull.Value));
        }

        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, DbType dbType, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, useDbNullIfNull ? (value ?? DBNull.Value) : value);
            p.DbType = dbType;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, DbType dbType, int size, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, useDbNullIfNull ? (value ?? DBNull.Value) : value);
            p.DbType = dbType;
            p.Size = size;
            p.Value = useDbNullIfNull ? (value ?? DBNull.Value) : value;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an outbound sql parameter
        /// </summary>
        public static T AddOutParam<T>(this DbHelper<T> helper, string parameterName, DbType dbType) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName);
            p.DbType = dbType;
            p.Direction = ParameterDirection.Output;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an outbound sql parameter
        /// </summary>
        public static T AddOutParam<T>(this DbHelper<T> helper, string parameterName, DbType dbType, int size) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName);
            p.DbType = dbType;
            p.Direction = ParameterDirection.Output;
            p.Size = size;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, DbType dbType) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName);
            p.DbType = dbType;
            p.Direction = ParameterDirection.InputOutput;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, object  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, useDbNullIfNull ? (value ?? DBNull.Value) : value);
            p.Direction = ParameterDirection.InputOutput;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, DbType dbType, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, useDbNullIfNull ? (value ?? DBNull.Value) : value);
            p.Direction = ParameterDirection.InputOutput;
            p.DbType = dbType;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, DbType dbType, int size, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, dbType);
            p.Size = size;
            p.Direction = ParameterDirection.InputOutput;
            p.Value = useDbNullIfNull ? (value ?? DBNull.Value) : value;
            return helper.AddParam(p);
        }
        
        /// <summary>
        /// Begin map an item to command parameters, will turn the context into the mapper till the finishMap() is called
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
        /// Begin map an item to command parameters, will turn the context into the mapper till the finishMap() is called
        /// </summary>
        /// <typeparam name="T">The dbhelper</typeparam>
        /// <param name="helper"></param>
        /// <param name="item"></param>
        /// <param name="enumToInt">convert enum to int, otherwise string</param>
        /// <returns></returns>
        public static ParameterMapper<T, object> BeginMapParam<T>(this DbHelper<T> helper, object item, bool enumToInt = true) where T : DbHelper<T>
        {
            return new ParameterMapper<T, object>(helper, enumToInt);
        }

        /// <summary>
        /// Try automatically map the item's propertis/fields into command parameters, will try to detect the types, skip those which are not mappable
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

        /// <summary>
        /// Try automatically map the item's propertis/fields into command parameters, will try to detect the types, skip those which are not mappable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="helper"></param>
        /// <param name="item">the object instance which provides propertoes</param>
        /// <param name="enumToInt">convert enum to int, otherwise string</param>
        /// <returns></returns>
        public static T AutoMapParam<T>(this DbHelper<T> helper, object item, bool enumToInt = true) where T : DbHelper<T>
        {
            return (new ParameterMapper<T, object>(helper, enumToInt)).AutoMap(item).FinishMap();
        }

        #region parameters detailed
        public static T AddInt32InParam<T>(this DbHelper<T> helper, string parameterName, int? value, bool useDbNullIfNull = false)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Int32, value, useDbNullIfNull);
        }

        public static T AddInt64InParam<T>(this DbHelper<T> helper, string parameterName, long? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Int64, value, useDbNullIfNull);
        }

        public static T AddAnsiStringInParam<T>(this DbHelper<T> helper, string parameterName, string value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.AnsiString, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }
        public static T AddStringFixedLengthInParam<T>(this DbHelper<T> helper, string parameterName, string  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.StringFixedLength, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }

        public static T AddAnsiStringFixedLengthInParam<T>(this DbHelper<T> helper, string parameterName, string  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.AnsiStringFixedLength, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }

        public static T AddStringInParam<T>(this DbHelper<T> helper, string parameterName, string value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.String, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }

        public static T AddXmlInParam<T>(this DbHelper<T> helper, string parameterName, XElement value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Xml, value == null ? null : value.ToString(), useDbNullIfNull);
        }
        public static T AddXmlInParam<T>(this DbHelper<T> helper, string parameterName, string value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Xml, value, useDbNullIfNull);
        }

        public static T AddBooleanInParam<T>(this DbHelper<T> helper, string parameterName, bool? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Boolean, value, useDbNullIfNull);
        }

        public static T AddDateTime2InParam<T>(this DbHelper<T> helper, string parameterName, DateTime?  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.DateTime2, value, useDbNullIfNull);
        }

        public static T AddDateTimeInParam<T>(this DbHelper<T> helper, string parameterName, DateTime? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.DateTime, value, useDbNullIfNull);
        }

        public static T AddDateInParam<T>(this DbHelper<T> helper, string parameterName, DateTime?  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Date, value, useDbNullIfNull);
        }
        public static T AddTimeInParam<T>(this DbHelper<T> helper, string parameterName, TimeSpan?  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Time, value, useDbNullIfNull);
        }
        public static T AddDateTimeOffsetInParam<T>(this DbHelper<T> helper, string parameterName, DateTimeOffset?  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.DateTimeOffset, value, useDbNullIfNull);
        }

        public static T AddDecimalInParam<T>(this DbHelper<T> helper, string parameterName, decimal? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Decimal, value, useDbNullIfNull);
        }

        public static T AddCurrencyInParam<T>(this DbHelper<T> helper, string parameterName, decimal?  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Currency, value, useDbNullIfNull);
        }

        public static T AddDoubleInParam<T>(this DbHelper<T> helper, string parameterName, double? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Double, value, useDbNullIfNull);
        }

        public static T AddInt16InParam<T>(this DbHelper<T> helper, string parameterName, short? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Int16, value, useDbNullIfNull);
        }

        public static T AddByteInParam<T>(this DbHelper<T> helper, string parameterName, byte? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Byte, value, useDbNullIfNull);
        }

        public static T AddGuidInParam<T>(this DbHelper<T> helper, string parameterName, Guid?  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Guid, value == null ? null : (object)new System.Data.SqlTypes.SqlGuid(value.Value), useDbNullIfNull);
        }

        public static T AddBinaryInParam<T>(this DbHelper<T> helper, string parameterName, Byte[] value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, DbType.Binary, value, useDbNullIfNull);
        }

        //TODO: tobe defined
        #endregion parameters detailed
    }
}
