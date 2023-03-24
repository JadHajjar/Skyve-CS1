
using System;
using System.Xml.Serialization;

namespace CompatibilityReport.CatalogData
{
    
    [Serializable]
    public class ElementWithId
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlText]
        public string Value { get; set; }

        public override string ToString() {
            return $"{Value}";
        }
    }
}
