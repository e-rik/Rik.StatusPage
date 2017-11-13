using System.Collections.Generic;
using Rik.StatusPage.Configuration;

namespace Rik.StatusPage.Providers
{
    public class MsSqlDatabaseStatusProvider : DatabaseStatusProvider
    {
        protected override IEnumerable<string> ConnectionTypeNames => new []
        {
            "System.Data.SqlClient.SqlConnection, System.Data"
        };

        protected override string VersionQuery => "SELECT SERVERPROPERTY('productversion'), SERVERPROPERTY('productlevel'), SERVERPROPERTY('edition')";
        protected override string PlatformName => "Microsoft SQL Server";

        public MsSqlDatabaseStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        { }
    }
}