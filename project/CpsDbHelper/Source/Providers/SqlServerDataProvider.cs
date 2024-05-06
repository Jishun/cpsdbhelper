using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

        public IDbDataParameter CreateParameter(string dataType = null)
        {
            var p = new SqlParameter();
            if(Enum.TryParse<SqlDbType>(dataType, out var dbType)){
                p.SqlDbType = dbType;
            }
            return p;
        }
    }
}
