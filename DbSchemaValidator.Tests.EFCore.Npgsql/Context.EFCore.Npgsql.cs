using Microsoft.EntityFrameworkCore;

namespace DbSchemaValidator.Tests.EFCore.Npgsql
{
    public static class Configuration
    {
        public static readonly string Host = "localhost";
        public static readonly string Port = "5432";
        public static readonly string Database = "DbSchemaValidator";
        public static readonly string User = "postgres";
        public static readonly string Password = "docker";

        public static readonly string ConnectionString = $"Host={Host};Database={Database};UserName={User};Password={Password}";
    }
    
    public abstract class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Configuration.ConnectionString);
        }
    }
}