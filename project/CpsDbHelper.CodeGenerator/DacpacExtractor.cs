using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using DotNetUtils;
using Newtonsoft.Json;
using Templator;

namespace CpsDbHelper.CodeGenerator
{
    public static class DacpacExtractor
    {
        public static void ParseDacpac(string dacpacFileName, string targetNamespace, string outPath = "./Models", string extensionPrefix = "Generated")
        {
            using (var stream = File.OpenRead(dacpacFileName))
            {
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var modelEntry = zip.GetEntry("model.xml");
                    using (var entryStream = modelEntry.Open())
                    {
                        var xml = XDocument.Load(entryStream);
                        if (xml.Root == null)
                        {
                            return;
                        }
                        foreach (var x in xml.Root.DescendantsAndSelf())
                        {
                            x.Name = x.Name.LocalName;
                            x.ReplaceAttributes((from xattrib in x.Attributes().Where(xa => !xa.IsNamespaceDeclaration) select new XAttribute(xattrib.Name.LocalName, xattrib.Value)));
                        }
                        var template = "CpsDbHelper.CodeGenerator.Templates.Class.txt".GetResourceTextFromExecutingAssembly();
                        var entities = new Dictionary<string, Entity>();
                        var elements = xml.Root.XPathSelectElements("/DataSchemaModel/Model/Element[@Type='SqlTable']");
                        foreach (var e in elements)
                        {
                            var entity = new Entity {TableName = e.GetAttributeString("Name")};
                            foreach (var pe in e.XPathSelectElements("Relationship[@Name='Columns']/Entry/Element[@Type='SqlSimpleColumn']"))
                            {
                                var nullableNode = pe.XPathSelectElement("Property[@Name='IsNullable']");
                                var p = new EntityProperty()
                                {
                                    Nullable = nullableNode == null || nullableNode.GetAttributeBool("Value"),
                                    Name = pe.GetAttributeString("Name"),
                                    Type = pe.XPathSelectElement("Relationship[@Name='TypeSpecifier']/Entry/Element[@Type='SqlTypeSpecifier']/Relationship[@Name='Type']/Entry/References").GetAttributeString("Name"),
                                };
                                entity.Properties.Add(p);
                            }
                            entities.Add(entity.TableName, entity);
                        }
                        elements = xml.Root.XPathSelectElements("/DataSchemaModel/Model/Element[@Type='SqlView']");
                        foreach (var e in elements)
                        {
                            
                        }
                        var config = TemplatorConfig.DefaultInstance;
                        var parser = new TemplatorParser(config);
                        foreach (var templatorKeyword in SqlToCsharpHelper.GetCustomizedTemplatorKeyword())
                        {
                            config.Keywords.Add(templatorKeyword.Name, templatorKeyword);
                        }
                        if (!Directory.Exists(outPath))
                        {
                            Directory.CreateDirectory(outPath);
                        }
                        foreach (var entity in entities.Values)
                        {
                            var fileName = extensionPrefix.IsNullOrWhiteSpace() ? entity.Name + ".cs" : "{0}.{1}.cs".FormatInvariantCulture(entity.Name, extensionPrefix);
                            var json = JsonConvert.SerializeObject(entity);
                            var input = json.ParseJsonDict();
                            input.Add("Namespace", targetNamespace);
                            var file = parser.ParseText(template, input);
                            using (var sw = new StreamWriter(Path.Combine(outPath, fileName)))
                            {
                                sw.Write(file);
                            }
                        }
                    }
                }
            }
        }
    }
}
