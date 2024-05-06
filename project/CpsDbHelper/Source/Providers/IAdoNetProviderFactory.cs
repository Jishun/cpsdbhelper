using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpsDbHelper
{
    public interface IAdoNetProviderFactory
    {
        IDbConnection CreateConnection(string connectionString);

        IDbDataParameter CreateParameter(string dataType = null);
    }
}
