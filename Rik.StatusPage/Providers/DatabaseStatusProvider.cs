using System;
using System.Data;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public class DatabaseStatusProvider : StatusProvider
    {
        private readonly string connectionString;
        private readonly Type connectionType;

        public DatabaseStatusProvider(string name, string connectionString, Type connectionType)
            : base(name)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Database connection string is required.", nameof(connectionString));

            if (connectionType == null)
                throw new ArgumentException("Database connection type is required.", nameof(connectionType));

            this.connectionString = connectionString;
            this.connectionType = connectionType;
        }

        protected override ExternalUnit OnCheckStatus(ExternalUnit externalUnit)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                //externalUnit.ServerPlatform.Version = connection.ServerVersion;

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1 FROM DUAL";

                    command.ExecuteScalar();
                    return externalUnit.SetStatus(UnitStatus.Ok);
                }
            }
        }

        private IDbConnection CreateConnection()
        {
            var connection = (IDbConnection) Activator.CreateInstance(connectionType);

            connection.ConnectionString = connectionString;

            return connection;
        }
    }
}