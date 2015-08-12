using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CpsDbHelper.CodeGenerator;
using CpsDbHelper.TestDataModel;
using CpsDbHelper.Utils;
using DotNetUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CpsDbHelperCodeGeneratorTest
{
    [TestClass]
    public class DacpacExtractorTest
    {
        [TestMethod]
        public void TestExtractDacpack()
        {
            var p = new DacpacExtractor();
            using (var sw = new StreamWriter("dacpac.xml"))
            {
                p.PluralMappings = new[]
                {
                    new DacpacExtractor.PluralMapping(){ EntityName = "Table1", PluralForm = "Table1es" }
                    ,new DacpacExtractor.PluralMapping(){ EntityName = "Table2", PluralForm = "Table2es" }
                };
                p.ModelNamespace = "hehe";
                var e = p.ToXElement();
                sw.Write(e.ToString());
            }
        }

        [TestMethod]
        public void TestGeneratedCode()
        {
            const string localConnectionString = "Data Source=.;Integrated Security=True;Pooling=False;Initial Catalog=CpsDbHelper.TestDatabase";
            var da = new CpsDbHelperDataAccess(localConnectionString);
            var db = new DbHelperFactory(localConnectionString);
            db.BeginNonQuery("Truncate table dbo.table1")
                .ExecuteSqlString();
            db.BeginNonQuery("Truncate table dbo.table2").ExecuteSqlString();
            var t1 = new Table1()
            {
                Id = 10,
                Name = "t1r1"
            };
            da.BeginTransaction();
            da.SaveTable1ById(t1);
            var t1load = da.GetTable1ById(t1.Id);
            Assert.AreEqual(t1load.Name, t1.Name);
            var t2 = new Table2()
            {
                Descript = 1,
                Name = "t2r1"
            };
            var id = da.SaveTable2ById(t2);
            var t2load = da.GetTable2ById(id.Value);
            Assert.AreEqual(t2.Name, t2load.Name);
            da.EndTransaction(false);
            t1load = da.GetTable1ById(t1.Id);
            Assert.IsNull(t1load);
            t1.Name = "t1r1m";
            da.SaveTable1ById(t1);
            t1load = da.GetTable1ById(t1.Id);
            Assert.AreEqual(t1load.Name, t1.Name);
            da.BeginTransaction();
            t1.Name = "t1r1m2";
            da.SaveTable1ById(t1);
            t1load = da.GetTable1ById(t1.Id);
            da.EndTransaction();
            Assert.AreEqual(t1load.Name, t1.Name);
            t1load = da.GetTable1ById(11);
            Assert.IsNull(t1load);
            id = da.SaveTable2ById(t2);
            t2load = da.GetTable2ByNameAndDescript(t2.Name, t2.Descript);
            Assert.AreEqual(t2load.Id, id.Value);
        }
    }
}
