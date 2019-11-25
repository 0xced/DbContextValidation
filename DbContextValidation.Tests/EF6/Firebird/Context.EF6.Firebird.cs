using System.Data.Entity;
using EntityFramework.Firebird;
using FirebirdSql.Data.FirebirdClient;

// ReSharper disable once CheckNamespace
namespace DbContextValidation.Tests.Firebird
{
    public class FirebirdConfiguration : DbConfiguration
    {
        public FirebirdConfiguration()
        {
            SetDefaultConnectionFactory(new FbConnectionFactory());
            SetProviderFactory("FirebirdSql.Data.FirebirdClient", FirebirdClientFactory.Instance);
            SetProviderServices("FirebirdSql.Data.FirebirdClient", FbProviderServices.Instance);
        }
    }

    [DbConfigurationType(typeof(FirebirdConfiguration))] 
    public abstract class Context : DbContext
    {
        protected Context(string connectionString) : base(connectionString)
        {
        }
    }
}