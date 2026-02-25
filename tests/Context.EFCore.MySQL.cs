using Microsoft.EntityFrameworkCore;

namespace DbContextValidation.Tests.MySQL;

public abstract class Context(string connectionString) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseMySQL(connectionString);
}