using System;
using System.Collections.Generic;
using System.IO;
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

        public string ImageName => "genschsa/mssql-server-linux:latest";

        public override IReadOnlyDictionary<string, string> EnvironmentVariables => new Dictionary<string, string>
        {
            ["ACCEPT_EULA"] = "Y",
            ["MSSQL_SA_PASSWORD"] = Password,
        };

        public ushort Port => 1433;

        public override IReadOnlyDictionary<DirectoryInfo, string> Volumes => new Dictionary<DirectoryInfo, string> { [SqlDirectory("SQL.SqlServer")] = "/docker-entrypoint-initdb.d" };
    }
}