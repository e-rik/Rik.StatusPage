namespace Rik.StatusPage.Configuration
{
    public class WebServiceStatusProviderOptions : StatusProviderOptions
    {
        public string Uri { get; set; }

        public WebServiceStatusProviderOptions(string name)
            : base(name)
        { }
    }
}