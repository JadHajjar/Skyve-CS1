using System;
using System.Xml.Serialization;

namespace CompatibilityReport.CatalogData
{
    [Serializable]
    public class LocalizedNote
    {
        [XmlAttribute("localeId")]
        public string LocaleId { get; private set; }
        
        [XmlText]
        public string Value { get; private set; }

        public LocalizedNote() {
            LocaleId = string.Empty;
            Value = string.Empty;
        }

        public LocalizedNote(string note, string localeId) {
            Value = note;
            LocaleId = localeId;
        }

        public void UpdateNote(string note, string localeId) {
            Value = note;
            LocaleId = localeId;
        }
    }
}
