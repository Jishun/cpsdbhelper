/*Generated by CpsDbHelper CodeGenerator, require CpsDbHelper version >= 1.0.0.4
Source code at https://github.com/Jishun/cpsdbhelper
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CpsDbHelper;
using System.Runtime.Serialization;


namespace CpsDbHelper
{
    public partial interface ICpsDbHelperDataAccess
    {
        void BeginTransaction();
        void EndTransaction(bool commit = true);
        
        IList<Table2> GetTable2sByForIdAndForName(int? forId, long? forName);
        
        
        Table2 GetTable2ById(int id);
        
        TableAnother GetTableAnotherByIdAndName1(int id, long name1, bool includeTable2s = false);
        
        Table2 GetTable2ByNameAndDescript(string name, int? descript);
        
        
        void SaveTableAnotherByIdAndName1(TableAnother tableAnother);
        
        
        int? SaveTable2ByForIdAndForName(Table2 table2);
        
        int? SaveTable2ById(Table2 table2);
        
        int? SaveTable2ByNameAndDescript(Table2 table2);
        
        
        void DeleteTable2ById(int id);
        
        void DeleteTableAnotherByIdAndName1(int id, long name1);
        
        void DeleteTable2ByNameAndDescript(string name, int? descript);
        
    }
}