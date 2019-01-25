using FirebirdSql.EntityFrameworkCore.Firebird.Extensions;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace DbContextValidation.Tests
{
    public abstract class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseFirebird(Config.ConnectionString);
        }
    }
}