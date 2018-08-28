using Microsoft.EntityFrameworkCore;

namespace DbSchemaValidator.Tests.EFCore.SQLite
{
    public static class Configuration
    {
        public static readonly string Host = null;
        public static readonly string Port = null;
        public static readonly string Database = "../../../../DbSchemaValidator.Tests/DbSchemaValidator.db";
        public static readonly string User = null;
        public static readonly string Password = null;

        public static readonly string ConnectionString = $"Data Source={Database}";
    }

    public abstract class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(Configuration.ConnectionString);
        }
    }
}