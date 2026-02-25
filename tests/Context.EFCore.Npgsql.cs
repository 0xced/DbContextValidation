using Microsoft.EntityFrameworkCore;

namespace DbContextValidation.Tests.Npgsql;

public abstract class Context(string connectionString) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseNpgsql(connectionString);
}