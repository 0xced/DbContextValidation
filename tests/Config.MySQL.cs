using System.Collections.Generic;
using static DbContextValidation.Tests.SqlInitializationHelper;

namespace DbContextValidation.Tests
{
#if PROVIDER_MYSQL_POMELO
    public class Configuration : DockerRunner.Database.MySqlConnector.MySqlServerConfiguration
#else
    public class Configuration : DockerRunner.Database.MySql.MySqlServerConfiguration
#endif
    {
        public const string Schema = null;



        public override IEnumerable<string> SqlStatements => ReadSqlStatements(SqlDirectory("SQL.MySQL"));
    }
}