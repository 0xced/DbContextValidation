using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using FirebirdSql.Data.FirebirdClient;
using Testcontainers.FirebirdSql;
using Xunit.Abstractions;

namespace DbContextValidation.Tests
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by xUnit")]
    public class DbFixture : DbFixture<FirebirdSqlBuilder, FirebirdSqlContainer>
    {
        public DbFixture(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected override FirebirdSqlBuilder CreateBuilder() => new FirebirdSqlBuilder("jacobalberty/firebird:v4.0");

        public override DbProviderFactory DbProviderFactory => FirebirdClientFactory.Instance;

        protected override string SqlDirectoryName => "SQL.Firebird";

        public override string? Schema => null;
    }
}