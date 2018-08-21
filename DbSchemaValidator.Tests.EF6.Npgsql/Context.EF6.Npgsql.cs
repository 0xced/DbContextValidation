using System.Data.Entity;
using Npgsql;

namespace DbSchemaValidator.Tests.EF6
{
    public class NpgsqlConfiguration : DbConfiguration
    {
        public NpgsqlConfiguration() 
        {
            SetDefaultConnectionFactory(new NpgsqlConnectionFactory());
            SetProviderFactory("Npgsql", NpgsqlFactory.Instance);
            SetProviderServices("Npgsql", NpgsqlServices.Instance);
        }
    }

    [DbConfigurationType(typeof(NpgsqlConfiguration))] 
    public abstract class Context : DbContext
    {
        protected Context() : base("Host=localhost;Database=DbSchemaValidator")
        {
        }
    }
}