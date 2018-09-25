namespace Rik.StatusPage.Configuration
{
    public class StatusProviderOptions
    {
        public string Name { get; }
        public string DisplayUri { get; set; }

        public StatusProviderOptions(string name)
        {
            Name = name;
        }
    }
}