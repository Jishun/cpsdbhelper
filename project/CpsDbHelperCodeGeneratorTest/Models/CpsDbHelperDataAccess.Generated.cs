/*Generated by CpsDbHelper CodeGenerator, require CpsDbHelper version >= 1.0.0.4
Source code at https://github.com/djsxp/cpsdbhelper
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CpsDbHelper;
using CpsDbHelper.Extensions;
using CpsDbHelper.Utils;
using System.Runtime.Serialization;


namespace CpsDbHelper
{
    internal partial class CpsDbHelperDataAccess : ICpsDbHelperDataAccess
    {
        private DbHelperFactory _db;

        public CpsDbHelperDataAccess(string connectionString)
        {
            _db = new DbHelperFactory(connectionString);
        }

        public void BeginTransaction()
        {
            _db.BeginTransaction();
        }

        public void EndTransaction(bool commit = true)
        {
            _db.EndConnection(commit);
        }
        
        public IList<Table2> GetTable2sByForIdAndForName(int? forId, long? forName)
        {
            const string query = "SELECT * FROM [dbo].[Table2] WHERE ForId] = @forId AND ForName] = @forName";
            var reader = _db.BeginReader(query)
                         .AddIntInParam("ForId", forId, true)
                         .AddBigIntInParam("ForName", forName, true)
                         .AutoMapResult<Table2>() 
                         .ExecuteSqlString();
            var ret = reader.GetResult<IList<Table2>>(); 
            return ret;
        }
        
        
        public Table2 GetTable2ById(int id)
        {
            const string query = "SELECT * FROM [dbo].[Table2] WHERE Id] = @id";
            var reader = _db.BeginReader(query)
                         .AddIntInParam("Id", id, true)
                         .AutoMapResult<Table2>()
                         .ExecuteSqlString();
            var ret = reader.GetResultCollection<Table2>().FirstOrDefault();
            return ret;
        }
        
        public TableAnother GetTableAnotherByIdAndName1(int id, long name1, bool includeTable2s = false)
        {
            const string query = "SELECT * FROM [dbo].[Table1] WHERE Id] = @id AND Name1] = @name1 IF(@includeTable2s = 1) SELECT * FROM [dbo].[Table2] WHERE ForId = @id  AND ForName = @name1 ";
            var reader = _db.BeginReader(query)
                         .AddIntInParam("Id", id, true)
                         .AddBigIntInParam("Name1", name1, true)
                         .AddBitInParam("includeTable2s", includeTable2s)
                         .AutoMapResult<TableAnother>()
                         .AutoMapResult<Table2>("Table2s")
                         .ExecuteSqlString();
            var ret = reader.GetResultCollection<TableAnother>().FirstOrDefault();
            if(includeTable2s && ret != null){
                ret.Table2s = reader.GetResultCollection<Table2>("Table2s");
            }
            return ret;
        }
        
        public Table2 GetTable2ByNameAndDescript(string name, int? descript)
        {
            const string query = "SELECT * FROM [dbo].[Table2] WHERE Name] = @name AND Descript] = @descript";
            var reader = _db.BeginReader(query)
                         .AddNvarcharInParam("Name", name, true)
                         .AddIntInParam("Descript", descript, true)
                         .AutoMapResult<Table2>()
                         .ExecuteSqlString();
            var ret = reader.GetResultCollection<Table2>().FirstOrDefault();
            return ret;
        }
        
        
        public void SaveTableAnotherByIdAndName1(TableAnother tableAnother)
        {
            const string query = "IF EXISTS(SELECT 1 FROM [dbo].[Table1] WHERE Id] = @id AND Name1] = @name1) UPDATE [dbo].[Table1] SET Id] = @id, Name] = @name, Name1] = @name1, Name2] = @name2, Name3] = @name3, Name4] = @name4, Name5] = @name5, Name6] = @name6, Name7] = @name7, Name8] = @name8, Name9] = @name9, Name0] = @name0, Name11] = @name11, Name12] = @name12, Name13] = @name13, Name14] = @name14, Name15] = @name15, Name16] = @name16, Name17] = @name17, Name18] = @name18, Name20] = @name20, Name21] = @name21, Name22] = @name22, Name23] = @name23, Name24] = @name24, Name25] = @name25, Name26] = @name26, Name27] = @name27, Name28] = @name28  WHERE Id] = @id AND Name1] = @name1 ELSE INSERT INTO [dbo].[Table1] (Id], Name], Name1], Name2], Name3], Name4], Name5], Name6], Name7], Name8], Name9], Name0], Name11], Name12], Name13], Name14], Name15], Name16], Name17], Name18], Name20], Name21], Name22], Name23], Name24], Name25], Name26], Name27], Name28]) VALUES(@id, @name, @name1, @name2, @name3, @name4, @name5, @name6, @name7, @name8, @name9, @name0, @name11, @name12, @name13, @name14, @name15, @name16, @name17, @name18, @name20, @name21, @name22, @name23, @name24, @name25, @name26, @name27, @name28)";
            var ret = _db.BeginNonQuery(query) 
                         .AddIntInParam("Id",  tableAnother.Id, true) 
                         .AddCharInParam("Name",  tableAnother.Name, true) 
                         .AddBigIntInParam("Name1",  tableAnother.Name1, true) 
                         .AddBinaryInParam("Name2",  tableAnother.Name2, true) 
                         .AddBitInParam("Name3",  tableAnother.Name3, true) 
                         .AddCharInParam("Name4",  tableAnother.Name4, true) 
                         .AddDateTimeInParam("Name5",  tableAnother.Name5, true) 
                         .AddDecimalInParam("Name6",  tableAnother.Name6, true) 
                         .AddFloatInParam("Name7",  tableAnother.Name7, true) 
                         .AddImageInParam("Name8",  tableAnother.Name8, true) 
                         .AddIntInParam("Name9",  tableAnother.Name9, true) 
                         .AddDecimalInParam("Name0",  tableAnother.Name0, true) 
                         .AddNcharInParam("Name11",  tableAnother.Name11, true) 
                         .AddNtextInParam("Name12",  tableAnother.Name12, true) 
                         .AddNvarcharInParam("Name13",  tableAnother.Name13, true) 
                         .AddDecimalInParam("Name14",  tableAnother.Name14, true) 
                         .AddDateTimeInParam("Name15",  tableAnother.Name15, true) 
                         .AddSmallIntInParam("Name16",  tableAnother.Name16, true) 
                         .AddDecimalInParam("Name17",  tableAnother.Name17, true) 
                         .AddTextInParam("Name18",  tableAnother.Name18, true) 
                         .AddTinyIntInParam("Name20",  tableAnother.Name20, true) 
                         .AddGuidInParam("Name21",  tableAnother.Name21, true) 
                         .AddBinaryInParam("Name22",  tableAnother.Name22, true) 
                         .AddVarcharInParam("Name23",  tableAnother.Name23, true) 
                         .AddXmlInParam("Name24",  tableAnother.Name24, true) 
                         .AddDateInParam("Name25",  tableAnother.Name25, true) 
                         .AddTimeInParam("Name26",  tableAnother.Name26, true) 
                         .AddDateTimeOffsetInParam("Name27",  tableAnother.Name27, true) 
                         .AddDateTime2InParam("Name28",  tableAnother.Name28, true)
                         .ExecuteSqlString(); 
        }
        
        
        public int? SaveTable2ByForIdAndForName(Table2 table2)
        {
            const string query = "IF EXISTS(SELECT 1 FROM [dbo].[Table2] WHERE ForId] = @forId AND ForName] = @forName) UPDATE [dbo].[Table2] SET Name] = @name, ForId] = @forId, ForName] = @forName, Descript] = @descript  WHERE ForId] = @forId AND ForName] = @forName ELSE BEGIN INSERT INTO [dbo].[Table2] (Name], ForId], ForName], Descript]) VALUES(@name, @forId, @forName, @descript) SELECT SCOPE_IDENTITY() END";
            var scalar = _db.BeginScalar<int?>(query) 
                         .AddIntInParam("Id", table2.Id, true) 
                         .AddNvarcharInParam("Name",  table2.Name, true) 
                         .AddIntInParam("ForId",  table2.ForId, true) 
                         .AddBigIntInParam("ForName",  table2.ForName, true) 
                         .AddIntInParam("Descript",  table2.Descript, true)
                         .ExecuteSqlString();
            var ret = scalar.GetResult(); 
            return ret;
        }
        
        public int? SaveTable2ById(Table2 table2)
        {
            const string query = "IF EXISTS(SELECT 1 FROM [dbo].[Table2] WHERE Id] = @id) UPDATE [dbo].[Table2] SET Name] = @name, ForId] = @forId, ForName] = @forName, Descript] = @descript  WHERE Id] = @id ELSE BEGIN INSERT INTO [dbo].[Table2] (Name], ForId], ForName], Descript]) VALUES(@name, @forId, @forName, @descript) SELECT SCOPE_IDENTITY() END";
            var scalar = _db.BeginScalar<int?>(query) 
                         .AddIntInParam("Id", table2.Id, true) 
                         .AddNvarcharInParam("Name",  table2.Name, true) 
                         .AddIntInParam("ForId",  table2.ForId, true) 
                         .AddBigIntInParam("ForName",  table2.ForName, true) 
                         .AddIntInParam("Descript",  table2.Descript, true)
                         .ExecuteSqlString();
            var ret = scalar.GetResult(); 
            return ret;
        }
        
        public int? SaveTable2ByNameAndDescript(Table2 table2)
        {
            const string query = "IF EXISTS(SELECT 1 FROM [dbo].[Table2] WHERE Name] = @name AND Descript] = @descript) UPDATE [dbo].[Table2] SET Name] = @name, ForId] = @forId, ForName] = @forName, Descript] = @descript  WHERE Name] = @name AND Descript] = @descript ELSE BEGIN INSERT INTO [dbo].[Table2] (Name], ForId], ForName], Descript]) VALUES(@name, @forId, @forName, @descript) SELECT SCOPE_IDENTITY() END";
            var scalar = _db.BeginScalar<int?>(query) 
                         .AddIntInParam("Id", table2.Id, true) 
                         .AddNvarcharInParam("Name",  table2.Name, true) 
                         .AddIntInParam("ForId",  table2.ForId, true) 
                         .AddBigIntInParam("ForName",  table2.ForName, true) 
                         .AddIntInParam("Descript",  table2.Descript, true)
                         .ExecuteSqlString();
            var ret = scalar.GetResult(); 
            return ret;
        }
        
        
        public void DeleteTable2ById(int id)
        {
            const string query = "DELETE FROM [dbo].[Table2] WHERE Id] = @id";
            _db.BeginNonQuery(query)
                         .AddIntInParam("Id", id, true)
                         .ExecuteSqlString(); 
        }
        
        public void DeleteTableAnotherByIdAndName1(int id, long name1)
        {
            const string query = "DELETE FROM [dbo].[Table1] WHERE Id] = @id AND Name1] = @name1";
            _db.BeginNonQuery(query)
                         .AddIntInParam("Id", id, true)
                         .AddBigIntInParam("Name1", name1, true)
                         .ExecuteSqlString(); 
        }
        
        public void DeleteTable2ByNameAndDescript(string name, int? descript)
        {
            const string query = "DELETE FROM [dbo].[Table2] WHERE Name] = @name AND Descript] = @descript";
            _db.BeginNonQuery(query)
                         .AddNvarcharInParam("Name", name, true)
                         .AddIntInParam("Descript", descript, true)
                         .ExecuteSqlString(); 
        }
        

        #region stored procedures
        
        public const string Sp_dbo_Procedure1 = "[dbo].[Procedure1]"; //@param1 int; @param2 xml
        
        #endregion stored procedures

        #region ScalarFunction names
        
        public const string Fn_dbo_DatabaseScalarFunction1 = "[dbo].[DatabaseScalarFunction1]"; //@param1 int; @param2 int
        
        #endregion ScalarFunction names
        
        #region InlineTableValuedFunction names
        
        public const string Fn_dbo_Function1 = "[dbo].[Function1]"; //@param1 int; @param2 char
        
        #endregion InlineTableValuedFunction names
        
        #region MultiStatementTableValuedFunction names
        
        public const string Fn_dbo_TableFunction = "[dbo].[TableFunction]"; //@param1 int; @param2 char
        
        #endregion MultiStatementTableValuedFunction names
    }
}