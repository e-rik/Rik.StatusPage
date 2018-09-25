namespace Rik.StatusPage.Configuration
{
    public class XRoadProducerStatusProviderOptions : StatusProviderOptions
    {
        public string Consumer { get; set; }
        public string ProducerName { get; set; }
        public string ProtocolVersion { get; set; }
        public string Uri { get; set; }
        public string UserId { get; set; }

        public XRoadProducerStatusProviderOptions(string name)
            : base(name)
        { }
    }
}