using System.Collections.Generic;
using DockerRunner.Database.PostgreSql;
using static DbContextValidation.Tests.SqlInitializationHelper;

namespace DbContextValidation.Tests
{
    public class Configuration : PostgresConfiguration
    {
        public const string Schema = "public";

        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.Common"));
    }
}