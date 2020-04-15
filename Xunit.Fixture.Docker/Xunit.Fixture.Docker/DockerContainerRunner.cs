using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Fixture.Docker
{
    internal class DockerContainerRunner : IDockerContainerRunner
    {
        private IDockerContainerConfiguration _configuration;

        public EventHandler<CommandEventArgs> RunningCommand { get; set; }
        public EventHandler<RanCommandEventArgs> RanCommand { get; set; }

        public async Task<ContainerInfo> StartContainerAsync(IDockerContainerConfiguration configuration, CancellationToken cancellationToken = default)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            var dockerStartDateTime = DateTime.Now;

            var arguments = GetDockerRunArguments();
            var containerId = (await RunDockerAsync("run " + arguments, cancellationToken: cancellationToken)).output;
            var ports = await DockerContainerGetPortsAsync(dockerStartDateTime, containerId, cancellationToken);

            var hostFormat = Environment.GetEnvironmentVariable("XUNIT_FIXTURE_DOCKER_HOST_FORMAT");
            if (hostFormat != null)
            {
                // The idea is to use {{ .NetworkSettings.Networks.nat.IPAddress }} or whatever network is configured on Docker on CI, e.g. AppVeyor
                // See https://github.com/docker/for-win/issues/204 and https://stackoverflow.com/questions/44817861/windows-container-port-binding-not-working-on-windows-server-2016-using-docker/44827162#44827162
                var address = (await RunDockerAsync($"inspect {containerId} --format={hostFormat}", cancellationToken: cancellationToken)).output;
                return new ContainerInfo(new ContainerId(containerId), address, ports);
            }

            var host = await DockerGetHostAsync(cancellationToken);
            return new ContainerInfo(new ContainerId(containerId), host, ports);
        }

        public async Task StopContainerAsync(ContainerId containerId, bool wait, CancellationToken cancellationToken = default)
        {
            await RunDockerAsync($"stop \"{containerId}\"", waitForExit: wait, cancellationToken: cancellationToken);
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
                    "--publish-all",
                    "--detach",
                    "--rm",
                    $"\"{_configuration.ImageName}\"",
                });
            return string.Join(" ", arguments);
        }

        private async Task<string> DockerGetHostAsync(CancellationToken cancellationToken)
        {
            try
            {
                return (await RunProcessAsync("docker-machine", "ip", cancellationToken: cancellationToken)).output;
            }
            catch (Exception)
            {
                return "127.0.0.1";
            }
        }

        private async Task<IReadOnlyList<(ushort containerPort, ushort hostPort)>> DockerContainerGetPortsAsync(DateTime dockerStartDateTime, string containerId, CancellationToken cancellationToken)
        {
            var portLines = (await RunDockerAsync($"port {containerId}", cancellationToken: cancellationToken)).output;
            using (var reader = new StringReader(portLines))
            {
                var ports = new List<(ushort containerPort, ushort hostPort)>();
                string portLine;
                while ((portLine = await reader.ReadLineAsync()) != null)
                {
                    var match = Regex.Match(portLine, @"(?<containerPort>\d+)/.* -> 0\.0\.0\.0:(?<hostPort>\d+)");
                    var containerPort = match.Groups["containerPort"];
                    var hostPort = match.Groups["hostPort"];
                    if (!(containerPort.Success && hostPort.Success))
                    {
                        string logs;
                        try
                        {
                            var (output, error) = await RunDockerAsync($"logs --since {dockerStartDateTime:O} {containerId}", trimResult: false, cancellationToken: cancellationToken);
                            logs = !string.IsNullOrWhiteSpace(error) ? error : output;
                        }
                        catch
                        {
                            logs = "";
                        }
                        var message = string.IsNullOrWhiteSpace(logs) ? "Please check its logs." : "Here are its logs: " + Environment.NewLine + logs;
                        throw new ApplicationException($"The container {containerId} failed to start properly. {message}");
                    }

                    ports.Add((ushort.Parse(containerPort.Value), ushort.Parse(hostPort.Value)));
                }
                return ports;
            }
        }

        private async Task<(string output, string error)> RunDockerAsync(string arguments, bool waitForExit = true, bool trimResult = true, CancellationToken cancellationToken = default)
        {
            return await RunProcessAsync("docker", arguments, waitForExit, trimResult, cancellationToken);
        }

        private async Task<(string output, string error)> RunProcessAsync(string command, string arguments, bool waitForExit = true, bool trimResult = true, CancellationToken cancellationToken = default)
        {
            var startInfo = new ProcessStartInfo(command, arguments) { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            using (var process = new Process {StartInfo = startInfo})
            {
                RunningCommand?.Invoke(this, new CommandEventArgs(command, arguments));
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
                    var exitCode = await process.WaitForExitAsync(cancellationToken);
                    var error = await process.StandardError.ReadToEndAsync();
                    if (exitCode != 0)
                    {
                        throw new ApplicationException(error);
                    }
                    var output = await process.StandardOutput.ReadToEndAsync();
                    RanCommand?.Invoke(this, new RanCommandEventArgs(command, arguments, output));
                    return trimResult ? (output.TrimEnd('\n'), error.TrimEnd('\n')) : (output, error);
                }
                return (null, null);
            }
        }
    }
}