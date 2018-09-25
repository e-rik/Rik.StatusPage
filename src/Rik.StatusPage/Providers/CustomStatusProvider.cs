using System;
using System.Threading.Tasks;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Internal;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public class CustomStatusProvider : StatusProvider<CustomStatusProviderOptions>
    {
        private readonly StatusProvider<CustomStatusProviderOptions> customStatusProvider;

        public override string DisplayUri => customStatusProvider.DisplayUri;

        public CustomStatusProvider(CustomStatusProviderOptions options)
            : base(options)
        {
            if (string.IsNullOrWhiteSpace(options.Class))
                throw new ArgumentException("Class name is required for custom provider.", nameof(options.Class));

            var statusProviderType = TypeHelper.FindType(options.Class);
            if (statusProviderType == null)
                throw new Exception($"Cannot load custom provider type: {options.Class}.");

            customStatusProvider = (StatusProvider<CustomStatusProviderOptions>)Activator.CreateInstance(statusProviderType, options);
        }

        protected override Task<ExternalUnit> OnCheckStatusAsync(ExternalUnit externalUnit)
        {
            return customStatusProvider.CheckStatusAsync();
        }
    }
}