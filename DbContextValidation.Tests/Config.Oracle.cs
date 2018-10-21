using System;
using System.IO;

namespace DbContextValidation.Tests
{
    public static class Config
    {
        public static readonly string Schema = null;

        private const string Host = "localhost";
        public static ushort? Port;
        private const string Sid = "xe";
        private const string User = "system";
        private const string Password = "oracle";

        public static string ConnectionString => $"User Id={User};Password={Password};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST={Host})(PORT={Port}))(CONNECT_DATA=(SID={Sid})))";

        public static readonly string DockerContainerName = "DbContextValidation.Tests.Oracle";

        public static string DockerArguments(Func<string, string> sqlDirectory)
        {
            return string.Join(" ",
                "--publish 1521/tcp",
                "--detach",
                "wnameless/oracle-xe-11g:18.04");
        }

        public static string[] SqlScripts(Func<string, string> sqlDirectory)
        {
            var directory = sqlDirectory("SQL.Oracle");
            return new []
            {
                File.ReadAllText(Path.Combine(directory, "1. Drop tOrders.sql")),
                File.ReadAllText(Path.Combine(directory, "2. Drop tCustomers.sql")),
                File.ReadAllText(Path.Combine(directory, "3. Create tCustomers.sql")),
                File.ReadAllText(Path.Combine(directory, "4. Create tOrders.sql")),
            };
        }
    }
}