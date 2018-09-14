using System;

namespace DbContextValidation.Tests
{
    public static class Config
    {
        public static readonly string Schema = null;

        public static ushort? Port;
        
        public static readonly string ConnectionString = "Data Source=DbContextValidation.sqlite3";

        public static readonly string DockerContainerName = null;

        public static string DockerArguments(Func<string, string> sqlDirectory)
        {
            throw new NotSupportedException("SQLite doesn't require a Docker container.");
        }
    }
}