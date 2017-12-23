using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace DbSchemaValidator.EFCore.Tests.DB
{
    public abstract class Context : DbContext
    {
        private static readonly LoggerFactory ConsoleLoggerFactory = new LoggerFactory(new[] {new ConsoleLoggerProvider((name, level) => true, true)});

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite("Data Source=../../../DbSchemaValidator.db")
                .UseLoggerFactory(ConsoleLoggerFactory);
        }
        
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
    }
    
    public class ValidContext : Context {}
    
    public class MisspelledTableContext : Context
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Kustomers");
        }
    }

    public class MisspelledColumnContext : Context
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasColumnName("OrderFate");
        }
    }
}