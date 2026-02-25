using Microsoft.EntityFrameworkCore;

namespace DbContextValidation.Tests.Oracle;

public abstract class Context(string connectionString) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseOracle(connectionString);
}