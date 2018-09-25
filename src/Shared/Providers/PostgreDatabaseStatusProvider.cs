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
        protected override string PlatformName { get; } = "PostgreSQL Database";

        public override string DisplayUri => "";

        public PostgreDatabaseStatusProvider(DatabaseStatusProviderOptions options)
            : base(options)
        { }
    }
}