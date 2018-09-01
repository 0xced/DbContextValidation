using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Xunit;

namespace DbContextValidation.Tests
{
    public class DockerFixture : IAsyncLifetime
    {
        private DockerClient _client;
        private string _containerId;
        
        public async Task InitializeAsync()
        {
            var containerName = "/DbContextValidation.Tests." + Config.Provider;
            (_client, _containerId) = await EnsureContainerIsRunningAsync(containerName);
        }

        public async Task DisposeAsync()
        {
            if (_client == null)
                return;
            
            await _client?.Containers.StopContainerAsync(_containerId, new ContainerStopParameters());
            _client?.Dispose();
        }

        private static string SqlDirectory(string directoryName)
        {
            var assemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            var solutionDirectory = assemblyDirectory?.Parent?.Parent?.Parent?.Parent ?? throw new FileNotFoundException("Solution directory not found");
            var sqlDirectory = Path.Combine(solutionDirectory.FullName, "DbContextValidation.Tests", directoryName);
            if (!Directory.Exists(sqlDirectory))
            {
                throw new FileNotFoundException("Directory with SQL scripts not found", sqlDirectory);
            }
            return sqlDirectory;
        }
        
        public static async Task<(DockerClient dockerClient, string containerId)> EnsureContainerIsRunningAsync(string containerName)
        {
            if (Config.Provider == Provider.SQLite)
                return (null, null);
#if NETFRAMEWORK
            // Docker.DotNet does not work on mono yet, see https://github.com/Microsoft/Docker.DotNet/pull/323
            await Task.Delay(0); // to silence warning about async method
            return (null, null);
#else
            var endpointBaseUri = new Uri("unix:/var/run/docker.sock");
            var client = new DockerClientConfiguration(endpointBaseUri).CreateClient();
            IList<ContainerListResponse> containers;
            try
            {
                var listParameters = new ContainersListParameters { All = true };
                containers = await client.Containers.ListContainersAsync(listParameters);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Docker must be running to run {Config.Provider} tests", exception);
            }

            var container = containers.FirstOrDefault(e => e.Names.Contains(containerName));
            if (container?.State == "running")
                return (client, container.ID);
            
            var containerId = container?.ID ?? await CreateContainer(client, containerName);
            await client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
            return (client, containerId);
#endif
        }

        private static async Task<string> CreateContainer(IDockerClient client, string containerName)
        {
            var createParameters = Config.ContainerParameters(containerName, SqlDirectory);
            IList<ImagesListResponse> images;
            try
            {
                images = await client.Images.ListImagesAsync(new ImagesListParameters());
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Failed to list Docker images", exception);
            }

            if (!images.SelectMany(e => e.RepoTags).Contains(createParameters.Image))
            {
                try
                {
                    var imagesCreateParameters = new ImagesCreateParameters {FromImage = createParameters.Image};
                    var progress = new Progress<JSONMessage>(message => Console.WriteLine($"Pulling {createParameters.Image} -> {message.ProgressMessage} ({message.Progress})"));
                    await client.Images.CreateImageAsync(imagesCreateParameters, null, progress);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException($"Failed to pull Docker image {createParameters.Image}", exception);
                }
            }

            CreateContainerResponse response;
            try
            {
                response = await client.Containers.CreateContainerAsync(createParameters);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to create Docker container {containerName}", exception);
            }

            if (response.Warnings?.Any() ?? false)
                throw new InvalidOperationException($"Failed to create Docker container {containerName}: {string.Join(Environment.NewLine, response.Warnings)}");

            return response.ID;
        }
    }
}