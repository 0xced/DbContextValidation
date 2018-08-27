using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

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

    [DbConfigurationType(typeof(SqlServerConfiguration))] 
    public abstract class Context : DbContext
    {
        protected Context() : base("Server=localhost;Database=tempdb;User Id=sa;Password=SqlServer-doc4er")
        {
        }
    }
}