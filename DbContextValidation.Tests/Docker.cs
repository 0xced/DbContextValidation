using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DbContextValidation.Tests
{
    public class DockerFixture : IDisposable
    {
        private readonly IMessageSink _sink;

        public DockerFixture(IMessageSink sink)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            
            if (Config.Provider == Provider.SQLite)
                return;

            DockerContainerStart();
            WaitForDatabase(TimeSpan.FromSeconds(20));
        }

        private static string DockerContainerName()
        {
            return "DbContextValidation.Tests." + Config.Provider;
        }

        public void Dispose()
        {
            if (Config.Provider == Provider.SQLite)
                return;

            RunDocker("stop " + DockerContainerName());
        }
        
        private void DockerContainerStart()
        {
            try
            {
                RunDocker("start " + DockerContainerName());
            }
            catch (Exception e) when (e.Message.Contains("No such container"))
            {
                RunDocker($"run --name {DockerContainerName()} " + Config.DockerArguments(SqlDirectory));
            }
        }

        private void RunDocker(string arguments)
        {
            var docker = Process.Start(new ProcessStartInfo("docker", arguments) { UseShellExecute = false, RedirectStandardError = true });
            if (docker == null)
                throw new ApplicationException($"Failed to run `docker {arguments}`");
            WriteDiagnostic($"> docker {arguments}");
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

        private void WaitForDatabase(TimeSpan timeout)
        {
            var stopWatch = Stopwatch.StartNew();
            var connection = Config.CreateDbConnection();
            WriteDiagnostic($"Waiting for {connection} database to be available on {connection.ConnectionString}");
            while (true)
            {
                try
                {
                    connection.Open();
                    WriteDiagnostic($"It took {stopWatch.Elapsed.TotalSeconds:F1} seconds for the database to become available.");
                    break;
                }
                catch (Exception exception)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    if (stopWatch.Elapsed > timeout)
                    {
                        throw new TimeoutException($"Database was not available after waiting for {timeout.TotalSeconds:F1} seconds.", exception);
                    }
                }
            }
        }

        private void WriteDiagnostic(string message)
        {
            _sink.OnMessage(new DiagnosticMessage(message));
        }
    }
}