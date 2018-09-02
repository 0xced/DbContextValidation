using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace DbContextValidation.Tests
{
    public class DockerFixture : IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            if (Config.Provider == Provider.SQLite)
                return;

            if (!DockerContainerIsRunning())
            {
                RunDocker($"run --name DbContextValidation.Tests.{Config.Provider} " + Config.DockerArguments(SqlDirectory));
            }
            await WaitForDatabaseAsync(TimeSpan.FromSeconds(20));
        }

        public Task DisposeAsync()
        {
            if (Config.Provider == Provider.SQLite)
                return Task.FromResult(0);

            RunDocker("rm -f DbContextValidation.Tests." + Config.Provider);
            return Task.FromResult(0);
        }

        private static bool DockerContainerIsRunning()
        {
            try
            {
                RunDocker("inspect -f {{.State.Status}} DbContextValidation.Tests." + Config.Provider);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void RunDocker(string arguments)
        {
            var docker = Process.Start(new ProcessStartInfo("docker", arguments) { UseShellExecute = false, RedirectStandardError = true });
            if (docker == null)
                throw new ApplicationException($"Failed to run `docker {arguments}`");
            docker.WaitForExit();
            if (docker.ExitCode != 0)
            {
                var error = docker.StandardError.ReadToEnd();
                throw new ApplicationException(error);
            }
        }

        private static string SqlDirectory(string directoryName)
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

        private static async Task WaitForDatabaseAsync(TimeSpan timeout)
        {
            var stopWatch = Stopwatch.StartNew();
            var connection = Config.CreateDbConnection();
            for (;;)
            {
                try
                {
                    connection.Open();
                    break;
                }
                catch (Exception exception)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    if (stopWatch.Elapsed > timeout)
                    {
                        throw new TimeoutException($"Database was not available after waiting for {timeout.TotalSeconds} seconds.", exception);
                    }
                }
            }
        }
    }
}