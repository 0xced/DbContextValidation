using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Testcontainers.MsSql;
using Xunit.Abstractions;
#if EF6
#pragma warning disable 618 // EntityFramework (v6) depends on System.Data.SqlClient
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

namespace DbContextValidation.Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by xUnit")]
public class DbFixture : DbFixture<MsSqlBuilder, MsSqlContainer>
{
    public DbFixture(IMessageSink messageSink) : base(messageSink)
    {
    }

    protected override MsSqlBuilder CreateBuilder() => new("mcr.microsoft.com/mssql/server:2019-latest");

    public override DbProviderFactory DbProviderFactory => SqlClientFactory.Instance;

    protected override string SqlDirectoryName => "SQL.SqlServer";

    public override string Schema => "dbo";
}