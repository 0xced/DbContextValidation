namespace DbContextValidation.Tests
{
    public class Configuration : ConfigurationBase, IDockerDatabaseConfiguration
    {
        public const string Schema = null;

        private const string Host = "localhost";
        private const string Database = "DbContextValidation";
        private const string User = "root";
        private const string Password = "docker";

        public string ConnectionString(ushort port) => $"Host={Host};Port={port};Database={Database};UserName={User};Password={Password}";

        public string[] Arguments => new [] {
            $"-e MYSQL_ROOT_PASSWORD={Password}",
            $"-e MYSQL_DATABASE={Database}",
            "-e MYSQL_ROOT_HOST=%",
            $"--volume \"{SqlDirectory("SQL.MySQL")}:/docker-entrypoint-initdb.d:ro\"",
            "--publish 3306/tcp",
            "--detach",
            "mysql/mysql-server:5.7",
        };
    }
}