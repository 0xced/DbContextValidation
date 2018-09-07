using System;

namespace DbContextValidation.Tests
{
    public static class Config
    {
        public static readonly string Schema = "dbo";

        private const string Host = "localhost";
        private const string Port = "1433";
        private const string Database = "tempdb";
        private const string User = "sa";
        private const string Password = "SqlServer-doc4er";

        public static readonly string ConnectionString = $"Server={Host},{Port};Database={Database};User Id={User};Password={Password}";

        public static readonly string DockerContainerName = "DbContextValidation.Tests.SqlServer";

        public static string DockerArguments(Func<string, string> sqlDirectory)
        {
            return string.Join(" ",
                "-e ACCEPT_EULA=Y",
                $"-e MSSQL_SA_PASSWORD={Password}",
                $"--volume \"{sqlDirectory("SQL.SqlServer")}:/docker-entrypoint-initdb.d:ro\"",
                $"--publish {Port}:1433/tcp",
                "--detach",
                "genschsa/mssql-server-linux:latest");
        }
    }
}