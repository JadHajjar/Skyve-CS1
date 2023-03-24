using System;
using System.Xml.Serialization;

namespace CompatibilityReport.CatalogData
{    
    [Serializable]
    public class TextElement
    {
        
        [XmlText]
        public string Value { get; set; }
        
        public override string ToString() {
            return $"{Value}";
        }
    }
}
