using System.Data.Common;
using Testcontainers.MySql;
using Xunit.Abstractions;

namespace DbContextValidation.Tests
{
    public class DbFixture : DbFixture<MySqlBuilder, MySqlContainer>
    {
        public DbFixture(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected override MySqlBuilder CreateBuilder() => new MySqlBuilder("mysql:9");

#if PROVIDER_MYSQL
        public override DbProviderFactory DbProviderFactory => MySql.Data.MySqlClient.MySqlClientFactory.Instance;
#else
        public override DbProviderFactory DbProviderFactory => MySqlConnector.MySqlConnectorFactory.Instance;
#endif

        protected override string SqlDirectoryName => "SQL.MySQL";

        public override string Schema => null;
    }
}