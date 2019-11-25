using System.Collections.Generic;
using System.IO;
using Xunit.Fixture.Docker;

namespace DbContextValidation.Tests.Npgsql
{
    public class Configuration : ConfigurationBase, IDockerContainerConfiguration, IDatabaseConfiguration
    {
        public const string Schema = "public";

        private const string Database = "DbContextValidation";
        private const string User = "postgres";
        private const string Password = "docker";

        public string ConnectionString(string host, ushort port) => $"Host={host};Port={port};Database={Database};UserName={User};Password={Password}";

        public System.Data.Common.DbProviderFactory ProviderFactory => global::Npgsql.NpgsqlFactory.Instance;

        public string ImageName => "postgres:10.5-alpine";

        public override IReadOnlyDictionary<string, string> EnvironmentVariables => new Dictionary<string, string>
        {
            ["POSTGRES_DB"] = Database,
            ["POSTGRES_PASSWORD"] = Password,
        };

        public override IReadOnlyDictionary<DirectoryInfo, string> Volumes => new Dictionary<DirectoryInfo, string> { [SqlDirectory("SQL.Common")] = "/docker-entrypoint-initdb.d" };
    }
}