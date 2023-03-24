using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CompatibilityReport.CatalogData
{
    /// <summary>Groups are used for required and recommended mods. If a required/recommended mod is not subscribed but is a group member, 
    ///          the reporter checks if another member is subscribed. The original required/recommended mod will not be reported missing if so.</summary>
    /// <remarks>A mod can only be a member of one group.</remarks>
    [Serializable] 
    public class Group
    {
        public ulong GroupID { get; set; }
        public string Name { get; set; }
        [XmlArrayItem("SteamID")] public List<ulong> GroupMembers { get; set; } = new List<ulong>();


        /// <summary>Default constructor for deserialization.</summary>
        private Group()
        {
            // Nothing to do here
        }


        /// <summary>Constructor for group creation.</summary>
        public Group(ulong groupID, string name)
        {
            GroupID = groupID;
            Name = name ?? "";
        }


        /// <summary>Adds a mod to the group.</summary>
        public void AddMember(ulong steamID)
        {
            if (!GroupMembers.Contains(steamID))
            {
                GroupMembers.Add(steamID);
            }
        }


        /// <summary>Removes a mod from the group.</summary>
        /// <returns>True if removal succeeded, false if not.</returns>
        public bool RemoveMember(ulong steamID)
        {
            return GroupMembers.Remove(steamID);
        }


        /// <summary>Converts the group to a string containing the group ID and name.</summary>
        /// <returns>A string representing the group.</returns>
        public new string ToString()
        {
            return $"[Group { GroupID }] { Name }";
        }
    }
}
