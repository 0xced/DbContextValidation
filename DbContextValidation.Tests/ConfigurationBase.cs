using System;
using System.IO;
using System.Reflection;

namespace DbContextValidation.Tests
{
    public class ConfigurationBase
    {
        public virtual TimeSpan Timeout => TimeSpan.FromSeconds(30);
        
        public virtual string[] SqlScripts => new string[0];
        
        protected static string SqlDirectory(string directoryName)
        {
            var testsDirectory = Environment.GetEnvironmentVariable("TESTS_DIRECTORY");
            if (testsDirectory == null)
            {
                var assemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
                var solutionDirectory = assemblyDirectory?.Parent?.Parent?.Parent?.Parent?.FullName ?? throw new FileNotFoundException("Solution directory not found");
                testsDirectory = Path.Combine(solutionDirectory, "DbContextValidation.Tests");
            }
            var sqlDirectory = Path.Combine(testsDirectory, directoryName);
            if (!Directory.Exists(sqlDirectory))
            {
                throw new FileNotFoundException($"Directory with SQL scripts not found ({sqlDirectory})", sqlDirectory);
            }
            return sqlDirectory;
        }
    }
}