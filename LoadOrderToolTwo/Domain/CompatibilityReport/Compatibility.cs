using System;
using System.Xml.Serialization;

namespace CompatibilityReport.CatalogData
{
    [Serializable]
    public class Compatibility
    {
        // The mod names are only for catalog readability, they are not used anywhere else.
        public ulong FirstModID { get; set; }
        public string FirstModName { get; set; }

        public ulong SecondModID { get; set; }
        public string SecondModName { get; set; }

        // The compatibility status is from the perspective of the first mod.
        public Enums.CompatibilityStatus Status { get; set; }
        [XmlElement("Note")]
        public ElementWithId Note { get; set; }

        /// <summary>Default constructor for deserialization.</summary>
        private Compatibility()
        {
            // Nothing to do here.
        }


        /// <summary>Constructor for compatibility creation.</summary>
        public Compatibility(ulong firstModID, string firstModName, ulong secondModID, string secondModName, Enums.CompatibilityStatus status, ElementWithId note)
        {
            FirstModID = firstModID;
            FirstModName = firstModName ?? "";

            SecondModID = secondModID;
            SecondModName = secondModName ?? "";

            Status = status;
            Note = note ?? new ElementWithId();
        }


        /// <summary>Updates mod names.</summary>
        /// <remarks>The mod names are only for catalog readability, they are not used anywhere else.</remarks>
        public void UpdateModNames(string firstModName, string secondModName)
        {
            FirstModName = firstModName ?? "";
            SecondModName = secondModName ?? "";
        }
    }
}
