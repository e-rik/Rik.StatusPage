using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Xml;

namespace Rik.StatusPage.Configuration
{
    public class ApplicationConfigurationElement : ConfigurationElement
    {
        private readonly IList<XmlElement> unrecognizedElements = new List<XmlElement>();

        public IReadOnlyCollection<XmlElement> UnrecognizedElements => new ReadOnlyCollection<XmlElement>(unrecognizedElements);

        [ConfigurationProperty("name", IsRequired = false)]
        public string Name => ((string)this["name"]).GetAppSettingsOrValue();

        [ConfigurationProperty("version", IsRequired = false)]
        public string Version => ((string)this["version"]).GetAppSettingsOrValue();

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            using (var subReader = reader.ReadSubtree())
            {
                var doc = new XmlDocument();
                doc.Load(subReader);
                unrecognizedElements.Add(doc.DocumentElement);
            }

            return true;
        }
    }
}