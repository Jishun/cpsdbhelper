using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CpsDbHelper.Utils;

namespace CpsDbHelper.Postgres.test
{
    [TestClass]
    public class UnitTest1
    {
        public class EventLog
        {
            public long logid { get; set; }
            public int eventid { get; set; }
        }
        [TestMethod]
        public void TestMethod1()
        {
            var db = new DbHelperFactory("Server=localhost; Database=todo; User Id=postgres; Password=postgres; Timeout=30");

            var ret = db.UsePostgresql()
                .BeginReader("select * from dbo.EventLog")
                .AutoMapResult<EventLog>()
                .ExecuteSqlString()
                .GetResultCollection<EventLog>();


        }
    }
}
