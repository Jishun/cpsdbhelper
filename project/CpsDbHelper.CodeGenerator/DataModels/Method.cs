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
        public string TableName;
        public IList<Pair<string, string>> Params;
        public IList<EntityProperty> Columns;
        public IList<EntityProperty> IdentityColumns;
        public bool Unique;

        public static IEnumerable<Method> GetMethods(XElement xml, IList<Entity> entities)
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
                            .Select(e => new Pair<string, string>(e.GetAttributeString("Name"), ""))
                            .ToList()
                };
                GetType(entities, ret);
                yield return ret;
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
                            .Select(e => new Pair<string, string>(e.GetAttributeString("Name"), ""))
                            .ToList()
                };
                GetType(entities, ret);
                yield return ret;
            }
        }

        private static void GetType(IList<Entity> entities, Method method)
        {
            var e = entities.FirstOrDefault(en => en.TableName == method.TableName);
            if (e != null)
            {
                method.Columns = e.Properties.Where(p => !p.Identity).ToList();
                method.IdentityColumns = e.Properties.Where(p => p.Identity).ToList();
                foreach (var param in method.Params)
                {
                    var p = e.Properties.FirstOrDefault(pr => pr.Name == param.First);
                    if (p != null)
                    {
                        param.Second = p.Type;
                    }
                }
            }
        }
    }
}
