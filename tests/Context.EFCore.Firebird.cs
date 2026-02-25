using Microsoft.EntityFrameworkCore;

namespace DbContextValidation.Tests.Firebird;

public abstract class Context(string connectionString) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseFirebird(connectionString);
}