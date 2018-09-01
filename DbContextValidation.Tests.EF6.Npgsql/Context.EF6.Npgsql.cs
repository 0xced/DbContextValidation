using System.Data.Entity;
using Npgsql;

// ReSharper disable once CheckNamespace
namespace DbContextValidation.Tests
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
        protected Context() : base(Config.ConnectionString)
        {
        }
    }
}