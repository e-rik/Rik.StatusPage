using System.Collections.Generic;
using Rik.StatusPage.Configuration;

namespace Rik.StatusPage.Providers
{
    public class PostgreDatabaseStatusProvider : DatabaseStatusProvider
    {
        protected override IEnumerable<string> ConnectionTypeNames { get; } = new []
        {
            "Npgsql.NpgsqlConnection, Npgsql"
        };

        protected override string VersionQuery { get; } = "SELECT version();";

        public PostgreDatabaseStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        { }
    }
}