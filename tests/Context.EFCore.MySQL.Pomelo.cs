using Microsoft.EntityFrameworkCore;

namespace DbContextValidation.Tests.MySQL.Pomelo;

public abstract class Context(string connectionString) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
}