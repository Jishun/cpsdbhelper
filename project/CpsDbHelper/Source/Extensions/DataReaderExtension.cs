using System;
using System.Data;

namespace CpsDbHelper.Extensions
{
    public static class DataReaderExtension
    {
        /// <summary>
        /// Check if the returned reader contains specific column
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName">the column name</param>
        /// <returns></returns>
        public static bool ContainsColumn(this IDataReader reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get value from specific reader column by column name
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="reader"></param>
        /// <param name="columnName">column name</param>
        /// <returns></returns>
        public static T Get<T>(this IDataReader reader, string columnName)
        {
            if (reader.ContainsColumn(columnName))
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (!reader.IsDBNull(ordinal))
                {
                    return (T)reader.GetValue(ordinal);
                }
            }
            return default(T);
        }

        /// <summary>
        /// Get value from specific reader column by column ordinal
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="reader"></param>
        /// <param name="ordinal">column index</param>
        /// <returns></returns>
        public static T Get<T>(this IDataReader reader, int ordinal)
        {
            return (T)reader.GetValue(ordinal);
        }
    }
}