using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;
using System.Data.SQLite.EF6;

namespace DbSchemaValidator.Tests.EF6
{
    public class SQLiteConnectionFactory : IDbConnectionFactory
    {
        public DbConnection CreateConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }
    }

    public class SQLiteConfiguration : DbConfiguration
    {
        // Adapted from https://stackoverflow.com/questions/20460357/problems-using-entity-framework-6-and-sqlite/23237737#23237737 and https://stackoverflow.com/questions/20460357/problems-using-entity-framework-6-and-sqlite/24935665#24935665
        public SQLiteConfiguration() 
        {
            SetDefaultConnectionFactory(new SQLiteConnectionFactory());
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetProviderServices("System.Data.SQLite", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
            SetProviderServices("System.Data.SQLite.EF6", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
        } 
    }

    [DbConfigurationType(typeof(SQLiteConfiguration))] 
    public abstract class Context : DbContext
    {
        protected Context() : base("Data Source=../../../../DbSchemaValidator.Tests/DbSchemaValidator.db")
        {
            Database.Log = Console.WriteLine;
        }
    }
}