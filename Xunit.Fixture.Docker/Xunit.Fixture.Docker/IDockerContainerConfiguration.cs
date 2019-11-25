using System.Collections.Generic;
using System.IO;

namespace Xunit.Fixture.Docker
{
    /// <summary>
    /// Defines the interface required by <see cref="DockerFixture{TConfiguration}"/>
    /// </summary>
    public interface IDockerContainerConfiguration
    {
        /// <summary>
        /// The name given to the docker container. Used to start a container by its name if it already exists and to
        /// assign a name to the container for the given configuration.
        /// </summary>
        /// <example>MyLibrary_Tests_MySQL</example>
        string ContainerName { get; }

        /// <summary>
        /// The docker image name. May include a tag or not.
        /// </summary>
        /// <example>mysql/mysql-server:5.7</example>
        /// <example>redis</example>
        string ImageName { get; }

        /// <summary>
        /// Environment variables to pass to the container. See the documentation of the docker image for supported
        /// environment variables.
        /// </summary>
        IReadOnlyDictionary<string, string> EnvironmentVariables { get; }

        /// <summary>
        /// A mapping of directories from the host to be available in the container.
        /// </summary>
        /// <remarks>Neither the host nor the container path must include a colon (:) symbol.</remarks>
        IReadOnlyDictionary<DirectoryInfo, string> Volumes { get; }
    }
}