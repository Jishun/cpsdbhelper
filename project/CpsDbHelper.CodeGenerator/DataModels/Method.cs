using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using DotNetUtils;

namespace CpsDbHelper.CodeGenerator
{
    public class Method
    {
        public const string Template = "DataAccess.txt";
        public const string InterfaceTemplate = "IDataAccess.txt";
        public string TableName;
        public string KeyName;
        public bool ReadAsAsync;
        public bool WriteAsAsync;
        public bool DeleteAsAsync;
        public IList<EntityProperty> Params;
        public IList<EntityProperty> Columns;
        public IList<EntityProperty> IdentityColumns;
        public IList<Method> Foreigns = new List<Method>();
        public bool Unique;

        public string GetAsync
        {
            get { return ReadAsAsync ? "Async" : String.Empty; }
        }

        public string SaveAsync
        {
            get { return WriteAsAsync ? "Async" : String.Empty; }
        }

        public string DeleteAsync
        {
            get { return DeleteAsAsync ? "Async" : String.Empty; }
        }

        public string EntityName { get; set; }

        public static IEnumerable<Method> GetMethods(XElement xml, DacpacExtractor extractor, IList<Entity> entities)
        {
            if (extractor.IncludeForeignKey)
            {
                var elements = xml.XPathSelectElements("/DataSchemaModel/Model/Element[@Type='SqlForeignKeyConstraint']");
                foreach (var element in elements)
                {
                    var foreignTable = element.XPathSelectElement("Relationship[@Name='ForeignTable']/Entry/References")
                        .GetAttributeString("Name");
                    var ps = element.XPathSelectElements("Relationship[@Name='Columns']/Entry/References")
                            .Select(e => new EntityProperty() { Name = e.GetAttributeString("Name") }).ToList();
                    for (int index = 0; index < ps.Count; index++)
                    {
                        var property = ps[index];
                        property.ForeignName =
                            element.XPathSelectElement("Relationship[@Name='ForeignColumns']/Entry[" + (index + 1) + "]/References")
                                .GetAttributeString("Name");
                    }
                    var ret = new Method
                    {
                        TableName =
                            element.XPathSelectElement("Relationship[@Name='DefiningTable']/Entry/References")
                                .GetAttributeString("Name"),
                        Params = ps,
                        KeyName = element.GetAttributeString("Name"),
                        ReadAsAsync = extractor.GetAsync,
                        DeleteAsAsync = extractor.DeleteAsync,
                        WriteAsAsync = extractor.SaveAsync
                    };
                    if (!extractor.ObjectsToIgnore.EmptyIfNull().Contains(ret.TableName))
                    {
                        SqlToCsharpHelper.GetType(entities, ret, false, foreignTable);
                        MapAsyncSettings(ret, extractor);
                        yield return ret;
                    }
                }
            }
            if (extractor.IncludePrimaryKey)
            {
                var elements = xml.XPathSelectElements("/DataSchemaModel/Model/Element[@Type='SqlPrimaryKeyConstraint']");
                foreach (var element in elements)
                {
                    var ret = new Method
                    {
                        KeyName = element.GetAttributeString("Name"),
                        TableName =
                            element.XPathSelectElement("Relationship[@Name='DefiningTable']/Entry/References")
                                .GetAttributeString("Name"),
                        Unique = true,
                        Params =
                            element.XPathSelectElements(
                                "Relationship[@Name='ColumnSpecifications']/Entry/Element[@Type='SqlIndexedColumnSpecification']/Relationship[@Name='Column']/Entry/References")
                                .Select(e => new EntityProperty() { Name = e.GetAttributeString("Name") })
                                .ToList(),
                        ReadAsAsync = extractor.GetAsync,
                        DeleteAsAsync = extractor.DeleteAsync,
                        WriteAsAsync = extractor.SaveAsync
                    };
                    if (!extractor.ObjectsToIgnore.EmptyIfNull().Contains(ret.TableName))
                    {
                        SqlToCsharpHelper.GetType(entities, ret, true, null);
                        MapAsyncSettings(ret, extractor);
                        yield return ret;
                    }
                }
            }
            if (extractor.IncludeNonUniqueIndex || extractor.IncludeUniqueIndex)
            {
                var elements = xml.XPathSelectElements("/DataSchemaModel/Model/Element[@Type='SqlIndex']");
                foreach (var element in elements)
                {
                    var uniqueNode = element.XPathSelectElement("Property[@Name='IsUnique']");
                    var ret = new Method
                    {
                        TableName =
                            element.XPathSelectElement("Relationship[@Name='IndexedObject']/Entry/References")
                                .GetAttributeString("Name"),
                        Unique = uniqueNode != null && uniqueNode.GetAttributeBool("Value"),
                        Params =
                            element.XPathSelectElements(
                                "Relationship[@Name='ColumnSpecifications']/Entry/Element[@Type='SqlIndexedColumnSpecification']/Relationship[@Name='Column']/Entry/References")
                                .Select(e => new EntityProperty() { Name = e.GetAttributeString("Name") })
                                .ToList(),
                        KeyName = element.GetAttributeString("Name"),
                        ReadAsAsync = extractor.GetAsync,
                        DeleteAsAsync = extractor.DeleteAsync,
                        WriteAsAsync = extractor.SaveAsync
                    };
                    if (!extractor.ObjectsToIgnore.EmptyIfNull().Contains(ret.TableName))
                    {
                        if ((extractor.IncludeNonUniqueIndex && !ret.Unique)
                            || (extractor.IncludeUniqueIndex && ret.Unique))
                        {
                            SqlToCsharpHelper.GetType(entities, ret, false, null);
                            MapAsyncSettings(ret, extractor);
                            yield return ret;
                        }
                    }
                }
            }
        }

        static void MapAsyncSettings(Method method, DacpacExtractor extractor)
        {
            var map = extractor.AsyncMappings.EmptyIfNull().FirstOrDefault(a => a.IndexName == method.KeyName);
            if (map != null)
            {
                method.ReadAsAsync = map.GetAsync;
                method.WriteAsAsync = map.SaveAsync;
                method.DeleteAsAsync = map.DeleteAsync;
            }
        }

    }
}
