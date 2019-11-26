using System;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Fixture.Docker
{
    /// <summary>
    /// A fixture that takes care of starting and stopping a docker container and provides a <see cref="ContainerInfo"/>.
    /// </summary>
    /// <typeparam name="TConfiguration">A type that must conform to <see cref="IDockerContainerConfiguration"/> and have a default constructor.</typeparam>
    public class DockerFixture<TConfiguration> : IAsyncLifetime where TConfiguration : IDockerContainerConfiguration, new()
    {
        private readonly IDockerContainerRunner _dockerContainerRunner;
        private readonly IMessageSink _sink;

        /// <summary>
        /// The docker container configuration.
        /// </summary>
        protected readonly TConfiguration Configuration;

        private ContainerInfo _containerInfo;

        /// <summary>
        /// Initialize a new instance of the <see cref="DockerFixture{TConfiguration}"/> class.
        /// </summary>
        /// <param name="sink">The <see cref="IMessageSink"/> provided my xUnit.</param>
        /// <remarks>This constructor is called automatically by xUnit.</remarks>
        public DockerFixture(IMessageSink sink)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            Configuration = new TConfiguration();
            var dockerContainerRunner = new DockerContainerRunner();
            dockerContainerRunner.RunningCommand += (sender, args) => WriteDiagnostic($"> {args.Command} {args.Arguments}");
            _dockerContainerRunner = dockerContainerRunner;
        }

        /// <summary>
        /// Get information about a running docker container.
        /// </summary>
        /// <remarks>The <c>ContainerInfo</c> property is only available after <see cref="InitializeAsync"/> has completed.</remarks>
        public ContainerInfo ContainerInfo => _containerInfo ?? throw new InvalidOperationException($"Can not call {nameof(ContainerInfo)} before {nameof(IAsyncLifetime.InitializeAsync)} has completed");

        /// <summary>
        /// Starts the docker container.
        /// </summary>
        /// <returns>A task representing the container start operation.</returns>
        public virtual async Task InitializeAsync()
        {
            _containerInfo = await _dockerContainerRunner.StartContainerAsync(Configuration);
        }

        /// <summary>
        /// Stops the docker container.
        /// </summary>
        /// <returns>A task representing the container stop operation.</returns>
        /// <remarks>The method may return before the container is fully stopped.</remarks>
        public virtual async Task DisposeAsync()
        {
            var containerId = _containerInfo?.ContainerId;
            if (containerId != null)
            {
                await _dockerContainerRunner.StopContainerAsync(containerId, wait: false);
            }
        }

        /// <summary>
        /// Writes a diagnostic message to the xUnit's message sink.
        /// </summary>
        /// <param name="message">The message to write to the sink.</param>
        protected void WriteDiagnostic(string message)
        {
            _sink.OnMessage(new DiagnosticMessage(message));
        }
    }
}