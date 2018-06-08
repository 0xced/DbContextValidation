#if NETFRAMEWORK
using DbSchemaValidator.Tests.EF6;
using ModelBuilder = System.Data.Entity.DbModelBuilder;
#else
using DbSchemaValidator.Tests.EFCore;
using Microsoft.EntityFrameworkCore;
#endif

namespace DbSchemaValidator.Tests
{
    public class ValidContext : Context
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("tCustomers");
            modelBuilder.Entity<Order>().ToTable("tOrders");
        }
    }
    
    public class MisspelledTableContext : ValidContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Customer>().ToTable("Customers");
        }
    }

    public class MisspelledColumnContext : ValidContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasColumnName("OrderFate");
        }
    }
}