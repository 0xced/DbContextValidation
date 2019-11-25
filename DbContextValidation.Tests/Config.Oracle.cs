using System;
using System.Collections.Generic;
using Xunit.Fixture.Docker;

namespace DbContextValidation.Tests.Oracle
{
    public class Configuration : ConfigurationBase, IDockerContainerConfiguration, IDatabaseConfiguration
    {
        public const string Schema = null;

        private const string Sid = "xe";
        private const string User = "system";
        private const string Password = "oracle";

        public string ConnectionString(string host, ushort port) => $"User Id={User};Password={Password};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST={host})(PORT={port}))(CONNECT_DATA=(SID={Sid})))";

        public System.Data.Common.DbProviderFactory ProviderFactory => global::Oracle.ManagedDataAccess.Client.OracleClientFactory.Instance;

        public override TimeSpan Timeout => TimeSpan.FromSeconds(45);

        public string ImageName => "wnameless/oracle-xe-11g-r2";

        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.Oracle"));
    }
}