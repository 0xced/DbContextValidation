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
#if NETFRAMEWORK
            /*
             * EF Core is better at choosing the most appropriate default schema, see https://docs.microsoft.com/en-us/ef/core/modeling/relational/default-schema
             * > By convention, the database provider will choose the most appropriate default schema. For example, Microsoft SQL Server will use the dbo schema and SQLite will not use a schema (since schemas are not supported in SQLite).
             * But EF 6 needs a bit of help to choose an appropriate default schema.
             */
            modelBuilder.HasDefaultSchema("");
#endif
            modelBuilder.Entity<Customer>().ToTable("tCustomers");
            modelBuilder.Entity<Order>().ToTable("tOrders");
        }
    }

    public class ContextWithPublicSchema : ValidContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("public");
        }
    }
    
    public class ContextWithUnknownSchema : ValidContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("unknown");
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