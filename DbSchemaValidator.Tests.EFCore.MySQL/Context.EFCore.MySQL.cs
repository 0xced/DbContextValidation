using Microsoft.EntityFrameworkCore;

namespace DbSchemaValidator.Tests.EFCore.MySQL
{
    public abstract class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("Host=localhost;Database=DbSchemaValidator;UserName=root;Password=docker");
        }
    }
}