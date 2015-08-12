using System;
using System.Collections.Generic;
using System.Linq;
using CpsDbHelper;
using CpsDbHelper.Extensions;
using CpsDbHelper.Utils;

namespace CpsDbHelper.TestDataModel
{
    public partial interface ICpsDbHelperDataAccess
    {
        void BeginTransaction();
        void EndTransaction(bool commit = true);
        
        IList<Table1> GetTable1sByName(string name);
        
        
        Table2 GetTable2ById(int id);
        
        Table1 GetTable1ById(int id);
        
        Table2 GetTable2ByNameAndDescript(string name, int descript);
        
        
        void SaveTable1ById(Table1 table1);
        
        void SaveTable1ByName(Table1 table1);
        
        
        int? SaveTable2ById(Table2 table2);
        
        int? SaveTable2ByNameAndDescript(Table2 table2);
        
    }
}