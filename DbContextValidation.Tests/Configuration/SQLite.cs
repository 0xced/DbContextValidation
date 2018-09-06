using System;
using System.Data.Common;

namespace DbContextValidation.Tests
{
    public static class Config
    {
        public static readonly Provider Provider = Provider.SQLite;
        public static readonly string Schema = null;

        public static readonly string ConnectionString = "Data Source=../../../../DbContextValidation.Tests/DbContextValidation.sqlite3";

        public static string DockerArguments(Func<string, string> sqlDirectory)
        {
            throw new NotSupportedException("SQLite doesn't require a Docker container.");
        }
         
        public static DbConnection CreateDbConnection()
        {
            throw new NotSupportedException("SQLite doesn't require a Docker container.");
        }
    }
}