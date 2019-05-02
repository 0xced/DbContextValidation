using System.IO;

namespace DbContextValidation.Tests
{
    public class Configuration : ConfigurationBase, IDockerDatabaseConfiguration
    {
        public const string Schema = null;

        private const string Host = "localhost";
        private const string Database = "DbContextValidation";
        private const string User = "firebird";
        private const string Password = "docker";

        public string ConnectionString(ushort port) => $"DataSource={Host};Port={port};Database={Database};User={User};Password={Password}";

        public string ContainerName => "DbContextValidation.Tests.Firebird";

        public string[] Arguments => new [] {
            $"-e FIREBIRD_DATABASE={Database}",
            $"-e FIREBIRD_USER={User}",
            $"-e FIREBIRD_PASSWORD={Password}",
            "-e EnableWireCrypt=true",
            "--publish 3050/tcp",
            "--detach",
            "jacobalberty/firebird:3.0.4",
        };

        public override string[] SqlScripts
        {
            get
            {
                var directory = SqlDirectory("SQL.Firebird");
                return new []
                {
                    File.ReadAllText(Path.Combine(directory, "1. Drop tOrders.sql")),
                    File.ReadAllText(Path.Combine(directory, "2. Drop tCustomers.sql")),
                    File.ReadAllText(Path.Combine(directory, "3. Create tCustomers.sql")),
                    File.ReadAllText(Path.Combine(directory, "4. Create tOrders.sql")),
                };
            }
        }
    }
}