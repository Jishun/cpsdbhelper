using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CpsDbHelper.Examples.Models;
using CpsDbHelper.Extensions;
using CpsDbHelper.Utils;

namespace CpsDbHelper.Examples
{
    internal class UsingNonReader
    {
        private readonly DbHelperFactory _db = new DbHelperFactory("dummy connection string");

        /// <summary>
        /// Save a set of addresses with a user-defined table-valued structure type parameter
        /// and finally selects a count out
        /// </summary>
        /// <param name="addresses"></param>
        public int SaveAddresses(IEnumerable<Address> addresses)
        {
            return _db.BeginScalar<int>("sp_saveAddresses")
               .BeginAddStructParam<ScalarHelper<int>, Address>("Addresses")
               .MapField("City", item => item.City)
               .MapField("Country", item => item.Country)
               .FinishMap(addresses)
                //can also use the auto mapper if the columns are name match and sequence match
                .AutoMapStructParam("Addresses", addresses)
               .Execute()
               .GetResult();
        }

        /// <summary>
        /// Save a person and return the id value
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public int SavePerson(Person person)
        {
            return _db.BeginNonQuery("sp_SavePerson")
                      .AddVarcharInParam("name", person.Name)
                      .AddIntInParam("gender", (int) person.Gender)
                      .Execute()
                      .GetReturnValue<int>();
        }

        public XElement UsingXmlReader()
        {
            return _db.BeginXmlReader("sp_returnXml")
                      .Execute()
                      .GetResult();
        }
    }
}
