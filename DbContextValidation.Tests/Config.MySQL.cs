using System;

namespace DbContextValidation.Tests
{
    public static class Config
    {
        public static readonly string Schema = null;

        private const string Host = "localhost";
        public static ushort? Port;
        private const string Database = "DbContextValidation";
        private const string User = "root";
        private const string Password = "docker";

        public static string ConnectionString => $"Host={Host};Port={Port};Database={Database};UserName={User};Password={Password}";

        public static readonly string DockerContainerName = "DbContextValidation.Tests.MySQL";

        public static string DockerArguments(Func<string, string> sqlDirectory)
        {
            return string.Join(" ",
                $"-e MYSQL_ROOT_PASSWORD={Password}",
                $"-e MYSQL_DATABASE={Database}",
                "-e MYSQL_ROOT_HOST=%",
                $"--volume \"{sqlDirectory("SQL.MySQL")}:/docker-entrypoint-initdb.d:ro\"",
                "--publish 3306/tcp",
                "--detach",
                "mysql/mysql-server:5.7");
        }

        public static string[] SqlScripts(Func<string, string> sqlDirectory)
        {
            return new string[0];
        }
    }
}