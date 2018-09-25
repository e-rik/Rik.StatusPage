using System;
using System.Threading.Tasks;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public abstract class StatusProvider<TOptions> : IStatusProvider
        where TOptions : StatusProviderOptions
    {
        protected readonly TOptions options;

        public abstract string DisplayUri { get; }

        protected StatusProvider(TOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(options.Name))
                throw new ArgumentException("Status provider name is required.", nameof(options.Name));

            this.options = options;
        }

        protected abstract Task<ExternalUnit> OnCheckStatusAsync(ExternalUnit externalUnit);

        public virtual async Task<ExternalUnit> CheckStatusAsync()
        {
            var externalUnit = new ExternalUnit { Name = options.Name };

            try
            {
                externalUnit.Uri = string.IsNullOrWhiteSpace(options.DisplayUri) ? DisplayUri : options.DisplayUri;

                return await OnCheckStatusAsync(externalUnit);
            }
            catch (Exception exception)
            {
                return externalUnit.SetStatus(UnitStatus.NotOk, exception.Message);
            }
        }
    }
}