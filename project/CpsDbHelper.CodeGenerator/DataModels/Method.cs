using System;
using System.Collections.Generic;
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
        public IList<Triple<string, string, bool>> Params;
        public IList<EntityProperty> Columns;
        public IList<EntityProperty> IdentityColumns;
        public bool Unique;

        public static IEnumerable<Method> GetMethods(XElement xml, DacpacExtractor extractor, IList<Entity> entities)
        {
            const string xpath = "/DataSchemaModel/Model/Element[@Type='SqlPrimaryKeyConstraint']";
            var elements = xml.XPathSelectElements(xpath);
            foreach (var element in elements)
            {
                var ret = new Method
                {
                    TableName =
                        element.XPathSelectElement("Relationship[@Name='DefiningTable']/Entry/References")
                            .GetAttributeString("Name"),
                    Unique = true,
                    Params =
                        element.XPathSelectElements(
                            "Relationship[@Name='ColumnSpecifications']/Entry/Element[@Type='SqlIndexedColumnSpecification']/Relationship[@Name='Column']/Entry/References")
                            .Select(e => new Triple<string, string, bool>(e.GetAttributeString("Name"), "", false))
                            .ToList()
                };
                SqlToCsharpHelper.GetType(entities, ret);
                if (!extractor.ObjectsToIgnore.EmptyIfNull().Contains(ret.TableName))
                {
                    yield return ret;
                }
            }
            elements = xml.XPathSelectElements("/DataSchemaModel/Model/Element[@Type='SqlIndex']");
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
                            .Select(e => new Triple<string, string, bool>(e.GetAttributeString("Name"), "", false))
                            .ToList()
                };
                SqlToCsharpHelper.GetType(entities, ret);
                if (!extractor.ObjectsToIgnore.EmptyIfNull().Contains(ret.TableName))
                {
                    yield return ret;
                }
            }
        }

    }
}
