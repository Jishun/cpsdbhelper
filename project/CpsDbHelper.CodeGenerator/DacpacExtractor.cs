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
                        var entities = Entity.GetEntities(xml.Root).ToList();
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
                        var template = GetTemplate(Entity.Template);
                        foreach (var entity in entities)
                        {
                            var fileName = extensionPrefix.IsNullOrWhiteSpace() ? entity.Name + ".cs" : "{0}.{1}.cs".FormatInvariantCulture(entity.Name, extensionPrefix);
                            var json = JsonConvert.SerializeObject(entity);
                            var input = json.ParseJsonDict();
                            input.Add("Namespace", targetNamespace);
                            var file = parser.ParseText(template, input);
                            parser.StartOver();
                            using (var sw = new StreamWriter(Path.Combine(outPath, fileName)))
                            {
                                sw.Write(file);
                            }
                        }
                        var methods = Method.GetMethods(xml.Root, entities).ToList();
                        template = GetTemplate(Method.Template);
                        var name = extensionPrefix.IsNullOrWhiteSpace() ? "DataAccess.cs" : "DataAccess.{0}.cs".FormatInvariantCulture(extensionPrefix);
                        using (var sw = new StreamWriter(Path.Combine(outPath, name)))
                        {
                            var json = JsonConvert.SerializeObject(new 
                            {
                                NonQueryMethods = methods.Where(m => m.IdentityColumns.IsNullOrEmpty()),
                                ScalarMethods = methods.Where(m => !m.IdentityColumns.IsNullOrEmpty()) ,
                                UniqueMethods = methods.Where(m => m.Unique),
                                MultipleMethods = methods.Where(m => !m.Unique) 
                            });
                            var input = json.ParseJsonDict();
                            input.Add("Namespace", targetNamespace);
                            var file = parser.ParseText(template, input);
                            parser.StartOver();
                            sw.Write(file);
                        }
                    }
                }
            }
        }

        private static string GetTemplate(string name)
        {
            return ("CpsDbHelper.CodeGenerator.Templates." + name).GetResourceTextFromExecutingAssembly();
        }
    }
}
