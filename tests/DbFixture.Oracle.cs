using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Oracle.ManagedDataAccess.Client;
using Testcontainers.Oracle;
using Xunit.Abstractions;

namespace DbContextValidation.Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by xUnit")]
public class DbFixture : DbFixture<OracleBuilder, OracleContainer>
{
    public DbFixture(IMessageSink messageSink) : base(messageSink)
    {
    }

    protected override OracleBuilder CreateBuilder() => new("gvenzl/oracle-free:23-slim-faststart");

    public override DbProviderFactory DbProviderFactory => new OracleProviderFactory(OracleClientFactory.Instance);

    protected override string SqlDirectoryName => "SQL.Oracle";

    public override string? Schema => null;

    private class OracleProviderFactory : DbProviderFactory
    {
        private readonly DbProviderFactory _factory;

        public OracleProviderFactory(DbProviderFactory factory) => _factory = factory;

        public override DbConnection CreateConnection()
        {
            var connection = _factory.CreateConnection() as OracleConnection ?? throw new InvalidOperationException($"CreateConnection() did not return an OracleConnection for {_factory}");
            // Fixes "ORA-01882: timezone region not found" that occurs on GitHub actions
            // See https://stackoverflow.com/questions/47469074/timezone-region-not-found/74291427#74291427
            connection.UseHourOffsetForUnsupportedTimezone = true;
            return connection;
        }

        public override DbCommand? CreateCommand() => _factory.CreateCommand();
        public override DbCommandBuilder? CreateCommandBuilder() => _factory.CreateCommandBuilder();
        public override DbConnectionStringBuilder? CreateConnectionStringBuilder() => _factory.CreateConnectionStringBuilder();
        public override DbDataAdapter? CreateDataAdapter() => _factory.CreateDataAdapter();
        public override DbDataSourceEnumerator? CreateDataSourceEnumerator() => _factory.CreateDataSourceEnumerator();
        public override DbParameter? CreateParameter() => _factory.CreateParameter();
        public override bool CanCreateDataSourceEnumerator => _factory.CanCreateDataSourceEnumerator;

#if NETCOREAPP
            public override bool CanCreateCommandBuilder => _factory.CanCreateCommandBuilder;
            public override bool CanCreateDataAdapter => _factory.CanCreateDataAdapter;
            public override DbBatch CreateBatch() => _factory.CreateBatch();
            public override DbBatchCommand CreateBatchCommand() => _factory.CreateBatchCommand();
            public override bool CanCreateBatch => _factory.CanCreateBatch;
            public override DbDataSource CreateDataSource(string connectionString) => _factory.CreateDataSource(connectionString);
#endif
    }
}