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
            {"datetime2", "DateTime"},
            {"date", "DateTime"},
            {"int", "int"},
            {"nchar", "string"},
            {"char", "string"},
            {"nvarchar", "string"},
            {"varchar", "string"},
            {"text", "string"},
            {"timestamp", "byte[]"},
            {"uniqueidentifier", "Guid"},
        };

        private static readonly IDictionary<string, string> SqlToCSharpNullableTypeMap = new Dictionary<string, string>()
        {
            {"bigint", "long?"},
            {"bit", "bool?"},
            {"float", "decimal?"},
            {"tinyint", "byte?"},
            {"smallint", "short?"},
            {"decimal", "decimal?"},
            {"numeric", "decimal?"},
            {"datetime", "DateTime?"},
            {"datetime2", "DateTime?"},
            {"date", "DateTime?"},
            {"int", "int?"},
            {"uniqueidentifier", "Guid?"},
        };

        private static readonly IDictionary<string, string> SqlToDbHelperMap = new Dictionary<string, string>()
        {
            {"tinyint", "TinyInt"},
            {"uniqueidentifier", "Guid"},
            {"nvarchar", "Nvarchar"},
            {"timestamp", "Binary"},
            {"smallint", "SmallInt"},
            {"datetime", "DateTime"},
            {"datetime2", "DateTime2"},
        };

        public static string ToCsharpType(string sqlTypeName, bool nullable)
        {
            var name = GetSqlObjectShortName(sqlTypeName).ToLower();
            return (nullable ? SqlToCSharpNullableTypeMap.GetOrDefault(name) : null) ?? SqlToCSharpTypeMap.GetOrDefault(name);
        }

        public static string GetSqlObjectShortName(string fullName)
        {
            if (fullName != null)
            {
                return fullName.Split('.').Last().Trim('[', ']');
            }
            return null;
        }
        public static void GetType(IList<Entity> entities, Method method)
        {
            var e = entities.FirstOrDefault(en => en.TableName == method.TableName);
            if (e != null)
            {
                method.Columns = e.Properties.Where(p => !p.Identity).ToList(); //&& method.Params.All(pa => pa.First != p.Name)
                method.IdentityColumns = e.Properties.Where(p => p.Identity).ToList();
                foreach (var param in method.Params)
                {
                    var p = e.Properties.FirstOrDefault(pr => pr.Name == param.First);
                    if (p != null)
                    {
                        param.Second = p.Type;
                        param.Third = p.Nullable;
                    }
                }
            }
        }

        public static IEnumerable<TemplatorKeyword> GetCustomizedTemplatorKeyword(DacpacExtractor extractor)
        {
            yield return new TemplatorKeyword("Plural")
            {
                ManipulateOutput = true,
                OnGetValue = (holder, parser, value) =>
                {
                    var map = extractor.PluralMappings.EmptyIfNull().FirstOrDefault(m => m.EntityName == (string)value);
                    if (map != null)
                    {
                        return map.PluralForm;
                    }
                    return value + "s";
                }
            };
            yield return new TemplatorKeyword("CSharpEnumCast")
            {
                OnGetValue = (holder, parser, value) =>
                {
                    var nullable = (bool?)parser.Context.Input.GetOrDefault("Nullable")
                        ?? (bool)parser.Context.Input.GetOrDefault("Third", true);
                    var columnName = (string)parser.Context.Input.GetOrDefault((string)holder["CSharpEnumCast"]);
                    var map = extractor.EnumMappings.EmptyIfNull().FirstOrDefault(e => e.ColumnFullName == columnName);
                    return map != null ? "({0})".FormatInvariantCulture(ToCsharpType((string)value, nullable)) : string.Empty;
                }
            };
            yield return new TemplatorKeyword("CSharpType")
            {
                OnGetValue = (holder, parser, value) =>
                {
                    var nullable = (bool?)parser.Context.Input.GetOrDefault("Nullable")
                        ?? (bool)parser.Context.Input.GetOrDefault("Third", false);
                    var columnName = (string)parser.Context.Input.GetOrDefault((string)holder["CSharpType"]);
                    var map = extractor.EnumMappings.EmptyIfNull().FirstOrDefault(e => e.ColumnFullName.Trim() == columnName);
                    return map == null ? ToCsharpType((string) value, nullable) : map.EnumTypeName + (nullable ? "?" : "");
                }
            };
            yield return new TemplatorKeyword("SqlShortName")
            {
                ManipulateOutput = true,
                OnGetValue = (holder, parser, value) => GetSqlObjectShortName((string)value)
            };
            yield return new TemplatorKeyword("SqlbareName")
            {
                ManipulateOutput = true,
                OnGetValue = (holder, parser, value) =>
                {
                    var s = (string) value;
                    if (s != null)
                    {
                        return s.Replace("[", "")
                            .Replace("]", "")
                            .Replace(".", "_");
                    }
                    return null;
                }
            };
            yield return new TemplatorKeyword("DbHelperSqlType")
            {
                ManipulateOutput = true,
                OnGetValue = (holder, parser, value) =>
                {
                    var shortname = GetSqlObjectShortName((string) value);
                    return SqlToDbHelperMap.GetOrDefault(shortname.ToLower(), shortname);
                }
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
