using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CpsDbHelper;
using CpsDbHelper.CodeGenerator;
using CpsDbHelper.Utils;
using DotNetUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SSDTDeployer;

namespace CpsDbHelperCodeGeneratorTest
{
    [TestClass]
    public class DacpacExtractorTest
    {
        // static string _testDbName = null;
        static SsdtLocalDbDeployer _server;
        const string ConnectionString = @"Data Source=(localdb)\v11.0; AttachDBFilename='|DataDirectory|\{0}'; Integrated Security=True";


        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            _server = new SsdtLocalDbDeployer(DbName, true);

#if DEBUG
            var dacPackageFile =Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,
                        @"..\..\..\CpsDbHelper.TestDatabase\bin\Debug\CpsDbHelper.TestDatabase.dacpac"));
#else
            var dacPackageFile = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\CpsDbHelper.TestDatabase\bin\Release\CpsDbHelper.TestDatabase.dacpac"));
#endif
            _server.DeployDacPac(dacPackageFile);
        }
        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
            _server.DetachDb();
        }

        public const string OriginalDbName = "TestDatabase";
        public static string DbName
        {
            get
            {
                return  OriginalDbName  + "12.mdf";
            }
        }

        [TestMethod]
        public void TestExtractDacpack()
        {
            var p = new DacpacExtractor();
            using (var sw = new StreamWriter("dacpac.xml"))
            {
                p.PluralMappings = new[]
                {
                    new DacpacExtractor.PluralMapping(){ EntityName = "TableAnother", PluralForm = "TableAnotheres" }
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
            var db = new DbHelperFactory(ConnectionString.FormatInvariantCulture(DbName));
            var da = new CpsDbHelperDataAccess(ConnectionString.FormatInvariantCulture(DbName));
            Task.WaitAll(db.BeginNonQuery("delete from dbo.table2").ExecuteSqlStringAsync());
            db.BeginNonQuery("delete from dbo.Table1")
                .ExecuteSqlString();
            var t1 = new TableAnother()
            {
                Id = 10,
                Name1 = 10,
                Name = "t1r1",
                Name13 = "t1r1"
            };
            da.BeginTransaction();
            da.SaveTableAnotherByIdAndName1(t1);
            var t1load = da.GetTableAnotherByIdAndName1(t1.Id, t1.Name1);
            Assert.AreEqual(t1load.Name13, t1.Name13);
            var t2 = new Table2()
            {
                Descript = 1,
                Name = "t2r1"
            };
            var id = da.SaveTable2ById(t2);
            var t2load = da.GetTable2ById(id.Value);
            Assert.AreEqual(t2.Name, t2load.Name);
            da.EndTransaction(false);
            t1load = da.GetTableAnotherByIdAndName1(t1.Id,  t1.Name1);
            Assert.IsNull(t1load);
            t1.Name13 = "t1r1m";
            da.SaveTableAnotherByIdAndName1(t1);
            t1load = da.GetTableAnotherByIdAndName1(t1.Id, t1.Name1);
            Assert.AreEqual(t1load.Name13, t1.Name13);
            da.BeginTransaction();
            t1.Name13 = "t1r1m2";
            da.SaveTableAnotherByIdAndName1(t1);
            t1load = da.GetTableAnotherByIdAndName1(t1.Id, t1.Name1);
            da.EndTransaction();
            Assert.AreEqual(t1load.Name13, t1.Name13);
            t1load = da.GetTableAnotherByIdAndName1(11, 11);
            Assert.IsNull(t1load);
            id = da.SaveTable2ById(t2);
            t2load = da.GetTable2ByNameAndDescript(t2.Name, t2.Descript);
            Assert.AreEqual(t2load.Id, id.Value);
            var task = db.BeginReader("select * from table2")
                .AutoMapResult<Table2>()
                .ExecuteSqlStringAsync();
            task.Wait();
            var ret = task.Result
                .GetResultCollection<Table2>();

            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(ret.First().Id, id.Value);

            t2 = new Table2()
            {
                ForId = t1.Id,
                ForName = t1.Name1,
                Name = "hehe"
            };
            id = da.SaveTable2ByForIdAndForName(t2);
            var retT2 = da.GetTable2sByForIdAndForName(t2.ForId, t2.ForName);
            Assert.AreEqual(1,retT2.Count);
            Assert.AreEqual(t2.Name, retT2.First().Name);
            t1load = da.GetTableAnotherByIdAndName1(t2.ForId.Value, t2.ForName.Value, true);
            Assert.AreEqual(1, t1load.Table2s.Count);
            Assert.AreEqual(t2.Name, t1load.Table2s.First().Name);
            t1load = da.GetTableAnotherByIdAndName1(t2.ForId.Value, t2.ForName.Value);
            Assert.AreEqual(null, t1load.Table2s);
        }

    }
}
