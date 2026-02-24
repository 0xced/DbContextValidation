using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;
using Testcontainers.FirebirdSql;
using Xunit.Abstractions;

namespace DbContextValidation.Tests
{
    public class DbFixture : DbFixture<FirebirdSqlBuilder, FirebirdSqlContainer>
    {
        public DbFixture(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected override FirebirdSqlBuilder CreateBuilder() => new FirebirdSqlBuilder("jacobalberty/firebird:v4.0");

        public override DbProviderFactory DbProviderFactory => FirebirdClientFactory.Instance;

        protected override string SqlDirectoryName => "SQL.Firebird";

        public override string Schema => null;
    }
}