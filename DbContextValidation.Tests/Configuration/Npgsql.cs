using System;
using System.Data.Common;
using Npgsql;

namespace DbContextValidation.Tests
{
    public static class Config
    {
        public static readonly Provider Provider = Provider.Npgsql;
        public static readonly string Schema = "public";

        private static readonly string Host = "localhost";
        private static readonly string Port = "5432";
        private static readonly string Database = "DbContextValidation";
        private static readonly string User = "postgres";
        private static readonly string Password = "docker";

        public static readonly string ConnectionString = $"Host={Host};Database={Database};UserName={User};Password={Password}";
         
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