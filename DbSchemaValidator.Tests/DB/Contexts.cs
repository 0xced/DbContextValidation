using System;
using System.Data.Entity;

namespace DbSchemaValidator.Tests.DB
{
    public abstract class BIRTContext : DbContext
    {
        protected BIRTContext() : base("name=BIRT")
        {
            Database.Log = Console.WriteLine;
        }
    }

    public class ValidContext : BIRTContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customers").HasKey(c => c.CustomerNumber);
            modelBuilder.Entity<Order>().ToTable("Orders").HasKey(o => o.OrderNumber);
        }
    }

    public class MisspelledTableContext : BIRTContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Kustomers").HasKey(c => c.CustomerNumber);
            modelBuilder.Entity<Order>().ToTable("Orders").HasKey(o => o.OrderNumber);
        }
    }

    public class MisspelledColumnContext : BIRTContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customers").HasKey(c => c.CustomerNumber);
            modelBuilder.Entity<Order>().ToTable("Orders").HasKey(o => o.OrderNumber)
                .Property(o => o.OrderDate).HasColumnName("OrderFate");
        }
    }
}