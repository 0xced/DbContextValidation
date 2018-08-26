using Microsoft.EntityFrameworkCore;

namespace DbSchemaValidator.Tests.EFCore.Npgsql
{
    public abstract class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=DbSchemaValidator;UserName=postgres;Password=docker");
        }
    }
}