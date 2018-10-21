using System.Data.Entity;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.EntityFramework;

// ReSharper disable once CheckNamespace
namespace DbContextValidation.Tests
{
    public class OracleConfiguration : DbConfiguration
    {
        public OracleConfiguration()
        {
            SetDefaultConnectionFactory(new OracleConnectionFactory());
            SetProviderFactory("Oracle.ManagedDataAccess.Client", OracleClientFactory.Instance);
            SetProviderServices("Oracle.ManagedDataAccess.Client", EFOracleProviderServices.Instance);
        }
    }

    [DbConfigurationType(typeof(OracleConfiguration))] 
    public abstract class Context : DbContext
    {
        protected Context() : base(Config.ConnectionString)
        {
        }
    }
}