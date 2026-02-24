using System.Data.Common;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace DbContextValidation.Tests
{
    public class DbFixture : DbFixture<PostgreSqlBuilder, PostgreSqlContainer>
    {
        public DbFixture(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected override PostgreSqlBuilder CreateBuilder() => new PostgreSqlBuilder("postgres:18");

        public override DbProviderFactory DbProviderFactory => NpgsqlFactory.Instance;

        protected override string SqlDirectoryName => "SQL.Common";

        public override string Schema => "public";
    }
}