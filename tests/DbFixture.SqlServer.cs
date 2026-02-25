using System.Data.Common;
using Testcontainers.MsSql;
using Xunit.Abstractions;
#if EF6
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

namespace DbContextValidation.Tests
{
    public class DbFixture : DbFixture<MsSqlBuilder, MsSqlContainer>
    {
        public DbFixture(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected override MsSqlBuilder CreateBuilder() => new MsSqlBuilder("mcr.microsoft.com/mssql/server:2019-latest");

        public override DbProviderFactory DbProviderFactory => SqlClientFactory.Instance;

        protected override string SqlDirectoryName => "SQL.SqlServer";

        public override string Schema => "dbo";
    }
}