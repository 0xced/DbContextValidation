using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Fixture.DockerDb
{
    public class DockerDatabaseFixture : IDisposable
    {
        private readonly IMessageSink _sink;
        private readonly IDockerDatabaseConfiguration _configuration;

        public DockerDatabaseFixture(IMessageSink sink)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));

            // Ideally this IDockerDatabaseConfiguration should be injected but it's not possible with xUnit (2.4.1)
            _configuration = GetDockerDatabaseConfiguration();

            if (_configuration.ContainerName == null)
            {
                ConnectionString = _configuration.ConnectionString(DockerGetHost(), 0);
                return;
            }

            ConnectionString = DockerContainerStart();
            WaitForDatabase();
        }

        private static IDockerDatabaseConfiguration GetDockerDatabaseConfiguration()
        {
            var executingAssemblyName = Assembly.GetExecutingAssembly().GetName();
            var assembliesUnderTest = AppDomain.CurrentDomain.GetAssemblies().Where(e => e.GetReferencedAssemblies().Any(reference => AssemblyName.ReferenceMatchesDefinition(reference, executingAssemblyName)));
            foreach (var assembly in assembliesUnderTest)
            {
                var configurationTypes = assembly.ExportedTypes.Where(e => e.GetInterfaces().Any(i => i == typeof(IDockerDatabaseConfiguration))).ToList();
                if (configurationTypes.Count == 0)
                    continue;
                if (configurationTypes.Count > 1)
                    throw new InvalidOperationException($"The assembly under test ({assembly.GetName().Name}) must have only one public type implementing the '{nameof(IDockerDatabaseConfiguration)}' interface but {configurationTypes.Count} were found: {string.Join(", ", configurationTypes.Select(e => e.FullName))}.");

                var configurationType = configurationTypes[0];
                var constructorInfo = configurationType.GetConstructor(Type.EmptyTypes);
                if (constructorInfo == null)
                    throw new InvalidOperationException($"The '{configurationType.FullName}' type must have a public default constructor.");

                return (IDockerDatabaseConfiguration)constructorInfo.Invoke(new object[0]);
            }
            throw new InvalidOperationException($"Could not find the assembly under test (i.e. where a referenced assembly matches {executingAssemblyName.Name})");
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
                RunDocker($"start \"{_configuration.ContainerName}\"");
            }
            catch (Exception e) when (e.Message.Contains("No such container"))
            {
                RunDocker($"run --name \"{_configuration.ContainerName}\" " + string.Join(" ", _configuration.Arguments));
            }

            var port = DockerContainerGetPort();
            return _configuration.ConnectionString(DockerGetHost(), port);
        }

        private string DockerGetHost()
        {
            try
            {
                return RunProcess("docker-machine", "ip");
            }
            catch (Exception)
            {
                return "localhost";
            }
        }

        private ushort DockerContainerGetPort()
        {
            var portLine = RunDocker($"port \"{_configuration.ContainerName}\"");
            var port = Regex.Match(portLine, @"-> 0\.0\.0\.0:(?<port>\d+)").Groups["port"];
            if (!port.Success)
                throw new ApplicationException($"Could not find port in '{portLine}'");
            return ushort.Parse(port.Value);
        }

        private string RunDocker(string arguments, bool waitForExit = true, bool trimResult = true)
        {
            return RunProcess("docker", arguments, waitForExit, trimResult);
        }

        private string RunProcess(string command, string arguments, bool waitForExit = true, bool trimResult = true)
        {
            var startInfo = new ProcessStartInfo(command, arguments) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
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
                    if (process.ExitCode != 0)
                    {
                        var error = process.StandardError.ReadToEnd();
                        throw new ApplicationException(error);
                    }
                    var result = process.StandardOutput.ReadToEnd();
                    return trimResult ? result.TrimEnd('\n') : result;
                }
                return null;
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