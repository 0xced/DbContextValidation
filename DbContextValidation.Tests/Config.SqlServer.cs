using System;
using Xunit.Fixture.DockerDb;

namespace DbContextValidation.Tests.SqlServer
{
    public class Configuration : ConfigurationBase, IDockerDatabaseConfiguration
    {
        public const string Schema = "dbo";

        private const string Database = "tempdb";
        private const string User = "sa";
        private const string Password = "SqlServer-doc4er";

        public string ConnectionString(string host, ushort port) => $"Server={host},{port};Database={Database};User Id={User};Password={Password}";

        public System.Data.Common.DbProviderFactory ProviderFactory => System.Data.SqlClient.SqlClientFactory.Instance;

        public override TimeSpan Timeout => TimeSpan.FromSeconds(45);

        public string[] Arguments => new [] {
            "-e ACCEPT_EULA=Y",
            $"-e MSSQL_SA_PASSWORD={Password}",
            $"--volume \"{SqlDirectory("SQL.SqlServer")}:/docker-entrypoint-initdb.d:ro\"",
            "--publish 1433/tcp",
            "--detach",
            "genschsa/mssql-server-linux:latest",
        };
    }
}