using System.Collections.Generic;
using System.Reflection.Emit;
using Rik.StatusPage.Configuration;

namespace Rik.StatusPage.Providers
{
    public class MsSqlDatabaseStatusProvider : DatabaseStatusProvider
    {
        private static readonly GetDatabaseUriDelegate getDatabaseUri = CreateGetDatabaseUri();

        protected override IEnumerable<string> ConnectionTypeNames => new []
        {
            "System.Data.SqlClient.SqlConnection, System.Data"
        };

        protected override string VersionQuery => "SELECT SERVERPROPERTY('productversion'), SERVERPROPERTY('productlevel'), SERVERPROPERTY('edition')";
        protected override string PlatformName => "Microsoft SQL Server";

        protected override string GetDatabaseUri()
        {
            return getDatabaseUri(configuration.ConnectionString);
        }

        public MsSqlDatabaseStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        { }

        private static GetDatabaseUriDelegate CreateGetDatabaseUri()
        {
            var method = new DynamicMethod("MsSqlDatabaseStatusProvider_getDatabaseUri", typeof(string), new [] { typeof(string) });
            var il = method.GetILGenerator();

            il.Emit(OpCodes.Ldstr, "test");
            il.Emit(OpCodes.Ret);
            
            return (GetDatabaseUriDelegate)method.CreateDelegate(typeof(GetDatabaseUriDelegate));
        }
    }
}