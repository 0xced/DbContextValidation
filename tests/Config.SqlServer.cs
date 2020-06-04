using System;
using System.Collections.Generic;
using Xunit.Fixture.Docker;

namespace DbContextValidation.Tests.SqlServer
{
    public class Configuration : ConfigurationBase, IDockerContainerConfiguration, IDatabaseConfiguration
    {
        public const string Schema = "dbo";

        private const string Database = "tempdb";
        private const string User = "sa";
        private const string Password = "SqlServer-doc4er";

        public string ConnectionString(string host, ushort port) => $"Server={host},{port};Database={Database};User Id={User};Password={Password}";

#if NETCOREAPP3_1
        public System.Data.Common.DbProviderFactory ProviderFactory => Microsoft.Data.SqlClient.SqlClientFactory.Instance;
#else
        public System.Data.Common.DbProviderFactory ProviderFactory => System.Data.SqlClient.SqlClientFactory.Instance;
#endif

        public override TimeSpan Timeout => TimeSpan.FromMinutes(2);

        public string ImageName => "genschsa/mssql-server-linux:latest";

        public override IReadOnlyDictionary<string, string> EnvironmentVariables => new Dictionary<string, string>
        {
            ["ACCEPT_EULA"] = "Y",
            ["MSSQL_SA_PASSWORD"] = Password,
        };

        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.SqlServer"));
    }
}