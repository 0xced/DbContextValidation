using System;
using System.Data;
using System.Diagnostics;
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
        
        private string DockerContainerStart()
        {
            try
            {
                _dockerStartDateTime = DateTime.Now;
                RunDocker($"start \"{_configuration.ContainerName}\"");
            }
            catch (Exception e) when (e.Message.Contains("No such container"))
            {
                RunDocker($"run --name \"{_configuration.ContainerName}\" " + string.Join(" ", _configuration.Arguments));
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
            var scripts = _configuration.SqlScripts;
            foreach (var script in scripts)
            {
                WriteDiagnostic($"Executing script{Environment.NewLine}{script}");
                var command = connection.CreateCommand();
                command.CommandText = script;
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