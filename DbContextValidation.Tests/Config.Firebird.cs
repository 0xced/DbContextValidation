using System.Collections.Generic;
using Xunit.Fixture.Docker;

namespace DbContextValidation.Tests.Firebird
{
    public class Configuration : ConfigurationBase, IDockerContainerConfiguration, IDatabaseConfiguration
    {
        public const string Schema = null;

        private const string Database = "DbContextValidation";
        private const string User = "firebird";
        private const string Password = "docker";

        public string ConnectionString(string host, ushort port) => $"DataSource={host};Port={port};Database={Database};User={User};Password={Password}";

        public System.Data.Common.DbProviderFactory ProviderFactory => FirebirdSql.Data.FirebirdClient.FirebirdClientFactory.Instance;

        public string ImageName => "jacobalberty/firebird:3.0.4";

        public override IReadOnlyDictionary<string, string> EnvironmentVariables => new Dictionary<string, string>
        {
            ["FIREBIRD_DATABASE"] = Database,
            ["FIREBIRD_USER"] = User,
            ["FIREBIRD_PASSWORD"] = Password,
            ["EnableWireCrypt"] = "true",
        };

        public ushort Port => 3050;

        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.Firebird"));
    }
}