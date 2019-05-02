namespace DbContextValidation.Tests
{
    public class Configuration : ConfigurationBase, IDockerDatabaseConfiguration
    {
        public const string Schema = "dbo";

        private const string Host = "localhost";
        private const string Database = "tempdb";
        private const string User = "sa";
        private const string Password = "SqlServer-doc4er";

        public string ConnectionString(ushort port) => $"Server={Host},{port};Database={Database};User Id={User};Password={Password}";

        public string ContainerName => "DbContextValidation.Tests.SqlServer";

        public string[] Arguments => new [] {
            "-e ACCEPT_EULA=Y",
            $"-e MSSQL_SA_PASSWORD={Password}",
            $"--volume \"{SqlDirectory("SQL.SqlServer")}:/docker-entrypoint-initdb.d:ro\"",
            "--publish 1433/tcp",
            "--detach",
            "genschsa/mssql-server-linux:latest",
        };
    }
}