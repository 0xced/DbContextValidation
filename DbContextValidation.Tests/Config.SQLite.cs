using System;
using Xunit.Fixture.Docker;

namespace DbContextValidation.Tests.SQLite
{
    public class Configuration : ConfigurationBase, IDockerContainerConfiguration, IDatabaseConfiguration
    {
        public const string Schema = null;

        public string ConnectionString(string host, ushort port) => $"Data Source={SqlFile("DbContextValidation.sqlite3")}";

#if NETFRAMEWORK
        public System.Data.Common.DbProviderFactory ProviderFactory => System.Data.SQLite.SQLiteFactory.Instance;
#else
        public System.Data.Common.DbProviderFactory ProviderFactory => Microsoft.Data.Sqlite.SqliteFactory.Instance;
#endif

        public override string ContainerName => null;

        public string ImageName => null; //throw new NotSupportedException("SQLite doesn't require a Docker container.");

        public ushort Port => 0; //throw new NotSupportedException("SQLite doesn't require a Docker container.");
    }
}