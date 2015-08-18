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
    public class StoredProcedure
    {
        public string Name;
        public IList<EntityProperty> Params;

        public static IEnumerable<StoredProcedure> GetStoredProcedures(XElement xml, DacpacExtractor extractor)
        {
            return GetParamedDbos("SqlProcedure", xml, extractor);
        }
        public static IEnumerable<StoredProcedure> GetlMultiStatementTableValuedFunctions(XElement xml, DacpacExtractor extractor)
        {
            return GetParamedDbos("SqlMultiStatementTableValuedFunction", xml, extractor);
        }
        public static IEnumerable<StoredProcedure> GetScalarFunctions(XElement xml, DacpacExtractor extractor)
        {
            return GetParamedDbos("SqlScalarFunction", xml, extractor);
        }
        public static IEnumerable<StoredProcedure> GetInlineTableValuedFunctions(XElement xml, DacpacExtractor extractor)
        {
            return GetParamedDbos("SqlInlineTableValuedFunction", xml, extractor);
        }

        private static IEnumerable<StoredProcedure> GetParamedDbos(string type ,XElement xml, DacpacExtractor extractor)
        {
            string xpath = "/DataSchemaModel/Model/Element[@Type='{0}']".FormatInvariantCulture(type);
            var elements = xml.XPathSelectElements(xpath);
            foreach (var element in elements)
            {
                var ret = new StoredProcedure
                {
                    Name = element.GetAttributeString("Name"),
                    Params =
                        element.XPathSelectElements(
                            "Relationship[@Name='Parameters']/Entry/Element[@Type='SqlSubroutineParameter']")
                            .Select(e => new EntityProperty() {Name = e.GetAttributeString("Name"), Type = e.XPathSelectElement("Relationship[@Name='Type']/Entry/Element[@Type='SqlTypeSpecifier']/Relationship[@Name='Type']/Entry/References").GetAttributeString("Name")})
                            .ToList()
                };
                yield return ret;
            }
        }

    }
}
