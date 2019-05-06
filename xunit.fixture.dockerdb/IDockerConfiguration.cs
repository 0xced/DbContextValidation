using System;
using System.Data.Common;

namespace Xunit.Fixture.DockerDb
{
    public interface IDockerDatabaseConfiguration
    {
        string ConnectionString(ushort port);

        DbProviderFactory ProviderFactory { get; }
        TimeSpan Timeout { get; }
        string ContainerName { get; }
        string[] Arguments { get; }
        string[] SqlScripts { get; }
    }
}