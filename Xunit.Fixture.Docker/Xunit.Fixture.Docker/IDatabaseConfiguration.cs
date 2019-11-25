using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Xunit.Fixture.Docker
{
    /// <summary>
    /// Defines the interface required by <see cref="DockerDatabaseFixture{TConfiguration}"/>
    /// </summary>
    public interface IDatabaseConfiguration
    {
        /// <param name="host">The host on which the database is running.</param>
        /// <param name="port">The port on which the database is listening for connections.</param>
        /// <returns>The connection string for the given <paramref name="host"/> and <paramref name="port"/>.</returns>
        string ConnectionString(string host, ushort port);

        /// <summary>
        /// The provider factory used for connecting to the database and executing the <see cref="SqlStatements"/>.
        /// </summary>
        DbProviderFactory ProviderFactory { get; }

        /// <summary>
        /// The exposed database port by the docker container. See the documentation of the docker image for exposed ports.
        /// </summary>
        /// <remarks>If null, the first port exposed by the docker container is used as the database port.</remarks>
        /// <example>3306</example>
        ushort? Port { get; }

        /// <summary>
        /// The amount of time to wait for the database to be available after the docker container has started before giving up.
        /// </summary>
        TimeSpan Timeout { get; }

        /// <summary>
        /// A list of SQL statements to execute on the database as soon as it is available.
        /// </summary>
        IEnumerable<string> SqlStatements { get; }
    }
}