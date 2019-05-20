using System;
using System.Collections.Generic;
using System.IO;
using Xunit.Fixture.DockerDb;

namespace DbContextValidation.Tests.Oracle
{
    public class Configuration : ConfigurationBase, IDockerDatabaseConfiguration
    {
        public const string Schema = null;

        private const string Sid = "XE";
        private const string User = "system";
        private const string Password = "Oracle18";

        public string ConnectionString(string host, ushort port) => $"User Id={User};Password={Password};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST={host})(PORT={port}))(CONNECT_DATA=(SID={Sid})))";

        public System.Data.Common.DbProviderFactory ProviderFactory => global::Oracle.ManagedDataAccess.Client.OracleClientFactory.Instance;

        public override TimeSpan Timeout => TimeSpan.FromSeconds(45);

        public string ImageName => "quillbuilduser/oracle-18-xe:latest";

        public ushort Port => 1521;

        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.Oracle"));
    }
}