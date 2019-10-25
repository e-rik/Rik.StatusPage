using System;

namespace Rik.StatusPage.Configuration
{
    public class StatusProviderOptions
    {
        public string Name { get; }
        public string DisplayUri { get; set; }

        public StatusProviderOptions(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Status provider name is required.", nameof(name));

            Name = name;
        }
    }
}