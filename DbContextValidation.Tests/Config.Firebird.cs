using System;
using System.IO;

namespace DbContextValidation.Tests
{
    public static class Config
    {
        public static readonly string Schema = null;

        private const string Host = "localhost";
        public static ushort? Port;
        private const string Database = "DbContextValidation";
        private const string User = "firebird";
        private const string Password = "docker";

        public static string ConnectionString => $"DataSource={Host};Port={Port};Database={Database};User={User};Password={Password}";

        public static readonly string DockerContainerName = "DbContextValidation.Tests.Firebird";

        public static string DockerArguments(Func<string, string> sqlDirectory)
        {
            return string.Join(" ",
                $"-e FIREBIRD_DATABASE={Database}",
                $"-e FIREBIRD_USER={User}",
                $"-e FIREBIRD_PASSWORD={Password}",
                "-e EnableWireCrypt=true",
                "--publish 3050/tcp",
                "--detach",
                "jacobalberty/firebird:3.0.4");
        }

        public static string[] SqlScripts(Func<string, string> sqlDirectory)
        {
            var directory = sqlDirectory("SQL.Firebird");
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