using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using CpsDbHelper.Utils;
using Microsoft.SqlServer.Server;
using NpgsqlTypes;
using Npgsql;

namespace CpsDbHelper.Extensions
{
    public static class PostgresNpgsqlParameterExtension
    {
        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, NpgsqlDbType dbType) where T : DbHelper<T>
        {
            return helper.AddParam(new NpgsqlParameter(parameterName, dbType));
        }

        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, NpgsqlDbType dbType, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddParam(new NpgsqlParameter(parameterName, dbType) { Value = useDbNullIfNull ? (value ?? DBNull.Value) : value });
        }

        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, NpgsqlDbType dbType, int size, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddParam(new NpgsqlParameter(parameterName, dbType, size) { Value = useDbNullIfNull ? (value ?? DBNull.Value) : value });
        }

        /// <summary>
        /// Add an outbound sql parameter
        /// </summary>
        public static T AddOutParam<T>(this DbHelper<T> helper, string parameterName, NpgsqlDbType dbType) where T : DbHelper<T>
        {
            return helper.AddParam(new NpgsqlParameter(parameterName, dbType) { Direction = ParameterDirection.Output });
        }

        /// <summary>
        /// Add an outbound sql parameter
        /// </summary>
        public static T AddOutParam<T>(this DbHelper<T> helper, string parameterName, NpgsqlDbType dbType, int size) where T : DbHelper<T>
        {
            return helper.AddParam(new NpgsqlParameter(parameterName, dbType, size) { Direction = ParameterDirection.Output });
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, NpgsqlDbType dbType) where T : DbHelper<T>
        {
            return helper.AddParam(new NpgsqlParameter(parameterName, dbType) { Direction = ParameterDirection.InputOutput });
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, NpgsqlDbType dbType, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddParam(new NpgsqlParameter(parameterName, dbType) { Value = useDbNullIfNull ? (value ?? DBNull.Value) : value, Direction = ParameterDirection.InputOutput });
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, NpgsqlDbType dbType, int size, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddParam(new NpgsqlParameter(parameterName, dbType, size) { Value = useDbNullIfNull ? (value ?? DBNull.Value) : value, Direction = ParameterDirection.InputOutput });
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
        /// Start mapping a structured parameter, will turn the call context into the mapper. and map the table-valued type's columns with the item's properties
        /// </summary>
        /// <typeparam name="T">the dbhelper</typeparam>
        /// <param name="helper"></param>
        /// <param name="parameterName">the structured param name</param>
        /// <returns></returns>
        public static StructParameterConstructor<T, object> BeginAddStructParam<T>(this DbHelper<T> helper, string parameterName) where T : DbHelper<T>
        {
            return new StructParameterConstructor<T, object>(parameterName, helper);
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
        public static T AutoMapStructParam<T>(this DbHelper<T> helper, string parameterName, IEnumerable<object> value, bool enumToInt = true, params string[] skips) where T : DbHelper<T>
        {
            return (T)(new StructParameterConstructor<T, object>(parameterName, helper).AutoMap(enumToInt, skips).FinishMap(value));
        }

        #region parameters detailed
        public static T AddNpgIntegerInParam<T>(this DbHelper<T> helper, string parameterName, int? value, bool useDbNullIfNull = false)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, NpgsqlDbType.Integer, value, useDbNullIfNull);
        }

        public static T AddNpgBigintInParam<T>(this DbHelper<T> helper, string parameterName, long? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, NpgsqlDbType.Bigint, value, useDbNullIfNull);
        }

        public static T AddNpgVarcharInParam<T>(this DbHelper<T> helper, string parameterName, string value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, NpgsqlDbType.Varchar, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }
        public static T AddNpgTextInParam<T>(this DbHelper<T> helper, string parameterName, string  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, NpgsqlDbType.Text, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }

        public static T AddNpgCharInParam<T>(this DbHelper<T> helper, string parameterName, string  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, NpgsqlDbType.Char, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }
        

        public static T AddNpgBitInParam<T>(this DbHelper<T> helper, string parameterName, bool? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, NpgsqlDbType.Bit, value, useDbNullIfNull);
        }
        
        public static T AddNpgMoneyInParam<T>(this DbHelper<T> helper, string parameterName, decimal?  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, NpgsqlDbType.Money, value, useDbNullIfNull);
        }

        public static T AddNpgSmallintInParam<T>(this DbHelper<T> helper, string parameterName, short? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, NpgsqlDbType.Smallint, value, useDbNullIfNull);
        }

        public static T AddUuidInParam<T>(this DbHelper<T> helper, string parameterName, Guid?  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, NpgsqlDbType.Uuid, value, useDbNullIfNull);
        }

        public static T AddNpgByteaInParam<T>(this DbHelper<T> helper, string parameterName, Byte[] value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, NpgsqlDbType.Bytea, value, useDbNullIfNull);
        }

        public static T AddTimestampInParam<T>(this DbHelper<T> helper, string parameterName, Byte[]  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, NpgsqlDbType.Timestamp, value, useDbNullIfNull);
        }

        //TODO: tobe defined
        #endregion parameters detailed
    }
}
