using System.Collections.Generic;
using DockerRunner.Database.Oracle;
using static DbContextValidation.Tests.SqlInitializationHelper;

namespace DbContextValidation.Tests
{
    public class Configuration : Oracle11SlimConfiguration
    {
        public const string Schema = null;

        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.Oracle"));
    }
}