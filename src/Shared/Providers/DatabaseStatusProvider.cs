using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            connectionType = GetConnectionType();
        }

        protected override ExternalUnit OnCheckStatus(ExternalUnit externalUnit)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                return externalUnit.SetStatus(UnitStatus.NotOk, $"Database connection string is required.");

            if (connectionType == null)
                return externalUnit.SetStatus(UnitStatus.NotOk, $"Cannot load database connection type: {string.Join(", ", ConnectionTypeNames)}.");

            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = VersionQuery;

            externalUnit.ServerPlatform = new ServerPlatform
            {
                Name = PlatformName,
                Version = Convert.ToString(command.ExecuteScalar())
            };

            return externalUnit.SetStatus(UnitStatus.Ok);
        }

        private IDbConnection CreateConnection()
        {
            var connection = (IDbConnection)Activator.CreateInstance(connectionType);

            connection.ConnectionString = options.ConnectionString;

            return connection;
        }

        private Type GetConnectionType()
        {
            return ConnectionTypeNames.Select(TypeHelper.FindType).FirstOrDefault();
        }
    }
}