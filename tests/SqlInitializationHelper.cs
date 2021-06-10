using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DbContextValidation.Tests
{
    public static class SqlInitializationHelper
    {
        public static IEnumerable<string> ReadSqlStatements(DirectoryInfo directory)
        {
            return directory.GetFiles("*.sql").OrderBy(file => file.Name).Select(file => File.ReadAllText(file.FullName));
        }

        public static DirectoryInfo SqlDirectory(string directoryName)
        {
            var testsDirectory = TestsDirectory();
            var sqlDirectory = new DirectoryInfo(Path.Combine(testsDirectory.FullName, directoryName));
            if (!sqlDirectory.Exists)
            {
                throw new FileNotFoundException($"SQL directory not found ({sqlDirectory.FullName})", sqlDirectory.FullName);
            }
            return sqlDirectory;
        }

        public static FileInfo SqlFile(string fileName)
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
            var testsDirectory = Environment.GetEnvironmentVariable("TESTS_DIRECTORY");
            if (testsDirectory != null)
            {
                var testsDirectoryInfo = new DirectoryInfo(testsDirectory);
                if (!testsDirectoryInfo.Exists)
                    throw new FileNotFoundException($"Tests directory specified in the TESTS_DIRECTORY environment variable not found ({testsDirectoryInfo.FullName})", testsDirectoryInfo.FullName);
                return testsDirectoryInfo;
            }

            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = new FileInfo(assemblyLocation).Directory;
            for (var directory = assemblyDirectory; directory != null; directory = directory.Parent)
            {
                if (directory.Name == "tests")
                    return directory;
            }
            throw new FileNotFoundException($"Tests directory not found by going up '{assemblyLocation}' and searching for a directory named 'tests'.");
        }
    }
}