using System;
using System.Data.Entity;

namespace DbSchemaValidator.Tests.DB
{
    public abstract class Context : DbContext
    {
        protected Context() : base("name=DB")
        {
            Database.Log = Console.WriteLine;
        }
    }

    public class ValidContext : Context
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
        }
    }

    public class MisspelledTableContext : Context
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Kustomers");
            modelBuilder.Entity<Order>();
        }
    }

    public class MisspelledColumnContext : Context
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasColumnName("OrderFate");
        }
    }
}