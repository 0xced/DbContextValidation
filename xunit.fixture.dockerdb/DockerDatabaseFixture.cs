using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Fixture.DockerDb
{
    public class DockerDatabaseFixture<T> : IDisposable where T : IDockerDatabaseConfiguration, new()
    {
        private readonly IMessageSink _sink;
        private readonly IDockerDatabaseConfiguration _configuration;
        private DateTime _dockerStartDateTime;

        public DockerDatabaseFixture(IMessageSink sink)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _configuration = new T();

            if (_configuration.ContainerName == null)
            {
                ConnectionString = _configuration.ConnectionString(null, 0);
                return;
            }

            ConnectionString = DockerContainerStart();
            WaitForDatabase();
        }

        public string ConnectionString { get; }

        public void Dispose()
        {
            if (_configuration.ContainerName == null)
                return;

            RunDocker($"stop \"{_configuration.ContainerName}\"", waitForExit: false);
        }

        // See https://github.com/docker/compose/blob/1.24.0/compose/config/types.py#L127-L136
        private static string NormalizedPath(DirectoryInfo directory)
        {
            var segments = new Uri(directory.FullName).Segments;
            // The drive must be lowercase, see https://docs.docker.com/toolbox/toolbox_install_windows/#optional-add-shared-directories
            if (segments[1].EndsWith(":/"))
                segments[1] = segments[1].Replace(":", "").ToLower();
            return string.Join("", segments);
        }

        private string GetDockerRunArguments()
        {
            var volumes = _configuration.Volumes.Select(e => (hostDirectory: NormalizedPath(e.Key), containerDirectory: e.Value)).ToList();
            if (volumes.Any(e => e.hostDirectory.Contains(":") || e.containerDirectory.Contains(":")))
                throw new InvalidOperationException($"The '{nameof(_configuration.Volumes)}' mapping must not contain paths with the colon (:) character.");
            
            var environmentVariablesArguments = _configuration.EnvironmentVariables.Select(e => $"--env \"{e.Key}\"=\"{e.Value}\"");
            var volumesArguments = volumes.Select(e => $"--volume \"{e.hostDirectory}:{e.containerDirectory}\"");
            var arguments = environmentVariablesArguments.Concat(volumesArguments)
                .Concat(new []
                {
                    $"--name \"{_configuration.ContainerName}\"",
                    $"--publish {_configuration.Port}/tcp",
                    "--detach",
                    $"\"{_configuration.ImageName}\"",
                });
            return string.Join(" ", arguments);
        }

        private string DockerContainerStart()
        {
            try
            {
                _dockerStartDateTime = DateTime.Now;
                RunDocker($"start \"{_configuration.ContainerName}\"");
            }
            catch (Exception e) when (e.Message.Contains("No such container"))
            {
                var arguments = GetDockerRunArguments();
                RunDocker("run " + arguments);
            }

            var host = DockerGetHost();
            var port = DockerContainerGetPort();
            return _configuration.ConnectionString(host, port);
        }

        private string DockerGetHost()
        {
            try
            {
                return RunProcess("docker-machine", "ip").output;
            }
            catch (Exception)
            {
                return "localhost";
            }
        }

        private ushort DockerContainerGetPort()
        {
            var portLine = RunDocker($"port \"{_configuration.ContainerName}\"").output;
            var port = Regex.Match(portLine, @"-> 0\.0\.0\.0:(?<port>\d+)").Groups["port"];
            if (!port.Success)
            {
                string logs;
                try
                {
                    var (output, error) = RunDocker($"logs --since {_dockerStartDateTime:O} \"{_configuration.ContainerName}\"", trimResult: false);
                    logs = !string.IsNullOrWhiteSpace(error) ? error : output;
                }
                catch
                {
                    logs = "";
                }
                var message = string.IsNullOrWhiteSpace(logs) ? "Please check its logs." : "Here are its logs: " + Environment.NewLine + logs;
                throw new ApplicationException($"The '{_configuration.ContainerName}' container failed to start properly. {message}");
            }
            return ushort.Parse(port.Value);
        }

        private (string output, string error) RunDocker(string arguments, bool waitForExit = true, bool trimResult = true)
        {
            return RunProcess("docker", arguments, waitForExit, trimResult);
        }

        private (string output, string error) RunProcess(string command, string arguments, bool waitForExit = true, bool trimResult = true)
        {
            var startInfo = new ProcessStartInfo(command, arguments) { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            using (var process = new Process {StartInfo = startInfo})
            {
                WriteDiagnostic($"> {command} {arguments}");
                try
                {
                    process.Start();
                }
                catch (Exception exception)
                {
                    throw new ApplicationException($"Failed to run `{command} {arguments}` Is {command} installed?", exception);
                }
                if (waitForExit)
                {
                    process.WaitForExit();
                    WriteDiagnostic($"({process.ExitCode}) {command} {process.StartInfo.Arguments}");
                    var error = process.StandardError.ReadToEnd();
                    if (process.ExitCode != 0)
                    {
                        throw new ApplicationException(error);
                    }
                    var output = process.StandardOutput.ReadToEnd();
                    return trimResult ? (output.TrimEnd('\n'), error.TrimEnd('\n')) : (output, error);
                }
                return (null, null);
            }
        }

        private void WaitForDatabase()
        {
            var stopWatch = Stopwatch.StartNew();
            using (var connection = _configuration.ProviderFactory.CreateConnection() ?? throw new InvalidOperationException($"Failed to create a connection with {_configuration.ProviderFactory}."))
            {
                connection.ConnectionString = ConnectionString;
                WriteDiagnostic($"Waiting for database to be available on {connection.ConnectionString}");
                while (true)
                {
                    try
                    {
                        if (connection.State != ConnectionState.Open)
                        {
                            connection.Open();
                        }
                        WriteDiagnostic($"It took {stopWatch.Elapsed.TotalSeconds:F1} seconds for the database to become available.");
                        break;
                    }
                    catch (Exception exception)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        if (stopWatch.Elapsed > _configuration.Timeout)
                        {
                            throw new TimeoutException($"Database was not available on \"{connection.ConnectionString}\" after waiting for {_configuration.Timeout.TotalSeconds:F1} seconds.", exception);
                        }
                    }
                }
                RunScripts(connection);
            }
        }

        private void RunScripts(IDbConnection connection)
        {
            foreach (var statement in _configuration.SqlStatements)
            {
                WriteDiagnostic($"Executing SQL statement{Environment.NewLine}{statement}");
                var command = connection.CreateCommand();
                command.CommandText = statement;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        }

        private void WriteDiagnostic(string message)
        {
            _sink.OnMessage(new DiagnosticMessage(message));
        }
    }
}