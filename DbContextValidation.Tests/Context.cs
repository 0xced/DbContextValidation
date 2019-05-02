#if NETFRAMEWORK
using System.Data.Entity;
using ModelBuilder = System.Data.Entity.DbModelBuilder;
#else
using Microsoft.EntityFrameworkCore;
#endif

namespace DbContextValidation.Tests
{
    public class ValidContext : Context
    {
        static ValidContext()
        {
#if NETFRAMEWORK
            // Disable migrations
            Database.SetInitializer<ValidContext>(null);
            Database.SetInitializer<ValidContextWithExplicitSchema>(null);
            Database.SetInitializer<ContextWithUnknownSchema>(null);
            Database.SetInitializer<ContextWithMisspelledCustomersTable>(null);
            Database.SetInitializer<ContextWithMisspelledOrderDateColumn>(null);
            Database.SetInitializer<ContextWithMixedCaseColumns>(null);
#endif
        }

        public ValidContext(string connectionString) : base(connectionString)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
#if NETFRAMEWORK
            /*
             * EF Core is better at choosing the most appropriate default schema, see https://docs.microsoft.com/en-us/ef/core/modeling/relational/default-schema
             * > By convention, the database provider will choose the most appropriate default schema. For example, Microsoft SQL Server will use the dbo schema and SQLite will not use a schema (since schemas are not supported in SQLite).
             * But EF 6 needs a bit of help to choose an appropriate default schema.
             */
            var isSqlServer = Database.Connection.GetType().FullName == "System.Data.SqlClient.SqlConnection";
            if (!isSqlServer) // setting "" as default schema for SqlServer somehow turns it into "CodeFirstDatabase"
            {
                modelBuilder.HasDefaultSchema("");
            }
#endif
            modelBuilder.Entity<Customer>().ToTable("tCustomers");
            modelBuilder.Entity<Order>().ToTable("tOrders");
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
    }

    public class ValidContextWithExplicitSchema : ValidContext
    {
        private readonly string _schema;

        public ValidContextWithExplicitSchema(string connectionString, string schema) : base(connectionString)
        {
            _schema = schema;
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            if (_schema != null) // Some databases don't support schemata at all (e.g. SQLite, MySQL)
            {
                modelBuilder.HasDefaultSchema(_schema);
            }
        }
    }
    
    public class ContextWithUnknownSchema : ValidContext
    {
        public ContextWithUnknownSchema(string connectionString) : base(connectionString)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("unknown");
        }
    }
    
    public class ContextWithMisspelledCustomersTable : ValidContext
    {
        public ContextWithMisspelledCustomersTable(string connectionString) : base(connectionString)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Customer>().ToTable("Customers");
        }
    }

    public class ContextWithMisspelledOrderDateColumn : ValidContext
    {
        public ContextWithMisspelledOrderDateColumn(string connectionString) : base(connectionString)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasColumnName("OrderFate");
        }
    }
    
    public class ContextWithMixedCaseColumns : ValidContext
    {
        public ContextWithMixedCaseColumns(string connectionString) : base(connectionString)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasColumnName("oRdErDaTe");
            modelBuilder.Entity<Order>().Property(o => o.CustomerId).HasColumnName("cUsToMeRiD");
        }
    }
}