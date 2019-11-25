using System.Collections.Generic;

namespace Xunit.Fixture.Docker
{
    /// <summary>
    /// The ContainerInfo class holds information about a running docker container.
    /// </summary>
    public class ContainerInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerInfo"/> class.
        /// </summary>
        /// <param name="containerId">The docker container id.</param>
        /// <param name="host">The host that one must connect to in order to reach the docker container.</param>
        /// <param name="ports">A list of port mapping between the host and the container.</param>
        public ContainerInfo(ContainerId containerId, string host, IReadOnlyList<(ushort containerPort, ushort hostPort)> ports)
        {
            ContainerId = containerId;
            Host = host;
            Ports = ports;
        }

        /// <summary>
        /// The docker container id.
        /// </summary>
        public ContainerId ContainerId { get; }

        /// <summary>
        /// The host that one must connect to in order to reach the docker container.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// A list of port mapping between the host and the container.
        /// </summary>
        public IReadOnlyList<(ushort containerPort, ushort hostPort)> Ports { get; }
    }
}