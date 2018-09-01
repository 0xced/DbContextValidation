using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

// ReSharper disable once CheckNamespace
namespace DbContextValidation.Tests
{
    public class SqlServerConfiguration : DbConfiguration
    {
        public SqlServerConfiguration() 
        {
            SetDefaultConnectionFactory(new SqlConnectionFactory());
            SetProviderServices("System.Data.SqlClient", SqlProviderServices.Instance);
        }
    }

    [DbConfigurationType(typeof(SqlServerConfiguration))] 
    public abstract class Context : DbContext
    {
        protected Context() : base(Config.ConnectionString)
        {
        }
    }
}