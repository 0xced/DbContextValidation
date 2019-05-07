﻿using System.IO;
using Xunit.Fixture.DockerDb;

namespace DbContextValidation.Tests
{
    public class Configuration : ConfigurationBase, IDockerDatabaseConfiguration
    {
        public const string Schema = null;

        private const string Sid = "XE";
        private const string User = "system";
        private const string Password = "Oracle18";

        public string ConnectionString(string host, ushort port) => $"User Id={User};Password={Password};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST={host})(PORT={port}))(CONNECT_DATA=(SID={Sid})))";

        public System.Data.Common.DbProviderFactory ProviderFactory => global::Oracle.ManagedDataAccess.Client.OracleClientFactory.Instance;

        public string[] Arguments => new [] {
            "--publish 1521/tcp",
            "--detach",
            "quillbuilduser/oracle-18-xe:latest",
        };

        public override string[] SqlScripts
        {
            get
            {
                var directory = SqlDirectory("SQL.Oracle");
                return new []
                {
                    File.ReadAllText(Path.Combine(directory, "1. Drop tOrders.sql")),
                    File.ReadAllText(Path.Combine(directory, "2. Drop tCustomers.sql")),
                    File.ReadAllText(Path.Combine(directory, "3. Create tCustomers.sql")),
                    File.ReadAllText(Path.Combine(directory, "4. Create tOrders.sql")),
                };
            }
        }
    }
}