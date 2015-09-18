using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DotNetUtils;

namespace CpsDbHelper.CodeGenerator
{
    public class EntityProperty
    {
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlElement]
        public bool? Nullable { get; set; }
        [XmlIgnore]
        public bool Identity { get; set; }
        [XmlIgnore]
        public string ForeignName { get; set; }

        public string AttrBegin
        {
            get { return Attributes.IsNullOrEmpty() ? String.Empty : "["; }
        }
        public string AttrEnd
        {
            get { return Attributes.IsNullOrEmpty() ? String.Empty : "]"; }
        }
        [XmlIgnore]
        public object[] Attrs
        {
            get
            {
                return Attributes.EmptyIfNull().Select(a => new Dictionary<string, object>() {{"Attr", a}}).Cast<object>().ToArray();
            }
        }
        [XmlElement("Annotations")]
        public string[] Attributes;
    }
}
