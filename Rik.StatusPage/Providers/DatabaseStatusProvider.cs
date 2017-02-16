using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public abstract class DatabaseStatusProvider : StatusProvider
    {
        private readonly Type connectionType;

        protected abstract IEnumerable<string> ConnectionTypeNames { get; }
        protected abstract string VersionQuery { get; }

        protected DatabaseStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.ConnectionString))
                throw new ArgumentException("Database connection string is required.", nameof(configuration.ConnectionString));

            connectionType = GetConnectionType();
        }

        protected override ExternalUnit OnCheckStatus(ExternalUnit externalUnit)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = VersionQuery;

                    externalUnit.ServerPlatform = new ServerPlatform
                    {
                        Version = Convert.ToString(command.ExecuteScalar())
                    };

                    return externalUnit.SetStatus(UnitStatus.Ok);
                }
            }
        }

        private IDbConnection CreateConnection()
        {
            var connection = (IDbConnection) Activator.CreateInstance(connectionType);

            connection.ConnectionString = configuration.ConnectionString;

            return connection;
        }

        private Type GetConnectionType()
        {
            var type = ConnectionTypeNames.Select(FindType).FirstOrDefault();
            if (type == null)
                throw new Exception($"Cannot load database connection type: {string.Join(", ", ConnectionTypeNames)}.");

            return type;
        }

        private static Type FindType(string qualifiedName)
        {
            var type = Type.GetType(qualifiedName);
            if (type != null)
                return type;

            var nameParts = qualifiedName.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length < 2)
                return null;

            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == nameParts[1].Trim())?.GetType(nameParts[0].Trim());
        }
    }
}