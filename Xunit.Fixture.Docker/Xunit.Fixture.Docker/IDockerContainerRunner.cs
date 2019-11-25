using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Fixture.Docker
{
    internal interface IDockerContainerRunner
    {
        Task<ContainerInfo> StartContainerAsync(IDockerContainerConfiguration configuration, CancellationToken cancellationToken = default);
        Task StopContainerAsync(ContainerId containerId, bool wait, CancellationToken cancellationToken = default);
    }
}