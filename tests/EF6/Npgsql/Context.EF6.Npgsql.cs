using System.Data.Entity;
using Npgsql;

// ReSharper disable once CheckNamespace
namespace DbContextValidation.Tests.Npgsql
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
        protected Context(string connectionString) : base(connectionString)
        {
        }
    }
}