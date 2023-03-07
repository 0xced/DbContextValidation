using System.Collections.Generic;
using static DbContextValidation.Tests.SqlInitializationHelper;

namespace DbContextValidation.Tests
{
    public class Configuration : DockerRunner.Database.MySql.MySqlServerConfiguration
    {
        public const string Schema = null;

        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.MySQL"));
    }
}