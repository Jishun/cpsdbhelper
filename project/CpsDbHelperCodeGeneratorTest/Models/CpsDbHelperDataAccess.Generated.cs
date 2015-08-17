using System;
using System.Collections.Generic;
using System.Linq;
using CpsDbHelper;
using CpsDbHelper.Extensions;
using CpsDbHelper.Utils;

namespace CpsDbHelper.TestDataModel
{
    public partial class CpsDbHelperDataAccess : ICpsDbHelperDataAccess
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
        
        public IList<Table1> GetTable1sByName(string name)
        {
            const string query = "SELECT * FROM [dbo].[Table1] WHERE [Name] = @name";
            var ret = _db.BeginReader(query)
                         .AddVarcharInParam("Name", name)
                         .AutoMapResult<Table1>() 
                         .ExecuteSqlString()
                         .GetResult<IList<Table1>>(); 
            return ret;
        }
        
        
        public Table2 GetTable2ById(int id)
        {
            const string query = "SELECT * FROM [dbo].[Table2] WHERE [Id] = @id";
            var ret = _db.BeginReader(query)
                         .AddIntInParam("Id", id)
                         .AutoMapResult<Table2>() 
                         .ExecuteSqlString()
                         .GetResult<IList<Table2>>(); 
            return ret.FirstOrDefault();
        }
        
        public Table1 GetTable1ById(int id)
        {
            const string query = "SELECT * FROM [dbo].[Table1] WHERE [Id] = @id";
            var ret = _db.BeginReader(query)
                         .AddIntInParam("Id", id)
                         .AutoMapResult<Table1>() 
                         .ExecuteSqlString()
                         .GetResult<IList<Table1>>(); 
            return ret.FirstOrDefault();
        }
        
        public Table2 GetTable2ByNameAndDescript(string name, int? descript)
        {
            const string query = "SELECT * FROM [dbo].[Table2] WHERE [Name] = @name AND [Descript] = @descript";
            var ret = _db.BeginReader(query)
                         .AddNvarcharInParam("Name", name)
                         .AddIntInParam("Descript", descript)
                         .AutoMapResult<Table2>() 
                         .ExecuteSqlString()
                         .GetResult<IList<Table2>>(); 
            return ret.FirstOrDefault();
        }
        
        
        public void SaveTable1ById(Table1 table1)
        {
            const string query = "IF EXISTS(SELECT 1 FROM [dbo].[Table1] WHERE [Id] = @id) UPDATE [dbo].[Table1] SET [Id] = @id, [Name] = @name  WHERE [Id] = @id ELSE INSERT INTO [dbo].[Table1] ([Id], [Name]) VALUES(@id, @name)";
            var ret = _db.BeginNonQuery(query) 
                         .AddIntInParam("Id",  table1.Id) 
                         .AddVarcharInParam("Name",  table1.Name)
                         .ExecuteSqlString(); 
        }
        
        
        public int? SaveTable2ById(Table2 table2)
        {
            const string query = "IF EXISTS(SELECT 1 FROM [dbo].[Table2] WHERE [Id] = @id) UPDATE [dbo].[Table2] SET [Name] = @name, [Descript] = @descript  WHERE [Id] = @id ELSE BEGIN INSERT INTO [dbo].[Table2] ([Name], [Descript]) VALUES(@name, @descript) SELECT SCOPE_IDENTITY() END";
            var ret = _db.BeginScalar<int?>(query) 
                         .AddIntInParam("Id", table2.Id) 
                         .AddNvarcharInParam("Name",  table2.Name) 
                         .AddIntInParam("Descript",  table2.Descript)
                         .ExecuteSqlString()
                         .GetResult(); 
            return ret;
        }
        
        public int? SaveTable2ByNameAndDescript(Table2 table2)
        {
            const string query = "IF EXISTS(SELECT 1 FROM [dbo].[Table2] WHERE [Name] = @name AND [Descript] = @descript) UPDATE [dbo].[Table2] SET [Name] = @name, [Descript] = @descript  WHERE [Name] = @name AND [Descript] = @descript ELSE BEGIN INSERT INTO [dbo].[Table2] ([Name], [Descript]) VALUES(@name, @descript) SELECT SCOPE_IDENTITY() END";
            var ret = _db.BeginScalar<int?>(query) 
                         .AddIntInParam("Id", table2.Id) 
                         .AddNvarcharInParam("Name",  table2.Name) 
                         .AddIntInParam("Descript",  table2.Descript)
                         .ExecuteSqlString()
                         .GetResult(); 
            return ret;
        }
        
        
        public void DeleteTable2ById(int id)
        {
            const string query = "DELETE FROM [dbo].[Table2] WHERE [Id] = @id";
            _db.BeginNonQuery(query)
                         .AddIntInParam("Id", id)
                         .ExecuteSqlString(); 
        }
        
        public void DeleteTable1ById(int id)
        {
            const string query = "DELETE FROM [dbo].[Table1] WHERE [Id] = @id";
            _db.BeginNonQuery(query)
                         .AddIntInParam("Id", id)
                         .ExecuteSqlString(); 
        }
        
        public void DeleteTable2ByNameAndDescript(string name, int? descript)
        {
            const string query = "DELETE FROM [dbo].[Table2] WHERE [Name] = @name AND [Descript] = @descript";
            _db.BeginNonQuery(query)
                         .AddNvarcharInParam("Name", name)
                         .AddIntInParam("Descript", descript)
                         .ExecuteSqlString(); 
        }
        

        #region stored procedures
        
        public const string sp_dbo_Procedure1 = "[dbo].[Procedure1]"; //@param1 int; @param2 int
        
        #endregion stored procedures

        #region ScalarFunction names
        
        public const string sp_dbo_DatabaseScalarFunction1 = "[dbo].[DatabaseScalarFunction1]"; //@param1 int; @param2 int
        
        #endregion ScalarFunction names
        
        #region InlineTableValuedFunction names
        
        public const string sp_dbo_Function1 = "[dbo].[Function1]"; //@param1 int; @param2 char
        
        #endregion InlineTableValuedFunction names
        
        #region MultiStatementTableValuedFunction names
        
        public const string sp_dbo_TableFunction = "[dbo].[TableFunction]"; //@param1 int; @param2 char
        
        #endregion MultiStatementTableValuedFunction names
    }
}