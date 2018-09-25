namespace Rik.StatusPage.Configuration
{
    public class DatabaseStatusProviderOptions : StatusProviderOptions
    {
        public string ConnectionString { get; set; }

        public DatabaseStatusProviderOptions(string name)
            : base(name)
        { }
    }
}