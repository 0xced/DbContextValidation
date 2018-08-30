using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace DbSchemaValidator.Tests
{
    public abstract class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Config.ConnectionString);
        }
    }
}