using System.Diagnostics.CodeAnalysis;
#if EFCORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
using ModelBuilder = System.Data.Entity.DbModelBuilder;
#endif

#if PROVIDER_FIREBIRD
namespace DbContextValidation.Tests.Firebird;
#elif PROVIDER_MYSQL
namespace DbContextValidation.Tests.MySQL;
#elif PROVIDER_MYSQL_POMELO
namespace DbContextValidation.Tests.MySQL.Pomelo;
#elif PROVIDER_NPGSQL
namespace DbContextValidation.Tests.Npgsql;
#elif PROVIDER_ORACLE
namespace DbContextValidation.Tests.Oracle;
#elif PROVIDER_SQLITE
namespace DbContextValidation.Tests.SQLite;
#elif PROVIDER_SQLSERVER
namespace DbContextValidation.Tests.SqlServer;
#else
#error Make sure to define a PROVIDER_* constant in the tests project
namespace DbContextValidation.Tests;
#endif

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global", Justification = "Required for EF Core")]
public class ValidContext(string connectionString) : Context(connectionString)
{
    static ValidContext()
    {
#if !EFCORE
        // Disable migrations
        Database.SetInitializer<ValidContext>(null);
        Database.SetInitializer<ValidContextWithExplicitSchema>(null);
        Database.SetInitializer<ContextWithUnknownSchema>(null);
        Database.SetInitializer<ContextWithMisspelledCustomersTable>(null);
        Database.SetInitializer<ContextWithMisspelledOrderDateColumn>(null);
        Database.SetInitializer<ContextWithMixedCaseColumns>(null);
#endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
#if !EFCORE
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
        string? nullTable = null;
        modelBuilder.Entity<Product>().ToTable(nullTable);
    }

    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
}

public class ValidContextWithExplicitSchema(string connectionString, string? schema) : ValidContext(connectionString)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        if (schema != null) // Some databases don't support schemata at all (e.g., SQLite, MySQL)
        {
            modelBuilder.HasDefaultSchema(schema);
        }
    }
}

public class ContextWithUnknownSchema(string connectionString) : ValidContext(connectionString)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("unknown");
    }
}

public class ContextWithMisspelledCustomersTable(string connectionString) : ValidContext(connectionString)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Customer>().ToTable("Customers");
    }
}

public class ContextWithMisspelledOrderDateColumn(string connectionString) : ValidContext(connectionString)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasColumnName("OrderFate");
    }
}

public class ContextWithMixedCaseColumns(string connectionString) : ValidContext(connectionString)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasColumnName("oRdErDaTe");
        modelBuilder.Entity<Order>().Property(o => o.CustomerId).HasColumnName("cUsToMeRiD");
    }
}