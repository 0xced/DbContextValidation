using Microsoft.EntityFrameworkCore;

namespace DbSchemaValidator.Tests.EFCore.SqlServer
{
    public abstract class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=tempdb;User Id=sa;Password=SqlServer-doc4er");
        }
    }
}