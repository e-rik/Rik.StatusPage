using System;
using System.Xml;
using System.Xml.Serialization;

namespace Rik.StatusPage.Schema
{
    [Serializable]
    [XmlRoot("app", Namespace = "", IsNullable = false)]
    [XmlType(AnonymousType = true)]
    public class Application : Unit
    {
        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlIgnore]
        public bool VersionSpecified => Version != null;

        [XmlElement("runtime_environment", Order = 1, IsNullable = false)]
        public RuntimeEnvironment RuntimeEnvironment { get; set; }

        [XmlIgnore]
        public bool RuntimeEnvironmentSpecified => RuntimeEnvironment != null;

        [XmlArray("external_dependencies", Order = 2, IsNullable = false)]
        [XmlArrayItem("unit", IsNullable = false)]
        public ExternalUnit[] ExternalDependencies { get; set; }

        [XmlAnyElement(Order = 3)]
        public XmlElement[] AdditionalInfo { get; set; }
    }
}