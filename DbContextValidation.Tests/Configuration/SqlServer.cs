using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace DbContextValidation.Tests
{
    public static class Config
    {
        public static readonly Provider Provider = Provider.SqlServer;
        public static readonly string Schema = "dbo";

        private static readonly string Host = "localhost";
        private static readonly string Port = "1433";
        private static readonly string Database = "tempdb";
        private static readonly string User = "sa";
        private static readonly string Password = "SqlServer-doc4er";
 
        public static readonly string ConnectionString = $"Server={Host};Database={Database};User Id={User};Password={Password}";
         
        public static string DockerArguments(Func<string, string> sqlDirectory)
        {
            return string.Join(" ",
                "-e ACCEPT_EULA=Y",
                $"-e MSSQL_SA_PASSWORD={Password}",
                "-e MYSQL_ROOT_HOST=%",
                $"--volume \"{sqlDirectory("SQL.SqlServer")}:/docker-entrypoint-initdb.d:ro\"",
                $"--publish {Port}:1433/tcp",
                "--detach",
                "genschsa/mssql-server-linux:latest");
        }
         
        public static DbConnection CreateDbConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}