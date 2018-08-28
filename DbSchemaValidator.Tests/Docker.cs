using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Docker.DotNet;
using Docker.DotNet.Models;

#if PROVIDER_SQLSERVER && NETFRAMEWORK
using DbSchemaValidator.Tests.EF6.SqlServer;
#elif PROVIDER_MYSQL && NETFRAMEWORK
using DbSchemaValidator.Tests.EF6.MySQL;
#elif PROVIDER_NPGSQL && NETFRAMEWORK
using DbSchemaValidator.Tests.EF6.Npgsql;
#elif PROVIDER_SQLITE && NETFRAMEWORK
using DbSchemaValidator.Tests.EF6.SQLite;
#elif PROVIDER_SQLSERVER
using DbSchemaValidator.Tests.EFCore.SqlServer;
#elif PROVIDER_MYSQL
using DbSchemaValidator.Tests.EFCore.MySQL;
#elif PROVIDER_NPGSQL
using DbSchemaValidator.Tests.EFCore.Npgsql;
#elif PROVIDER_SQLITE 
using DbSchemaValidator.Tests.EFCore.SQLite;
#endif

namespace DbSchemaValidator.Tests
{
    public static class Docker
    {
        private static string SqlDirectory(string directoryName)
        {
            var assemblyDirectory = new FileInfo(Assembly.GetAssembly(typeof(Context)).Location).Directory;
            var solutionDirectory = assemblyDirectory?.Parent?.Parent?.Parent?.Parent ?? throw new FileNotFoundException("Solution directory not found");
            return Path.Combine(solutionDirectory.FullName, "DbSchemaValidator.Tests", directoryName);
        }
        
        private static CreateContainerParameters MySQLParameters(string containerName)
        {
            return new CreateContainerParameters
            {
                Name = containerName,
                Image = "mysql/mysql-server:5.7",
                Env = new [] { $"MYSQL_ROOT_PASSWORD={Configuration.Password}", $"MYSQL_DATABASE={Configuration.Database}", "MYSQL_ROOT_HOST=%" },
                HostConfig = new HostConfig
                {
                    Binds = new [] { $"{SqlDirectory("SQL.MySQL")}:/docker-entrypoint-initdb.d:ro" },
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        [$"{Configuration.Port}/tcp"] = new []{ new PortBinding { HostPort = Configuration.Port } }
                    }
                }
            };
        }

        private static CreateContainerParameters NpgsqlParameters(string containerName)
        {
            return new CreateContainerParameters
            {
                Name = containerName,
                Image = "postgres:10.5-alpine",
                Env = new [] { $"POSTGRES_PASSWORD={Configuration.Password}", $"POSTGRES_DB={Configuration.Database}" },
                HostConfig = new HostConfig
                {
                    Binds = new [] { $"{SqlDirectory("SQL.Common")}:/docker-entrypoint-initdb.d:ro" },
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        [$"{Configuration.Port}/tcp"] = new []{ new PortBinding { HostPort = Configuration.Port } }
                    }
                }
            };
        }

        private static CreateContainerParameters SqlServerParameters(string containerName)
        {
            return new CreateContainerParameters
            {
                Name = containerName,
                Image = "genschsa/mssql-server-linux:latest",
                Env = new [] { "ACCEPT_EULA=Y", $"MSSQL_SA_PASSWORD=${Configuration.Password}" },
                HostConfig = new HostConfig
                {
                    Binds = new [] { $"{SqlDirectory("SQL.SqlServer")}:/docker-entrypoint-initdb.d:ro" },
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        [$"{Configuration.Port}/tcp"] = new []{ new PortBinding { HostPort = Configuration.Port } }
                    }
                }
            };
        }

        private static CreateContainerParameters CreateParameters(Provider provider, string containerName)
        {
            switch (provider)
            {
                case Provider.MySQL:
                    return MySQLParameters(containerName);
                case Provider.Npgsql:
                    return NpgsqlParameters(containerName);
                case Provider.SqlServer:
                    return SqlServerParameters(containerName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
            }
        }

        public static void EnsureDockerContainerIsRunning(Provider provider, string containerName)
        {
#if NETFRAMEWORK
            // Docker.DotNet does not work on mono yet, see https://github.com/Microsoft/Docker.DotNet/pull/323
            return;
#endif
            var endpointBaseUri = new Uri("unix:/var/run/docker.sock");
            var client = new DockerClientConfiguration(endpointBaseUri).CreateClient();
            IList<ContainerListResponse> containers;
            try
            {
                var listParameters = new ContainersListParameters { All = true };
                containers = client.Containers.ListContainersAsync(listParameters).Result;
            }
            catch (DockerApiException exception)
            {
                throw new InvalidOperationException($"Docker must be running to run {provider} tests", exception);
            }

            if (containers.Where(e => e.State == "running").SelectMany(e => e.Names).Contains(containerName))
                return;
            
            var container = containers.FirstOrDefault(e => e.Names.Contains(containerName));
            var containerId = container?.ID ?? CreateContainer(client, provider, containerName);
            client.Containers.StartContainerAsync(containerId, new ContainerStartParameters()).Wait();
        }

        private static string CreateContainer(IDockerClient client, Provider provider, string containerName)
        {
            var createParameters = CreateParameters(provider, containerName);
            IList<ImagesListResponse> images;
            try
            {
                images = client.Images.ListImagesAsync(new ImagesListParameters()).Result;
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
                    client.Images.CreateImageAsync(imagesCreateParameters, null, progress).Wait();
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException($"Failed to pull Docker image {createParameters.Image}", exception);
                }
            }

            CreateContainerResponse response;
            try
            {
                response = client.Containers.CreateContainerAsync(createParameters).Result;
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