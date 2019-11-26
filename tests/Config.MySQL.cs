using System.Collections.Generic;
using System.IO;
using Xunit.Fixture.Docker;

#if PROVIDER_MYSQL_POMELO
namespace DbContextValidation.Tests.MySQL.Pomelo
#else
namespace DbContextValidation.Tests.MySQL
#endif
{
    public class Configuration : ConfigurationBase, IDockerContainerConfiguration, IDatabaseConfiguration
    {
        public const string Schema = null;

        private const string Database = "DbContextValidation";
        private const string User = "root";
        private const string Password = "docker";

        public string ConnectionString(string host, ushort port) => $"Host={host};Port={port};Database={Database};UserName={User};Password={Password}";

        public System.Data.Common.DbProviderFactory ProviderFactory => MySql.Data.MySqlClient.MySqlClientFactory.Instance;

        public string ImageName => "mysql/mysql-server:5.7";

        public override IReadOnlyDictionary<string, string> EnvironmentVariables => new Dictionary<string, string>
        {
            ["MYSQL_DATABASE"] = Database,
            ["MYSQL_ROOT_PASSWORD"] = Password,
            ["MYSQL_ROOT_HOST"] = "%",
        };

        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.MySQL"));
    }
}