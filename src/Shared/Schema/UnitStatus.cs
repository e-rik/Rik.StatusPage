using System;
using System.Xml.Serialization;

namespace Rik.StatusPage.Schema
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public enum UnitStatus
    {
        [XmlEnum("OK")]
        Ok,

        [XmlEnum("NOK")]
        NotOk
    }
}