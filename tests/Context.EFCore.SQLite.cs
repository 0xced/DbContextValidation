using Microsoft.EntityFrameworkCore;

namespace DbContextValidation.Tests.SQLite;

public abstract class Context(string connectionString) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(connectionString);
}