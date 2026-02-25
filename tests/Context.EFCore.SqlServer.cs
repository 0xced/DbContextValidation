using Microsoft.EntityFrameworkCore;

namespace DbContextValidation.Tests.SqlServer;

public abstract class Context(string connectionString) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(connectionString);
}