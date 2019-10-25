using System.Collections.Generic;
using System.Reflection.Emit;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Internal;

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

        public override string DisplayUri => getDatabaseUri(options.ConnectionString);

        public MsSqlDatabaseStatusProvider(DatabaseStatusProviderOptions options)
            : base(options)
        { }

        private static GetDatabaseUriDelegate CreateGetDatabaseUri()
        {
            var method = new DynamicMethod("MsSqlDatabaseStatusProvider_getDatabaseUri", typeof(string), new [] { typeof(string) });
            var il = method.GetILGenerator();

            var builderType = TypeHelper.FindType("System.Data.SqlClient.SqlConnectionStringBuilder, System.Data");
            if (builderType == null)
            {
                // return string.Emtpy
                il.Emit(OpCodes.Ldstr, string.Empty);
            }
            else
            {
                // var builder = new SqlConnectionStringBuilder(connectionString);
                var builder = il.DeclareLocal(builderType);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Newobj, builderType.GetConstructor(new[] {typeof(string)}));
                il.Emit(OpCodes.Stloc, builder);

                // return string.Format("{0}@{1}", builder.InitialCatalog, builder.DataSource);
                il.Emit(OpCodes.Ldstr, "{0}@{1}");
                il.Emit(OpCodes.Ldloc, builder);
                il.Emit(OpCodes.Callvirt, builderType.GetProperty("InitialCatalog").GetGetMethod());
                il.Emit(OpCodes.Ldloc, builder);
                il.Emit(OpCodes.Callvirt, builderType.GetProperty("DataSource").GetGetMethod());
                il.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new[] {typeof(string), typeof(object), typeof(object)}));
            }

            il.Emit(OpCodes.Ret);

            return (GetDatabaseUriDelegate)method.CreateDelegate(typeof(GetDatabaseUriDelegate));
        }
    }
}