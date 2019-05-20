using System;
using Xunit.Fixture.DockerDb;

namespace DbContextValidation.Tests.SQLite
{
    public class Configuration : ConfigurationBase, IDockerDatabaseConfiguration
    {
        public const string Schema = null;

        public string ConnectionString(string host, ushort port) => $"Data Source={SqlFile("DbContextValidation.sqlite3")}";

#if NETFRAMEWORK
        public System.Data.Common.DbProviderFactory ProviderFactory => System.Data.SQLite.SQLiteFactory.Instance;
#else
        public System.Data.Common.DbProviderFactory ProviderFactory => Microsoft.Data.Sqlite.SqliteFactory.Instance;
#endif

        public override string ContainerName => null;

        public string ImageName => throw new NotSupportedException("SQLite doesn't require a Docker container.");

        public ushort Port => throw new NotSupportedException("SQLite doesn't require a Docker container.");
    }
}