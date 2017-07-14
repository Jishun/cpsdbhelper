using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpsDbHelper
{
    public class SqlServerDataProvider : IAdoNetProviderFactory
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public IDbDataParameter CreateParameter()
        {
            return new SqlParameter();
        }
    }
}
