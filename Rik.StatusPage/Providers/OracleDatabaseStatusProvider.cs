using System.Collections.Generic;
using Rik.StatusPage.Configuration;

namespace Rik.StatusPage.Providers
{
    public class OracleDatabaseStatusProvider : DatabaseStatusProvider
    {
        protected override IEnumerable<string> ConnectionTypeNames { get; } = new[]
        {
            "Oracle.ManagedDataAccess.Client.OracleConnection, Oracle.ManagedDataAccess",
            "Oracle.DataAccess.Client.OracleConnection, Oracle.DataAccess"
        };

        protected override string VersionQuery => "SELECT version FROM V$INSTANCE";

        public OracleDatabaseStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        { }
    }
}