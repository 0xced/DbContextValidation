using System;
using Xunit.Fixture.DockerDb;

namespace DbContextValidation.Tests
{
    public class Configuration : ConfigurationBase, IDockerDatabaseConfiguration
    {
        public const string Schema = null;

        public string ConnectionString(ushort port) => $"Data Source={SqlFile("DbContextValidation.sqlite3")}";

#if NETFRAMEWORK
        public System.Data.Common.DbProviderFactory ProviderFactory => System.Data.SQLite.SQLiteFactory.Instance;
#else
        public System.Data.Common.DbProviderFactory ProviderFactory => Microsoft.Data.Sqlite.SqliteFactory.Instance;
#endif

        public override string ContainerName => null;

        public string[] Arguments => throw new NotSupportedException("SQLite doesn't require a Docker container.");
    }
}