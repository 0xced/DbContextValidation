using Microsoft.EntityFrameworkCore;

namespace DbSchemaValidator.Tests.EFCore
{
    public abstract class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=../../../../DbSchemaValidator.Tests/DbSchemaValidator.db");
        }
    }
}