using System;
using System.Data.Common;
using Npgsql;

namespace DbContextValidation.Tests
{
    public static class Config
    {
        public static readonly string Schema = "public";

        private const string Host = "localhost";
        private const string Port = "5432";
        private const string Database = "DbContextValidation";
        private const string User = "postgres";
        private const string Password = "docker";

        public static readonly string ConnectionString = $"Host={Host};Port={Port};Database={Database};UserName={User};Password={Password}";

        public static readonly string DockerContainerName = "DbContextValidation.Tests.Npgsql";

        public static string DockerArguments(Func<string, string> sqlDirectory)
        {
            return string.Join(" ",
                $"-e POSTGRES_PASSWORD={Password}",
                $"-e POSTGRES_DB={Database}",
                $"--volume \"{sqlDirectory("SQL.Common")}:/docker-entrypoint-initdb.d:ro\"",
                $"--publish {Port}:5432/tcp",
                "--detach",
                "postgres:10.5-alpine");
        }

        public static DbConnection CreateDbConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }
    }
}