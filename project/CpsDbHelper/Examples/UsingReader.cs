using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CpsDbHelper.Examples.Models;
using CpsDbHelper.Extensions;
using CpsDbHelper.Utils;

namespace CpsDbHelper.Examples
{
    internal class UsingReader
    {
        private readonly DbHelperFactory _db = new DbHelperFactory("dummy connection string");

        /// <summary>
        /// executing a stored procedure which returns one set of query and the columns matches the entity property defination
        /// </summary>
        /// <param name="id">an optional id parameter</param>
        /// <returns></returns>
        public IEnumerable<Person> GetPersion(int? id)
        {
            var reader = _db.BeginReader("sp_getPeople")
                         .AddIntInParam("Id", id)
                         .AddOutParam("totalCount", SqlDbType.Int)
                         .AutoMapResult<Person>() //the result key is used to identify result set, the default value can be used once
                         //the same as this way: 
                         //.BeginMapResult<Person>().AutoMap().FinishMap()
                         .Execute();
            //retrieve the result. the reader always use a List<T> to store the returned table. 
            var ret = reader.GetResult<IList<Person>>(); //the result key is default as the one when mapping.
            //retrive the output parameter with it's name.
            var count = reader.GetParamResult<int>("totalCount");
            return ret;
        }

        /// <summary>
        /// executing a stored procedure which returns two sets of results, people and address, of which the columns matches the property definations
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public KeyValuePair<Person, Address> GetPersionAndAddres(int id)
        {
            var reader = _db.BeginReader("sp_getPersonAndAddress")
                         .AddIntInParam("personId", id)
                         .AutoMapResult<Person>("Persion") //we can name the result set this way. or we can still use the default parameter
                         .AutoMapResult<Address>("Address") //map the second reader result, if the previous line uses the default value. this line must specify a different result key
                         .Execute();
            //retrieve the results with result key
            var ret = reader.GetResult<IList<Person>>("Persion");
            var addressRet = reader.GetResult<IList<Address>>("Address");
            return new KeyValuePair<Person, Address>(ret.FirstOrDefault(), addressRet.FirstOrDefault());
        }

        /// <summary>
        /// executing a stored procedure which returns a company with it's address in one set
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Company GetCompanyWithAddress(int id)
        {
            var presavedColumnIndexToImprovePerformance = 0;
            var reader = _db.BeginReader("sp_getCompanyWithAddress")
                         .AddIntInParam("companyId", id)
                         .PreProcessResult(rd => presavedColumnIndexToImprovePerformance = rd.GetOrdinal("Country"))
                         .BeginMapResult<Company>("Company") //it is also ok to leave the result key default 
                         .AutoMap() //can partially use the automap ability, the mapper will map the columns with properties with same names and leave the others to you
                         .MapField<string>("AdditionalColumnName", (item, columnValue) => item.PropertyWithNoMatchingColumn = columnValue)
                         .MapField((item, rd) =>
                             { //customizing a map logic to assign value to non-automapable field.
                                 item.Address = new Address()
                                     {
                                         City = rd.Get<string>("City"), //operating at current row. use the reader extension to read value of Column 'City'
                                         Country = rd.Get<string>(presavedColumnIndexToImprovePerformance) //or we can pre-get the ordinal and use it to get better performance
                                     };
                             })
                         .FinishMap()
                         .Execute();
            //retrieve the results with result key
            var ret = reader.GetResult<IList<Company>>("Company");
            return ret.FirstOrDefault();
        }

        /// <summary>
        /// executing a stored procedure which returns a set of addresses
        /// this time we demostrate controlling the reader without any mapping functionality, and a transaction
        /// </summary>
        /// <returns></returns>
        public Address GetAddresses()
        {
            return _db.BeginReader("sp_getAddresses")
               .DefineResult((rd, helper) =>
                   {
                       var ret = new List<Address>();
                       var ordinalCity = rd.GetOrdinal("City");
                       while (rd.Read())
                       {
                           ret.Add(new Address()
                               {
                                   City = rd.Get<string>(ordinalCity),
                                   Country = rd.Get<string>("Country")
                               });
                       }
                       //this return value is exactly result which the reader stores. we will get it later at the end of this method
                       return ret.FirstOrDefault();
                   })
               .BeginTransaction()
               .Execute(false)
               .CommitTransaction()
               .End()
               .GetResult<Address>();
        }

        /// <summary>
        /// executing a stored procedure which returns a set of addresses
        /// using define list result
        /// </summary>
        /// <returns></returns>
        public Address GetAddresses2()
        {
            return _db.BeginReader("sp_getAddresses")
               .DefineListResult((rd, helper) => new Address()
                   {
                       //the reader will loop the result set and apply this action and put the return value into a list 
                       //the result will be List<Address>
                       City = rd.Get<string>("City"),
                       Country = rd.Get<string>("Country")
                   })
               .BeginTransaction()
               .Execute(false)
               .CommitTransaction()
               .End()
               .GetResult<IList<Address>>()
               .FirstOrDefault();
        }
    }
}
