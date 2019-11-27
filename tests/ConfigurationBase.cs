using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Fixture.Docker;

#if PROVIDER_FIREBIRD
namespace DbContextValidation.Tests.Firebird
#elif PROVIDER_MYSQL
namespace DbContextValidation.Tests.MySQL
#elif PROVIDER_MYSQL_POMELO
namespace DbContextValidation.Tests.MySQL.Pomelo
#elif PROVIDER_NPGSQL
namespace DbContextValidation.Tests.Npgsql
#elif PROVIDER_ORACLE
namespace DbContextValidation.Tests.Oracle
#elif PROVIDER_SQLITE
namespace DbContextValidation.Tests.SQLite
#elif PROVIDER_SQLSERVER
namespace DbContextValidation.Tests.SqlServer
#else
#error Make sure to define a PROVIDER_* constant in the tests project
namespace DbContextValidation.Tests
#endif
{
    public class ConfigurationBase
    {
        public virtual TimeSpan Timeout => TimeSpan.FromSeconds(30);

        public virtual ushort? Port => null;

        public virtual IReadOnlyDictionary<string, string> EnvironmentVariables => new Dictionary<string, string>();

        public virtual IReadOnlyDictionary<DirectoryInfo, string> Volumes => new Dictionary<DirectoryInfo, string>();

        public virtual IEnumerable<string> SqlStatements => Enumerable.Empty<string>();

        protected static IEnumerable<string> ReadSqlStatements(DirectoryInfo directory)
        {
            return directory.GetFiles("*.sql").OrderBy(file => file.Name).Select(file => File.ReadAllText(file.FullName));
        }

        protected static DirectoryInfo SqlDirectory(string directoryName)
        {
            var testsDirectory = TestsDirectory();
            var sqlDirectory = new DirectoryInfo(Path.Combine(testsDirectory.FullName, directoryName));
            if (!sqlDirectory.Exists)
            {
                throw new FileNotFoundException($"SQL directory not found ({sqlDirectory.FullName})", sqlDirectory.FullName);
            }
            return sqlDirectory;
        }

        protected static FileInfo SqlFile(string fileName)
        {
            var testsDirectory = TestsDirectory();
            var sqlFile = new FileInfo(Path.Combine(testsDirectory.FullName, fileName));
            if (!sqlFile.Exists)
            {
                throw new FileNotFoundException($"SQL file not found ({sqlFile.FullName})", sqlFile.FullName);
            }
            return sqlFile;
        }

        private static DirectoryInfo TestsDirectory()
        {
            DirectoryInfo testsDirectoryInfo;
            var testsDirectory = Environment.GetEnvironmentVariable("TESTS_DIRECTORY");
            if (testsDirectory != null)
            {
                testsDirectoryInfo = new DirectoryInfo(testsDirectory);
                if (!testsDirectoryInfo.Exists)
                    throw new FileNotFoundException($"Tests directory not found ({testsDirectoryInfo.FullName})", testsDirectoryInfo.FullName);
            }
            else
            {
                var assemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
                var targetFrameworkDirectory = assemblyDirectory?.Name == "publish" ? assemblyDirectory.Parent?.Parent : assemblyDirectory;
                testsDirectoryInfo = targetFrameworkDirectory?.Parent?.Parent?.Parent?.Parent?.Parent ?? throw new FileNotFoundException("Tests directory not found");
            }
            return testsDirectoryInfo;
        }
    }
}