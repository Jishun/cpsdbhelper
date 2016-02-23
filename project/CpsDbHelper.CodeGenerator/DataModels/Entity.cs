using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using DotNetUtils;

namespace CpsDbHelper.CodeGenerator
{
    public class Entity
    {
        private string _name = null;

        [XmlIgnore]
        public const string Template = "Class.txt";

        [XmlAttribute]
        public string TableName { get; set; }

        [XmlAttribute]
        public bool IncludeForeignKey { get; set; }

        [XmlAttribute]
        public string ClassAccess { get; set; }

        [XmlIgnore]
        public IList<EntityProperty> Properties  = new List<EntityProperty>();

        [XmlElement]
        public string[] Annotations { get; set; }

        [XmlIgnore]
        public IList<Method> Foreigns = new List<Method>();
        
        [XmlAttribute]
        public string Name
        {
            get { return _name ?? SqlToCsharpHelper.GetSqlObjectShortName(TableName); }
            set { _name = value; }
        }

        public string AttrBegin
        {
            get { return Annotations.IsNullOrEmpty() ? String.Empty : "["; }
        }
        public string AttrEnd
        {
            get { return Annotations.IsNullOrEmpty() ? String.Empty : "]"; }
        }
        [XmlIgnore]
        public object[] Attrs
        {
            get
            {
                return Annotations.EmptyIfNull().Select(a => new Dictionary<string, object>() { { "Attr", a } }).Cast<object>().ToArray();
            }
        }

        public static IEnumerable<Entity> GetEntities(XElement xml, DacpacExtractor extractor)
        {
            const string xpath = "/DataSchemaModel/Model/Element[@Type='SqlTable']";
            var elements = xml.XPathSelectElements(xpath);
            foreach (var e in elements)
            {
                var entity = new Entity {
                    TableName = e.GetAttributeString("Name"), 
                    IncludeForeignKey = extractor.IncludeForeignKey,
                    ClassAccess = extractor.ClassAccess ?? "public"
                };
                foreach (var pe in e.XPathSelectElements("Relationship[@Name='Columns']/Entry/Element[@Type='SqlSimpleColumn']"))
                {
                    var nullableNode = pe.XPathSelectElement("Property[@Name='IsNullable']");
                    var identityNode = pe.XPathSelectElement("Property[@Name='IsIdentity']");
                    var p = new EntityProperty()
                    {
                        Nullable = nullableNode == null || nullableNode.GetAttributeBool("Value"),
                        Identity = identityNode != null && identityNode.GetAttributeBool("Value"),
                        Name = pe.GetAttributeString("Name"),
                        Type = pe.XPathSelectElement("Relationship[@Name='TypeSpecifier']/Entry/Element[@Type='SqlTypeSpecifier' or @Type='SqlXmlTypeSpecifier']/Relationship[@Name='Type']/Entry/References").GetAttributeString("Name"),
                    };
                    var over = extractor.ColumnOverrides.EmptyIfNull().FirstOrDefault(o => o.Name == p.Name);
                    if (over != null)
                    {
                        p.Type = over.Type ?? p.Type;
                        p.Nullable = over.Nullable ?? p.Nullable;
                        p.Attributes = over.Attributes;
                        over.Name = null;
                    }

                    if (p.Type.ToLower() != "[timestamp]" && p.Type.ToLower() != "[rowversion]" && !extractor.IsIgnored(p.Name))
                    {
                        entity.Properties.Add(p);
                    }
                }
                foreach (var c in extractor.ColumnOverrides.EmptyIfNull())
                {
                    if (c.Name != null && c.Type != null && c.Name.StartsWith(entity.TableName))
                    {
                        entity.Properties.Add(c);
                    }
                }
                var an = extractor.EntityOverrides.EmptyIfNull().FirstOrDefault(ae => ae.TableName == entity.TableName);
                if (an != null)
                {
                    entity.Annotations = an.Annotations;
                    entity.Name = an.Name;
                    entity.IncludeForeignKey = an.IncludeForeignKey;
                    entity.ClassAccess = an.ClassAccess.IsNullOrWhiteSpace() ? entity.ClassAccess : an.ClassAccess;
                }
                if (!extractor.ObjectsToIgnore.EmptyIfNull().Contains(entity.TableName))
                {
                    yield return entity;
                }
            }
        }

        public static IEnumerable<Entity> GetViewEntities(XElement xml)
        {
            throw new NotImplementedException();
        } 
    }

}
