using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Internal;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public abstract class DatabaseStatusProvider : StatusProvider<DatabaseStatusProviderOptions>
    {
        protected delegate string GetDatabaseUriDelegate(string connectionString);

        private readonly Type connectionType;

        protected abstract IEnumerable<string> ConnectionTypeNames { get; }
        protected abstract string VersionQuery { get; }
        protected abstract string PlatformName { get; }

        protected DatabaseStatusProvider(DatabaseStatusProviderOptions options)
            : base(options)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                throw new ArgumentException("Database connection string is required.", nameof(options.ConnectionString));

            connectionType = GetConnectionType();
        }

        protected override Task<ExternalUnit> OnCheckStatusAsync(ExternalUnit externalUnit)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = VersionQuery;

                    externalUnit.ServerPlatform = new ServerPlatform
                    {
                        Name = PlatformName,
                        Version = Convert.ToString(command.ExecuteScalar())
                    };

                    return Task.FromResult(externalUnit.SetStatus(UnitStatus.Ok));
                }
            }
        }

        private IDbConnection CreateConnection()
        {
            var connection = (IDbConnection) Activator.CreateInstance(connectionType);

            connection.ConnectionString = options.ConnectionString;

            return connection;
        }

        private Type GetConnectionType()
        {
            var type = ConnectionTypeNames.Select(TypeHelper.FindType).FirstOrDefault();
            if (type == null)
                throw new Exception($"Cannot load database connection type: {string.Join(", ", ConnectionTypeNames)}.");

            return type;
        }
    }
}