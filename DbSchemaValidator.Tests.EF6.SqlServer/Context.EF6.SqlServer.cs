using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;
using static DbSchemaValidator.Tests.EF6.SqlServer.Configuration;

namespace DbSchemaValidator.Tests.EF6.SqlServer
{
    public class SqlServerConfiguration : DbConfiguration
    {
        public SqlServerConfiguration() 
        {
            SetDefaultConnectionFactory(new SqlConnectionFactory());
            SetProviderServices("System.Data.SqlClient", SqlProviderServices.Instance);
        }
    }

    public static class Configuration
    {
        public static readonly string Host = "localhost";
        public static readonly string Port = "1433";
        public static readonly string Database = "tempdb";
        public static readonly string User = "sa";
        public static readonly string Password = "SqlServer-doc4er";

        public static readonly string ConnectionString = $"Server={Host};Database={Database};User Id={User};Password={Password}";
    }

    [DbConfigurationType(typeof(SqlServerConfiguration))] 
    public abstract class Context : DbContext
    {
        protected Context() : base(ConnectionString)
        {
        }
    }
}