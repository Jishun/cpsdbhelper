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
        private const string ConfigErrorMessage =
            "if you have recently upgraded code generator, please find examples from 'https://github.com/djsxp/cpsdbhelper/blob/master/project/CpsDbHelper.CodeGerator.BuildTask/CodeGeneratorSettings.xml.pp' to update the format, appologize for the inconvenience";

        public const string ConfigFileName = "CodeGeneratorSettings.xml";

        public class PluralMapping
        {
            [XmlAttribute]
            public string EntityName { get; set; }
            [XmlAttribute]
            public string PluralForm { get; set; }
        }
        public class EnumMapping
        {
            [XmlAttribute]
            public string ColumnFullName { get; set; }
            [XmlAttribute]
            public string EnumTypeName { get; set; }
        }
        public class AsyncMapping
        {
            [XmlAttribute]
            public string IndexName { get; set; }
            [XmlAttribute]
            public bool GetAsync { get; set; }
            [XmlAttribute]
            public bool SaveAsync { get; set; }
            [XmlAttribute]
            public bool DeleteAsync { get; set; }
        }

        [XmlAttribute]
        public bool Enabled { get; set; }
        [XmlAttribute]
        public bool IncludePrimaryKey { get; set; }
        [XmlAttribute]
        public bool IncludeUniqueIndex { get; set; }
        [XmlAttribute]
        public bool IncludeNonUniqueIndex { get; set; }
        [XmlAttribute]
        public bool IncludeForeignKey { get; set; }
        [XmlAttribute]
        public bool GetAsync { get; set; }
        [XmlAttribute]
        public bool SaveAsync { get; set; }
        [XmlAttribute]
        public bool DeleteAsync { get; set; }
        [XmlAttribute]
        public bool ErrorIfDacpacNotFound { get; set; }

        public string ClassAccess { get; set; }
        public string DbProjectPath { get; set; }
        public string ModelNamespace { get; set; }
        public string DalNamespace { get; set; }
        public string ModelOutPath { get; set; }
        public string DalOutPath { get; set; }
        public string DataAccessClassName { get; set; }
        public string FileNameExtensionPrefix { get; set; }

        [XmlElement]
        public string[] ObjectsToIgnore { get; set; }
        [XmlElement]
        public string[] Usings { get; set; }
        [XmlElement]
        public string[] EnabledInConfigurations { get; set; }
        [XmlElement]
        public EntityProperty[] ColumnOverrides { get; set; }
        [XmlElement]
        public Entity[] EntityOverrides { get; set; }

        [XmlElement]
        public PluralMapping[] PluralMappings { get; set; }
        [XmlElement]
        public EnumMapping[] EnumMappings { get; set; }
        [XmlElement]
        public AsyncMapping[] AsyncMappings { get; set; }

        public DacpacExtractor()
        {
            ErrorIfDacpacNotFound = 
            Enabled =
            IncludeForeignKey =
            IncludeNonUniqueIndex = 
            IncludePrimaryKey = 
            IncludeUniqueIndex = true;
        }

        public bool IsIgnored(string key)
        {
            return !key.IsNullOrEmpty() && ObjectsToIgnore.EmptyIfNull().Contains(key);
        }

        public static DacpacExtractor LoadFromXml(XElement element, out string errorMessage)
        {
            try
            {
                errorMessage = null;
                return element.FromXElement<DacpacExtractor>();
            }
            catch (Exception e)
            {
                errorMessage =
                    "Unable to load config from config file, " + ConfigErrorMessage;
            }
            return null;
        }

        public string ParseDacpac(string dacpacFileName)
        {
            try
            {
                if (!Enabled)
                {
                    return null;
                }
                if (!File.Exists(dacpacFileName) && !ErrorIfDacpacNotFound)
                {
                    return null;
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
                                return null;
                            }
                            foreach (var x in xml.Root.DescendantsAndSelf())
                            {
                                x.Name = x.Name.LocalName;
                                x.ReplaceAttributes((from xattrib in x.Attributes().Where(xa => !xa.IsNamespaceDeclaration) select new XAttribute(xattrib.Name.LocalName, xattrib.Value)));
                            }
                            var entities = Entity.GetEntities(xml.Root, this).ToList();
                            var methods = Method.GetMethods(xml.Root, this, entities).ToList();
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
                                input.Add("Usings", Usings.EmptyIfNull().Select(u => new Dictionary<string, object>() { { "Using", u } }).Cast<object>().ToArray());
                                var file = parser.ParseText(template, input);
                                parser.StartOver();
                                using (var sw = new StreamWriter(Path.Combine(ModelOutPath, fileName)))
                                {
                                    sw.Write(file);
                                }
                            }
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
                                    input.Add("ClassAccess", ClassAccess ?? "public");
                                    input.Add("Usings", Usings.EmptyIfNull().Select(u => new Dictionary<string, object>() { { "Using", u } }).Cast<object>().ToArray());
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
            catch (Exception)
            {
                return "Unexpected Error while generating code. " + ConfigErrorMessage;
            }
            return null;
        }

        private static string GetTemplate(string name)
        {
            return ("CpsDbHelper.CodeGenerator.Templates." + name).GetResourceTextFromExecutingAssembly();
        }
    }
}
