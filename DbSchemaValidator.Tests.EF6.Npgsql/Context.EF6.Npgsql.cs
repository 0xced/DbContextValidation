using System.Data.Entity;
using Npgsql;
using static DbSchemaValidator.Tests.EF6.Npgsql.Configuration;

namespace DbSchemaValidator.Tests.EF6.Npgsql
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

    public static class Configuration
    {
        public static readonly string Host = "localhost";
        public static readonly string Port = "5432";
        public static readonly string Database = "DbSchemaValidator";
        public static readonly string User = "postgres";
        public static readonly string Password = "docker";

        public static readonly string ConnectionString = $"Host={Host};Database={Database};UserName={User};Password={Password}";
    }

    [DbConfigurationType(typeof(NpgsqlConfiguration))] 
    public abstract class Context : DbContext
    {
        protected Context() : base(ConnectionString)
        {
        }
    }
}