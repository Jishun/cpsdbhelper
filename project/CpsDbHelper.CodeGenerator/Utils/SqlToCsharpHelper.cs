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
            yield return new TemplatorKeyword("FirstLower")
            {
                ManipulateOutput = true,
                OnGetValue = (holder, parser, value) => ((string)value).Substring(0,1).ToLower() + ((string)value).Substring(1,((string)(value)).Length-1)
            };
            yield return new TemplatorKeyword("FirstUpper")
            {
                ManipulateOutput = true,
                OnGetValue = (holder, parser, value) => ((string)value).Substring(0, 1).ToUpper() + ((string)value).Substring(1, ((string)(value)).Length - 1)
            };
            yield return new TemplatorKeyword("Join")
            {
                ManipulateOutput = true,
                HandleNullOrEmpty = true,
                OnGetValue = (holder, parser, value) =>
                {
                    var key = parser.StackLevel + holder.Name + "InputIndex";
                    var i = (int?) parser.Context[key];
                    if (i.HasValue && i > 0)
                    {
                        parser.AppendResult(holder["Join"]);
                    }
                    return value;
                }
            };
        }
    }
}
