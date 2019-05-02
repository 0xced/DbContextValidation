namespace DbContextValidation.Tests
{
    public class Configuration : ConfigurationBase, IDockerDatabaseConfiguration
    {
        public const string Schema = "public";

        private const string Host = "localhost";
        private const string Database = "DbContextValidation";
        private const string User = "postgres";
        private const string Password = "docker";

        public string ConnectionString(ushort port) => $"Host={Host};Port={port};Database={Database};UserName={User};Password={Password}";
        
        public string ContainerName => "DbContextValidation.Tests.Npgsql";

        public string[] Arguments => new [] {
            $"-e POSTGRES_PASSWORD={Password}",
            $"-e POSTGRES_DB={Database}",
            $"--volume \"{SqlDirectory("SQL.Common")}:/docker-entrypoint-initdb.d:ro\"",
            "--publish 5432/tcp",
            "--detach",
            "postgres:10.5-alpine",
        };
    }
}