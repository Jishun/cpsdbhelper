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
    public class Entity
    {
        public const string Template = "Class.txt";
        public string TableName;
        public IList<EntityProperty> Properties  = new List<EntityProperty>();

        public string Name
        {
            get { return SqlToCsharpHelper.GetSqlObjectShortName(TableName); }
        }

        public static IEnumerable<Entity> GetEntities(XElement xml)
        {
            const string xpath = "/DataSchemaModel/Model/Element[@Type='SqlTable']";
            var elements = xml.XPathSelectElements(xpath);
            foreach (var e in elements)
            {
                var entity = new Entity {TableName = e.GetAttributeString("Name")};
                foreach (var pe in e.XPathSelectElements("Relationship[@Name='Columns']/Entry/Element[@Type='SqlSimpleColumn']"))
                {
                    var nullableNode = pe.XPathSelectElement("Property[@Name='IsNullable']");
                    var identityNode = pe.XPathSelectElement("Property[@Name='IsIdentity']");
                    var p = new EntityProperty()
                    {
                        Nullable = nullableNode == null || nullableNode.GetAttributeBool("Value"),
                        Identity = identityNode != null && identityNode.GetAttributeBool("Value"),
                        Name = pe.GetAttributeString("Name"),
                        Type = pe.XPathSelectElement("Relationship[@Name='TypeSpecifier']/Entry/Element[@Type='SqlTypeSpecifier']/Relationship[@Name='Type']/Entry/References").GetAttributeString("Name"),
                    };
                    entity.Properties.Add(p);
                }
                yield return entity;
            }
        }

        public static IEnumerable<Entity> GetViewEntities(XElement xml)
        {
            throw new NotImplementedException();
        } 
    }

}
