using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using DotNetUtils;
using Newtonsoft.Json;
using Templator;

namespace CpsDbHelper.CodeGenerator
{
    public class DacpacExtractor
    {
        public class PluralMapping
        {
            public string EntityName { get; set; }
            public string PluralForm { get; set; }
        }
        public class EnumMapping
        {
            public string ColumnFullName { get; set; }
            public string EnumTypeName { get; set; }
        }
        public class AsyncMapping
        {
            public string IndexName { get; set; }
            public bool GetAsync { get; set; }
            public bool SaveAsync { get; set; }
            public bool DeleteAsync { get; set; }
        }

        public string DbProjectPath { get; set; }
        public bool Enabled { get; set; }
        public bool GetAsync { get; set; }
        public bool SaveAsync { get; set; }
        public bool DeleteAsync { get; set; }
        public bool ErrorIfDacpacNotFound { get; set; }
        [XmlElement("EnabledInConfigurations")]
        public string[] EnabledInConfigurations { get; set; }
        [XmlElement("ColumnOverrides")]
        public EntityProperty[] ColumnOverrides { get; set; }

        public string ModelNamespace { get; set; }
        public string DalNamespace { get; set; }
        public string ModelOutPath { get; set; }
        public string DalOutPath { get; set; }
        public string DataAccessClassName { get; set; }
        public string FileNameExtensionPrefix { get; set; }

        [XmlElement("ObjectsToIgnore")]
        public string[] ObjectsToIgnore { get; set; }
        [XmlElement("PluralMappings")]
        public PluralMapping[] PluralMappings { get; set; }

        [XmlElement("EnumMappings")]
        public EnumMapping[] EnumMappings { get; set; }
        [XmlElement("AsyncMappings")]
        public AsyncMapping[] AsyncMappings { get; set; }

        public void ParseDacpac(string dacpacFileName)
        {
            if (!Enabled)
            {
                return;
            }
            if (!File.Exists(dacpacFileName) && !ErrorIfDacpacNotFound)
            {
                return;
            }
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
                        var entities = Entity.GetEntities(xml.Root, this).ToList();
                        var config = TemplatorConfig.DefaultInstance;
                        var parser = new TemplatorParser(config);
                        foreach (var templatorKeyword in SqlToCsharpHelper.GetCustomizedTemplatorKeyword(this))
                        {
                            config.Keywords.AddOrOverwrite(templatorKeyword.Name, templatorKeyword);
                        }
                        if (!Directory.Exists(ModelOutPath))
                        {
                            Directory.CreateDirectory(ModelOutPath);
                        }
                        if (!Directory.Exists(DalOutPath))
                        {
                            Directory.CreateDirectory(DalOutPath);
                        }
                        var template = GetTemplate(Entity.Template);
                        foreach (var entity in entities)
                        {
                            var fileName = FileNameExtensionPrefix.IsNullOrWhiteSpace() ? entity.Name + ".cs" : "{0}.{1}.cs".FormatInvariantCulture(entity.Name, FileNameExtensionPrefix);
                            var json = JsonConvert.SerializeObject(entity);
                            var input = json.ParseJsonDict();
                            input.Add("Namespace", ModelNamespace);
                            var file = parser.ParseText(template, input);
                            parser.StartOver();
                            using (var sw = new StreamWriter(Path.Combine(ModelOutPath, fileName)))
                            {
                                sw.Write(file);
                            }
                        }
                        var methods = Method.GetMethods(xml.Root, this, entities).ToList();
                        template = GetTemplate(Method.Template);
                        var iTemplate = GetTemplate(Method.InterfaceTemplate);
                        var name = FileNameExtensionPrefix.IsNullOrWhiteSpace() ? DataAccessClassName + ".cs" : "{0}.{1}.cs".FormatInvariantCulture(DataAccessClassName, FileNameExtensionPrefix);
                        using (var sw = new StreamWriter(Path.Combine(DalOutPath, name)))
                        {
                            using (var isw = new StreamWriter(Path.Combine(DalOutPath, "I" + name)))
                            {
                                var json = JsonConvert.SerializeObject(new
                                {
                                    NonQueryMethods = methods.Where(m => m.IdentityColumns.IsNullOrEmpty() && m.Unique),
                                    ScalarMethods = methods.Where(m => !m.IdentityColumns.IsNullOrEmpty()),
                                    UniqueMethods = methods.Where(m => m.Unique),
                                    MultipleMethods = methods.Where(m => !m.Unique),
                                    InlineTableFunctions = StoredProcedure.GetInlineTableValuedFunctions(xml.Root, this),
                                    TableFunctions = StoredProcedure.GetlMultiStatementTableValuedFunctions(xml.Root, this),
                                    ScalarFunctions = StoredProcedure.GetScalarFunctions(xml.Root, this),
                                    Sps = StoredProcedure.GetStoredProcedures(xml.Root, this),
                                });
                                var input = json.ParseJsonDict();
                                input.Add("DataAccessClassName", DataAccessClassName);
                                input.Add("DalNamespace", DalNamespace);
                                input.Add("HelperVersion", "1.0.0.4");
                                input.Add("Namespace", ModelNamespace);
                                var file = parser.ParseText(template, input);
                                parser.StartOver();
                                sw.Write(file);
                                file = parser.ParseText(iTemplate, input);
                                parser.StartOver();
                                isw.Write(file);
                            }
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
