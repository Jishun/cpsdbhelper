using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpsDbHelper.CodeGenerator
{
    public class Entity
    {
        public string TableName;
        public IList<EntityProperty> Properties  = new List<EntityProperty>();

        public string Name
        {
            get { return SqlToCsharpHelper.GetSqlObjectShortName(TableName); }
        }
    }

}
