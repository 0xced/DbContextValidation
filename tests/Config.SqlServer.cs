using System.Collections.Generic;
using DockerRunner.Database.SqlServer;
using static DbContextValidation.Tests.SqlInitializationHelper;

namespace DbContextValidation.Tests
{
    public class Configuration : SqlServer2019Configuration
    {
        public const string Schema = "dbo";

        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.SqlServer"));
    }
}