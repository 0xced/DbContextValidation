using System;
using System.Collections.Generic;
using System.Data.Common;
using DockerRunner.Database.Oracle;
using Oracle.ManagedDataAccess.Client;
using static DbContextValidation.Tests.SqlInitializationHelper;

namespace DbContextValidation.Tests
{
    public class Configuration : Oracle11SlimConfiguration
    {
        public const string Schema = null;

        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.Oracle"));

        public override DbProviderFactory ProviderFactory => new OracleProviderFactory(base.ProviderFactory);

        private class OracleProviderFactory : DbProviderFactory
        {
            private readonly DbProviderFactory _factory;

            public OracleProviderFactory(DbProviderFactory factory) => _factory = factory;

            public override DbConnection CreateConnection()
            {
                var connection = (OracleConnection)_factory.CreateConnection() ?? throw new InvalidOperationException($"CreateConnection() returned null for {_factory}");
                // Fixes "ORA-01882: timezone region not found" that occurs on GitHub actions
                // See https://stackoverflow.com/questions/47469074/timezone-region-not-found/74291427#74291427
#if !NETCOREAPP2_1
                connection.UseHourOffsetForUnsupportedTimezone = true;
#endif
                return connection;
            }

            public override DbCommand CreateCommand() => _factory.CreateCommand();
            public override DbCommandBuilder CreateCommandBuilder() => _factory.CreateCommandBuilder();
            public override DbConnectionStringBuilder CreateConnectionStringBuilder() => _factory.CreateConnectionStringBuilder();
            public override DbDataAdapter CreateDataAdapter() => _factory.CreateDataAdapter();
            public override DbDataSourceEnumerator CreateDataSourceEnumerator() => _factory.CreateDataSourceEnumerator();
            public override DbParameter CreateParameter() => _factory.CreateParameter();
            public override bool CanCreateDataSourceEnumerator => _factory.CanCreateDataSourceEnumerator;

#if NETCOREAPP3_0_OR_GREATER
            public override bool CanCreateCommandBuilder => _factory.CanCreateCommandBuilder;
            public override bool CanCreateDataAdapter => _factory.CanCreateDataAdapter;
#endif

#if NET6_0_OR_GREATER
            public override DbBatch CreateBatch() => _factory.CreateBatch();
            public override DbBatchCommand CreateBatchCommand() => _factory.CreateBatchCommand();
            public override bool CanCreateBatch => _factory.CanCreateBatch;
#endif
        }
    }
}