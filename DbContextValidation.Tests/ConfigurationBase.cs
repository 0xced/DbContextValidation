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
            var testsDirectory = TestsDirectory();
            var sqlDirectory = Path.Combine(testsDirectory.FullName, directoryName);
            if (!Directory.Exists(sqlDirectory))
            {
                throw new FileNotFoundException($"SQL directory not found ({sqlDirectory})", sqlDirectory);
            }
            return NormalizePath(sqlDirectory);
        }

        protected static string SqlFile(string fileName)
        {
            var testsDirectory = TestsDirectory();
            var sqlFile = Path.Combine(testsDirectory.FullName, fileName);
            if (!File.Exists(sqlFile))
            {
                throw new FileNotFoundException($"SQL file not found ({sqlFile})", sqlFile);
            }
            return sqlFile;
        }

        // See https://github.com/docker/compose/blob/1.24.0/compose/config/types.py#L127-L136
        private static string NormalizePath(string path)
        {
            try
            {
                return string.Join("", new Uri(path).Segments.Select(e => e.Replace(":", "")));
            }
            catch (UriFormatException)
            {
                // This is a already a unix-style path
                return path;
            }
        }

        private static DirectoryInfo TestsDirectory()
        {
            DirectoryInfo testsDirectoryInfo;
            var testsDirectory = Environment.GetEnvironmentVariable("TESTS_DIRECTORY");
            if (testsDirectory != null)
            {
                testsDirectoryInfo = new DirectoryInfo(testsDirectory);
            }
            else
            {
                var assemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
                var targetFrameworkDirectory = assemblyDirectory?.Name == "publish" ? assemblyDirectory.Parent?.Parent : assemblyDirectory; 
                var solutionDirectory = targetFrameworkDirectory?.Parent?.Parent?.Parent?.Parent ?? throw new FileNotFoundException("Solution directory not found");
                testsDirectoryInfo = new DirectoryInfo(Path.Combine(solutionDirectory.FullName, "DbContextValidation.Tests"));
            }
            if (!testsDirectoryInfo.Exists)
                throw new FileNotFoundException($"Tests directory not found ({testsDirectoryInfo.FullName})", testsDirectoryInfo.FullName);
            return testsDirectoryInfo;
        }
    }
}