using System;

namespace DbContextValidation.Tests
{
    public interface IDockerDatabaseConfiguration
    {
        string ConnectionString(ushort port);

        TimeSpan Timeout { get; }
        string ContainerName { get; }
        string[] Arguments { get; }
        string[] SqlScripts { get; }
    }
}