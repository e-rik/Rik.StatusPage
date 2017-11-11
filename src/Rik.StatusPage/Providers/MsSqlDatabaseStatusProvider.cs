using System.Collections.Generic;
using Rik.StatusPage.Configuration;

namespace Rik.StatusPage.Providers
{
    public class MsSqlDatabaseStatusProvider : DatabaseStatusProvider
    {
        protected override IEnumerable<string> ConnectionTypeNames { get; } = new []
        {
            "System.Data.SqlClient.SqlConnection, System.Data"
        };

        protected override string VersionQuery { get; } = "SELECT SERVERPROPERTY('productversion'), SERVERPROPERTY('productlevel'), SERVERPROPERTY('edition')";
        protected override string PlatformName { get; } = "Microsoft SQL Server";

        protected override string GetDatabaseUri()
        {
            return "";
        }

        public MsSqlDatabaseStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        { }
    }
}