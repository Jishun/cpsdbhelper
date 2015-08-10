using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetUtils;
using Templator;

namespace CpsDbHelper.CodeGenerator
{
    public class SqlToCsharpHelper
    {
        private static readonly IDictionary<string, string> SqlToCSharpTypeMap = new Dictionary<string, string>()
        {
            {"bigint", "long"},
            {"bit", "bool"},
            {"float", "decimal"},
            {"tinyint", "byte"},
            {"smallint", "short"},
            {"decimal", "decimal"},
            {"numeric", "decimal"},
            {"datetime", "DateTime"},
            {"int", "int"},
            {"nchar", "string"},
            {"char", "string"},
            {"nvarchar", "string"},
            {"varchar", "string"},
            {"text", "string"},
            {"timestamp", "byte[]"},
            {"uniqueidentifier", "Guid"},
        };

        public static string ToCsharpType(string sqlTypeName)
        {
            return SqlToCSharpTypeMap.GetOrDefault(GetSqlObjectShortName(sqlTypeName).ToLower());
        }

        public static string GetSqlObjectShortName(string fullName)
        {
            if (fullName != null)
            {
                return fullName.Split('.').Last().Trim('[', ']');
            }
            return null;
        }

        public static IEnumerable<TemplatorKeyword> GetCustomizedTemplatorKeyword()
        {
            yield return new TemplatorKeyword("CSharpType")
            {
                ManipulateOutput = true,
                OnGetValue = (holder, parser, value) => ToCsharpType((string) value)
            };
            yield return new TemplatorKeyword("SqlShortName")
            {
                ManipulateOutput = true,
                OnGetValue = (holder, parser, value) => GetSqlObjectShortName((string)value)
            };
        }
    }
}
