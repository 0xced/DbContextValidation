using System;
using System.Collections.Generic;
using System.Data.Common;
using DockerRunner.Database;
#if EF6
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using static DbContextValidation.Tests.SqlInitializationHelper;

namespace DbContextValidation.Tests
{
    public class Configuration : DockerDatabaseContainerConfiguration
    {
        public const string Schema = "dbo";

        private const string User = "sa";
        // SQL Server password policy requirements: the password must be at least 8 characters long and contain characters from three of the following four sets: Uppercase letters, Lowercase letters, Base 10 digits, and Symbols.
        private const string Password = "Docker(!)";

        public override string ImageName => "mcr.microsoft.com/mssql/server:2019-latest";

        public override string ConnectionString(string host, ushort port)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = $"{host},{port}",
                UserID = User,
                Password = Password,
                TrustServerCertificate = true,
            };
            return builder.ConnectionString;
        }

        public override DbProviderFactory ProviderFactory => SqlClientFactory.Instance;

        public override IReadOnlyDictionary<string, string> EnvironmentVariables => new Dictionary<string, string>
        {
            ["ACCEPT_EULA"] = "Y",
            ["SA_PASSWORD"] = Password,
        };

        public override TimeSpan Timeout => TimeSpan.FromMinutes(1);

        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.SqlServer"));
    }
}