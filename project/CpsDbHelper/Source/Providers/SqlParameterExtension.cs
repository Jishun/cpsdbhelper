using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Xml.Linq;
using CpsDbHelper.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;

namespace CpsDbHelper.Extensions
{
    public static class ParameterExtension
    {
        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, null, dbType.ToString());
            p.Direction = ParameterDirection.Input;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, null, dbType.ToString());
            p.Direction = ParameterDirection.Input;
            p.Value = useDbNullIfNull ? (value ?? DBNull.Value) : value;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an inbound sql parameter
        /// </summary>
        public static T AddInParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType, int size, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, null, dbType.ToString());
            p.Direction = ParameterDirection.Input;
            p.Size = size;
            p.Value = useDbNullIfNull ? (value ?? DBNull.Value) : value;
            return helper.AddParam(p); 
        }

        /// <summary>
        /// Add an outbound sql parameter
        /// </summary>
        public static T AddOutParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, null, dbType.ToString());
            p.Direction = ParameterDirection.Output;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an outbound sql parameter
        /// </summary>
        public static T AddOutParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType, int size) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, null, dbType.ToString());
            p.Direction = ParameterDirection.Output;
            p.Size = size;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, null, dbType.ToString());
            p.Direction = ParameterDirection.InputOutput;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, null, dbType.ToString());
            p.Direction = ParameterDirection.InputOutput;
            p.Value = useDbNullIfNull ? (value ?? DBNull.Value) : value;
            return helper.AddParam(p);
        }

        /// <summary>
        /// Add an in-outbound sql parameter
        /// </summary>
        public static T AddInOutParam<T>(this DbHelper<T> helper, string parameterName, SqlDbType dbType, int size, object value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            var p = helper.CreateParameter(parameterName, null, dbType.ToString());
            p.Direction = ParameterDirection.InputOutput;
            p.Size = size;
            p.Value = useDbNullIfNull ? (value ?? DBNull.Value) : value;
            return helper.AddParam(p);
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
        public static T AddIntInParam<T>(this DbHelper<T> helper, string parameterName, int? value, bool useDbNullIfNull = false)  where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Int, value, useDbNullIfNull);
        }

        public static T AddBigIntInParam<T>(this DbHelper<T> helper, string parameterName, long? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.BigInt, value, useDbNullIfNull);
        }

        public static T AddVarcharInParam<T>(this DbHelper<T> helper, string parameterName, string value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.VarChar, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }
        public static T AddNcharInParam<T>(this DbHelper<T> helper, string parameterName, string  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.NChar, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }
        public static T AddTextInParam<T>(this DbHelper<T> helper, string parameterName, string  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Text, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }

        public static T AddCharInParam<T>(this DbHelper<T> helper, string parameterName, string  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Char, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }

        public static T AddNvarcharInParam<T>(this DbHelper<T> helper, string parameterName, string value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.NVarChar, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }
        public static T AddNtextInParam<T>(this DbHelper<T> helper, string parameterName, string  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.NText, value == null ? 0 : value.Length, (object)value, useDbNullIfNull);
        }

        public static T AddBitInParam<T>(this DbHelper<T> helper, string parameterName, bool? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Bit, value, useDbNullIfNull);
        }
        
        public static T AddMoneyInParam<T>(this DbHelper<T> helper, string parameterName, decimal?  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Money, value, useDbNullIfNull);
        }

        public static T AddFloatInParam<T>(this DbHelper<T> helper, string parameterName, double? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Float, value, useDbNullIfNull);
        }

        public static T AddSmallIntInParam<T>(this DbHelper<T> helper, string parameterName, short? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.SmallInt, value, useDbNullIfNull);
        }

        public static T AddTinyIntInParam<T>(this DbHelper<T> helper, string parameterName, byte? value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.TinyInt, value, useDbNullIfNull);
        }

        public static T AddUniqueIdentifierInParam<T>(this DbHelper<T> helper, string parameterName, Guid?  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.UniqueIdentifier, value == null ? null : (object)new System.Data.SqlTypes.SqlGuid(value.Value), useDbNullIfNull);
        }

        public static T AddVarBinaryInParam<T>(this DbHelper<T> helper, string parameterName, Byte[] value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.VarBinary, value, useDbNullIfNull);
        }

        public static T AddImageInParam<T>(this DbHelper<T> helper, string parameterName, Byte[]  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Image, value, useDbNullIfNull);
        }

        public static T AddTimestampInParam<T>(this DbHelper<T> helper, string parameterName, Byte[]  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Timestamp, value, useDbNullIfNull);
        }

        public static T AddStructParam<T>(this DbHelper<T> helper, string parameterName, IList<SqlDataRecord> value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Structured, value, useDbNullIfNull);
        }

        public static T AddStructParam<T>(this DbHelper<T> helper, string parameterName, DataTable  value, bool useDbNullIfNull = false) where T : DbHelper<T>
        {
            return helper.AddInParam(parameterName, SqlDbType.Structured, value, useDbNullIfNull);
        }

        //TODO: tobe defined
        #endregion parameters detailed
    }
}
