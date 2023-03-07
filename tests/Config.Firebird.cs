using System.Collections.Generic;
using System.Data.Common;
using DockerRunner.Database;
using static DbContextValidation.Tests.SqlInitializationHelper;

namespace DbContextValidation.Tests
{
    public class Configuration : DockerDatabaseContainerConfiguration
    {
        public const string Schema = null;

        private const string Database = "DbContextValidation";
        private const string User = "firebird";
        private const string Password = "docker";

        public override string ImageName => "jacobalberty/firebird";

        public override string ConnectionString(string host, ushort port) => $"DataSource={host};Port={port};Database={Database};User={User};Password={Password}";

        public override DbProviderFactory ProviderFactory => FirebirdSql.Data.FirebirdClient.FirebirdClientFactory.Instance;

        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.Firebird"));

        public override IReadOnlyDictionary<string, string> EnvironmentVariables => new Dictionary<string, string>
        {
            ["FIREBIRD_DATABASE"] = Database,
            ["FIREBIRD_USER"] = User,
            ["FIREBIRD_PASSWORD"] = Password,
            ["EnableWireCrypt"] = "true",
        };
    }
}