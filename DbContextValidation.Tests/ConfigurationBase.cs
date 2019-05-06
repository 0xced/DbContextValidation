using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Fixture.DockerDb;

namespace DbContextValidation.Tests
{
    public class ConfigurationBase
    {
        public virtual TimeSpan Timeout => TimeSpan.FromSeconds(30);

        private readonly Lazy<string> _containerName = new Lazy<string>(() => Assembly.GetExecutingAssembly().ExportedTypes.Single(e => e.GetInterfaces().FirstOrDefault() == typeof(IClassFixture<DockerDatabaseFixture>)).Namespace);
        public virtual string ContainerName => _containerName.Value;

        public virtual string[] SqlScripts => new string[0];

        protected static string SqlDirectory(string directoryName)
        {
            var testsDirectory = Environment.GetEnvironmentVariable("TESTS_DIRECTORY");
            if (testsDirectory == null)
            {
                var assemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
                var targetFrameworkDirectory = assemblyDirectory?.Name == "publish" ? assemblyDirectory.Parent?.Parent : assemblyDirectory; 
                var solutionDirectory = targetFrameworkDirectory?.Parent?.Parent?.Parent?.Parent ?? throw new FileNotFoundException("Solution directory not found");
                testsDirectory = Path.Combine(solutionDirectory.FullName, "DbContextValidation.Tests");
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