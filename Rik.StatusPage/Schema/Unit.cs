using System;
using System.Xml.Serialization;

namespace Rik.StatusPage.Schema
{
    [Serializable]
    [XmlType("unit_type")]
    public class Unit
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("status", Order = 1, IsNullable = false)]
        public UnitStatus Status { get; set; } = UnitStatus.NotOk;

        [XmlElement("status_msg", Order = 2, IsNullable = false)]
        public string StatusMessage { get; set; }

        [XmlIgnore]
        public bool StatusMessageSpecified => StatusMessage != null;

        [XmlElement("server_platform", Order = 3, IsNullable = false)]
        public ServerPlatform ServerPlatform { get; set; }

        [XmlIgnore]
        public bool ServerPlatformSpecified => ServerPlatform != null;
    }
}