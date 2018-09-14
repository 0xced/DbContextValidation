using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DbContextValidation.Tests
{
    public class DatabaseFixture : IDisposable
    {
        private readonly IMessageSink _sink;

        public DatabaseFixture(IMessageSink sink)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            
            if (Config.DockerContainerName == null)
                return;

            DockerContainerStart();
            WaitForDatabase(TimeSpan.FromSeconds(20));
        }

        public void Dispose()
        {
            if (Config.DockerContainerName == null)
                return;

            RunDocker("stop " + Config.DockerContainerName, waitForExit: false);
        }
        
        private void DockerContainerStart()
        {
            try
            {
                RunDocker("start " + Config.DockerContainerName);
            }
            catch (Exception e) when (e.Message.Contains("No such container"))
            {
                RunDocker($"run --name {Config.DockerContainerName} " + Config.DockerArguments(SqlDirectory));
            }
            var portLine = RunDocker($"port {Config.DockerContainerName}").TrimEnd('\n');
            var port = Regex.Match(portLine, @"-> 0\.0\.0\.0:(?<port>\d+)").Groups["port"];
            if (!port.Success)
                throw new ApplicationException($"Could not find port in '{portLine}'");
            Config.Port = ushort.Parse(port.Value);
        }

        private string RunDocker(string arguments, bool waitForExit = true)
        {
            var startInfo = new ProcessStartInfo("docker", arguments) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            using (var docker = Process.Start(startInfo))
            {
                if (docker == null)
                    throw new ApplicationException($"Failed to run `docker {arguments}`");
                WriteDiagnostic($"> docker {arguments}");
                if (waitForExit)
                {
                    docker.WaitForExit();
                    WriteDiagnostic($"({docker.ExitCode}) docker {docker.StartInfo.Arguments}");
                    if (docker.ExitCode != 0)
                    {
                        var error = docker.StandardError.ReadToEnd();
                        throw new ApplicationException(error);
                    }
                    return docker.StandardOutput.ReadToEnd();
                }
                return null;
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
            using (var context = new ValidContext())
            {
#if NETFRAMEWORK
                var connection = context.Database.Connection;
#else
                var connection = Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.GetDbConnection(context.Database);
#endif
                var provider = Config.DockerContainerName.Split('.').Last();
                WriteDiagnostic($"Waiting for {provider} database to be available on {connection.ConnectionString}");
                while (true)
                {
                    try
                    {
                        connection.Open();
                        WriteDiagnostic($"It took {stopWatch.Elapsed.TotalSeconds:F1} seconds for the {provider} database to become available.");
                        break;
                    }
                    catch (Exception exception)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        if (stopWatch.Elapsed > timeout)
                        {
                            throw new TimeoutException($"{provider} database was not available after waiting for {timeout.TotalSeconds:F1} seconds.", exception);
                        }
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