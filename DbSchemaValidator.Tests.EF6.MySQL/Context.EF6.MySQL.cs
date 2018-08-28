using System.Data.Entity;
using MySql.Data.EntityFramework;
using static DbSchemaValidator.Tests.EF6.MySQL.Configuration;

namespace DbSchemaValidator.Tests.EF6.MySQL
{
    public static class Configuration
    {
        public static readonly string Host = "localhost";
        public static readonly string Port = "3306";
        public static readonly string Database = "DbSchemaValidator";
        public static readonly string User = "root";
        public static readonly string Password = "docker";

        public static readonly string ConnectionString = $"Host={Host};Database={Database};UserName={User};Password={Password}";
    }

    [DbConfigurationType(typeof(MySqlEFConfiguration))] 
    public abstract class Context : DbContext
    {
        protected Context() : base(ConnectionString)
        {
        }
    }
}