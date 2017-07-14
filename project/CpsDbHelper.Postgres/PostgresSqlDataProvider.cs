using Npgsql;
using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace CpsDbHelper.Postgres
{
    public class PostgresSqlDataProvider : IAdoNetProviderFactory
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }

        public IDbDataParameter CreateParameter()
        {
            return new NpgsqlParameter();
        }
    }
}
/*
using (System.Data.IDbConnection conn = new NpgsqlConnection(""))
            {
                conn.Open();

                // Insert some data
                using (System.Data.IDbCommand cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO data (some_field) VALUES (@p)";
                    cmd.Parameters.Add(new SqlParameter());
                    cmd.ExecuteNonQuery();
                }

                // Retrieve all rows
                using (var cmd = new NpgsqlCommand("SELECT some_field FROM data", conn))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        Console.WriteLine(reader.GetString(0));
            }
 */
