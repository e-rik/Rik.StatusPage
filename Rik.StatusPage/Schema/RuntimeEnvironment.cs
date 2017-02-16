using System;
using System.Xml.Serialization;

namespace Rik.StatusPage.Schema
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public class RuntimeEnvironment
    {
        [XmlText]
        public string Name { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }
    }
}