namespace Rik.StatusPage.Configuration
{
    public class FileStorageStatusProviderOptions : StatusProviderOptions
    {
        public string StoragePath { get; set; }
        public bool RequireRead { get; set; }
        public bool RequireWrite { get; set; }

        public FileStorageStatusProviderOptions(string name)
            : base(name)
        { }
    }
}