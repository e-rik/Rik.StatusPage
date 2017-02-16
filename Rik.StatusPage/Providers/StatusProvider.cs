using System;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public abstract class StatusProvider
    {
        private readonly string name;

        protected StatusProvider(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Status provider name is required.", nameof(name));

            this.name = name;
        }

        protected abstract ExternalUnit OnCheckStatus(ExternalUnit externalUnit);

        public virtual ExternalUnit CheckStatus()
        {
            var externalUnit = new ExternalUnit { Name = name };

            try
            {
                return OnCheckStatus(externalUnit);
            }
            catch (Exception exception)
            {
                return externalUnit.SetStatus(UnitStatus.NotOk, exception.Message);
            }
        }
    }
}