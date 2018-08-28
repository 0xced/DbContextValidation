using Microsoft.EntityFrameworkCore;

namespace DbSchemaValidator.Tests.EFCore.SqlServer
{
    public static class Configuration
    {
        public static readonly string Host = "localhost";
        public static readonly string Port = "1433";
        public static readonly string Database = "tempdb";
        public static readonly string User = "sa";
        public static readonly string Password = "SqlServer-doc4er";

        public static readonly string ConnectionString = $"Server={Host};Database={Database};User Id={User};Password={Password}";
    }

    public abstract class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Configuration.ConnectionString);
        }
    }
}