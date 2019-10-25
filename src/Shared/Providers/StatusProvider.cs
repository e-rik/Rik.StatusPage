using System;
using System.Threading;
using System.Threading.Tasks;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public abstract class StatusProvider<TOptions> : IStatusProvider
        where TOptions : StatusProviderOptions
    {
        protected readonly TOptions options;

        public virtual string Name => options.Name;

        public abstract string DisplayUri { get; }

        protected StatusProvider(TOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected virtual ExternalUnit OnCheckStatus(ExternalUnit externalUnit) =>
            externalUnit;

        protected virtual Task<ExternalUnit> OnCheckStatusAsync(ExternalUnit externalUnit, CancellationToken cancellationToken) =>
            Task.FromResult(OnCheckStatus(externalUnit));

        public virtual async Task<ExternalUnit> CheckStatusAsync(CancellationToken cancellationToken)
        {
            var externalUnit = new ExternalUnit { Name = Name };

            try
            {
                externalUnit.Uri = string.IsNullOrWhiteSpace(options.DisplayUri) ? DisplayUri : options.DisplayUri;

                return await OnCheckStatusAsync(externalUnit, cancellationToken);
            }
            catch (Exception exception)
            {
                return externalUnit.SetStatus(UnitStatus.NotOk, exception.Message);
            }
        }
    }
}