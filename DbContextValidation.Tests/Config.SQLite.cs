using System;

namespace DbContextValidation.Tests
{
    public class Configuration : ConfigurationBase, IDockerDatabaseConfiguration
    {
        public const string Schema = null;

        public string ConnectionString(ushort port) => "Data Source=DbContextValidation.sqlite3";

        public override string ContainerName => null;

        public string[] Arguments => throw new NotSupportedException("SQLite doesn't require a Docker container.");
    }
}