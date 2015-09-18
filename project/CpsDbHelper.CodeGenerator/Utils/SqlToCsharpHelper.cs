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
            {"bigint","long"},
            {"binary","byte[]"},
            {"bit","bool"},
            {"char","string"},
            {"date","DateTime"},
            {"datetime","DateTime"},
            {"datetime2","DateTime"},
            {"decimal","decimal"},
            {"datetimeoffset","DateTimeOffset"},
            {"float","double"},
            {"image","byte[]"},
            {"int","int"},
            {"money","decimal"},
            {"nchar","string"},
            {"ntext","string"},
            {"nvarchar","string"},
            {"nvarcharmax","string"},
            {"real","decimal"},
            {"smalldatetime","DateTime"},
            {"smallint","short"},
            {"smallmoney","decimal"},
            {"text","string"},
            {"timestamp","byte[]"},
            {"rowversion","byte[]"},
            {"tinyint","byte"},
            {"uniqueidentifier","Guid"},
            {"varbinary","byte[]"},
            {"varbinarymax","byte[]"},
            {"varchar","string"},
            {"varcharmax","string"},
            {"xml","string"},
            {"time","TimeSpan"},
            {"hierarchyid","SqlHierarchyId "}
            
        };

        private static readonly IDictionary<string, string> SqlToCSharpNullableTypeMap = new Dictionary<string, string>()
        {
            {"bigint","long?"},
            {"bit","bool?"},
            {"datetime","DateTime?"},
            {"decimal","decimal?"},
            {"float","double?"},
            {"int","int?"},
            {"money","decimal?"},
            {"real","decimal?"},
            {"smalldatetime","DateTime?"},
            {"smallint","short?"},
            {"smallmoney","decimal?"},
            {"tinyint","byte?"},
            {"uniqueidentifier","Guid?"},
            {"date","DateTime?"},
            {"time","TimeSpan?"},
            {"datetimeoffset","DateTimeOffset?"},
            {"datetime2","DateTime?"},
        };

        private static readonly IDictionary<string, string> SqlToDbHelperMap = new Dictionary<string, string>()
        {
            {"tinyint", "TinyInt"},
            {"uniqueidentifier", "Guid"},
            {"nvarchar", "Nvarchar"},
            {"smallint", "SmallInt"},
            {"datetime", "DateTime"},
            {"datetime2", "DateTime2"},
            {"bigint", "BigInt"},
            {"money","Decimal"},
            {"nvarcharmax","Nvarchar"},
            {"real","Decimal"},
            {"smalldatetime","DateTime"},
            {"smallmoney","Decimal"},
            {"varbinary","Binary"},
            {"varbinarymax","Binary"},
            {"varcharmax","Varchar"},
            {"datetimeoffset","DateTimeOffset"},

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
        public static void GetType(IList<Entity> entities, Method method, bool isPrimaryKey, string foreignTableName)
        {
            if (foreignTableName != null)
            {
                var fe = entities.FirstOrDefault(en => en.TableName == foreignTableName);
                if (fe != null && fe.IncludeForeignKey)
                {
                    fe.Foreigns.Add(method);
                }
            }
            var e = entities.FirstOrDefault(en => en.TableName == method.TableName);
            if (e != null)
            {
                method.EntityName = e.Name;
                method.Columns = e.Properties.Where(p => !p.Identity).ToList(); //&& method.Params.All(pa => pa.Name != p.Name)
                method.IdentityColumns = e.Properties.Where(p => p.Identity).ToList();
                foreach (var param in method.Params)
                {
                    var p = e.Properties.FirstOrDefault(pr => pr.Name == param.Name);
                    if (p != null)
                    {
                        param.Type = p.Type;
                        param.Nullable = p.Nullable;
                    }
                }
                if (isPrimaryKey)
                {
                    foreach (var fes in e.Foreigns.Where(ef => ef.Params.Count == method.Params.Count))
                    {
                        var i = 0;
                        for (; i < method.Params.Count; i++)
                        {
                            if (fes.Params[i].ForeignName != method.Params[i].Name)
                            {
                                break;
                            }
                        }
                        if (i == method.Params.Count)
                        {
                            method.Foreigns.Add(fes);
                            break;
                        }
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
                    var nullable = (bool) parser.Context.Input.GetOrDefault("Nullable");
                    var columnName = (string)parser.Context.Input.GetOrDefault("Name");
                    var map = extractor.EnumMappings.EmptyIfNull().FirstOrDefault(e => e.ColumnFullName == columnName);
                    return map != null ? "({0})".FormatInvariantCulture(ToCsharpType((string)value, nullable)) : string.Empty;
                }
            };
            yield return new TemplatorKeyword("CSharpType")
            {
                OnGetValue = (holder, parser, value) =>
                {
                    var nullable = (bool)parser.Context.Input.GetOrDefault("Nullable");
                    var columnName = (string)parser.Context.Input.GetOrDefault("Name");
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
