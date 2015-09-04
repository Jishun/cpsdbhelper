using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CpsDbHelper;
using CpsDbHelper.CodeGenerator;
using CpsDbHelper.Utils;
using DotNetUtils;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CpsDbHelperCodeGeneratorTest
{
    [TestClass]
    public class DacpacExtractorTest
    {
        public const string OriginalDbName = "TestDatabase";
        public string DbName
        {
            get
            {
                return  OriginalDbName + Thread.CurrentThread.ManagedThreadId + ".mdf";
            }
        }
        public string DbLogName
        {
            get
            {
                return OriginalDbName + Thread.CurrentThread.ManagedThreadId + "_log.ldf";
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
            var localConnectionString = @"Data Source=(localdb)\v11.0; AttachDBFilename='|DataDirectory|\{0}'; Integrated Security=True".FormatInvariantCulture(DbName);
            //var localConnectionString = @"Data Source=(localdb)\v11.0; AttachDBFilename='|DataDirectory|\TestDataBase.mdf'; Integrated Security=True";

            var connection = new SqlConnection(localConnectionString);
            try
            {
                connection.Open();

                var dacServices = new DacServices(@"Data Source=(localdb)\v11.0;Integrated Security=True");
                dacServices.Message += (sender, args) => Debug.WriteLineIf(Debugger.IsAttached, args.Message);
                dacServices.ProgressChanged +=
                    (sender, args) =>
                        Debug.WriteLineIf(Debugger.IsAttached,
                            String.Format("[{0}] {1} - {2}", args.OperationId, args.Status, args.Message));

#if DEBUG
                var dacPackageFile =
                    Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,
                        @"..\..\..\CpsDbHelper.TestDatabase\bin\Debug\CpsDbHelper.TestDatabase.dacpac"));
#else
                var dacPackageFile = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\CpsDbHelper.TestDatabase\bin\Release\CpsDbHelper.TestDatabase.dacpac"));
#endif

                var package = DacPackage.Load(dacPackageFile);
                CancellationToken? cancellationToken = new CancellationToken();

                dacServices.Deploy(package, connection.Database, true, null, cancellationToken);
                connection.Close();
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }


            var db = new DbHelperFactory(localConnectionString);
            var da = new CpsDbHelperDataAccess(localConnectionString);
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

        [TestCleanup]
        [TestInitialize]
        public void RemoveAllDb()
        {
            var sysdbs = new[] { "model", "master", "msdb", "tempdb" };

            var server = new Server(@"(localdb)\v11.0");
            var names = (from object db in server.Databases let name = db.ToString().TrimStart('[').TrimEnd(']') 
                         where !sysdbs.Contains(name) select name).ToList();
            foreach (var name in names)
            {
                var database = server.Databases[name];
                if (database != null)
                {
                    try
                    {
                        server.KillAllProcesses(name);
                    }
                    catch (Exception)
                    {
                        
                    }
                    try
                    {
                        database.DatabaseOptions.UserAccess = DatabaseUserAccess.Single;
                    }
                    catch (Exception)
                    {
                        
                    }
                    try
                    {
                        database.Alter(TerminationClause.RollbackTransactionsImmediately);
                    }
                    catch (Exception)
                    {
                        
                    }
                    try
                    {
                        server.DetachDatabase(name, true);
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            }
            if (File.Exists(DbName))
            {
                File.Delete(DbName);
            }
            if (File.Exists(DbLogName))
            {
                File.Delete(DbLogName);
            }
            File.Copy(OriginalDbName + ".mdf", DbName);
        }
    }
}
