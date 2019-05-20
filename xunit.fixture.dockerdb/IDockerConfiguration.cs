using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace Xunit.Fixture.DockerDb
{
    public interface IDockerDatabaseConfiguration
    {
        string ConnectionString(string host, ushort port);

        DbProviderFactory ProviderFactory { get; }
        TimeSpan Timeout { get; }

        string ContainerName { get; }
        string ImageName { get; }
        IReadOnlyDictionary<string, string> EnvironmentVariables { get; }
        ushort Port { get; }
        IReadOnlyDictionary<DirectoryInfo, string> Volumes { get; }
        IEnumerable<string> SqlStatements { get; }
    }
}