using System.Linq;
#if NETFRAMEWORK
using ModelBuilder = System.Data.Entity.DbModelBuilder;
#else
using Microsoft.EntityFrameworkCore;
#endif

#if PROVIDER_NPGSQL && NETFRAMEWORK 
using DbSchemaValidator.Tests.EF6.Npgsql;
#elif PROVIDER_NPGSQL
using DbSchemaValidator.Tests.EFCore.Npgsql;
#elif PROVIDER_SQLITE && NETFRAMEWORK
using DbSchemaValidator.Tests.EF6.SQLite;
#elif PROVIDER_SQLITE 
using DbSchemaValidator.Tests.EFCore.SQLite;
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
    
    public class ContextWithMisspelledCustomersTable : ValidContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Customer>().ToTable("Customers");
        }
    }

    public class ContextWithMisspelledOrderDateColumn : ValidContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasColumnName("OrderFate");
        }
    }
    
    public class ContextWithMixedCaseColumns : ValidContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasColumnName("oRdErDaTe");
            modelBuilder.Entity<Order>().Property(o => o.CustomerId).HasColumnName("cUsToMeRiD");
        }
    }
}