using System;
using System.Xml.Serialization;

namespace Rik.StatusPage.Schema
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public class ExternalUnit : Unit
    {
        [XmlElement("uri", IsNullable = false, Order = 1)]
        public string Uri { get; set; }
    }
}