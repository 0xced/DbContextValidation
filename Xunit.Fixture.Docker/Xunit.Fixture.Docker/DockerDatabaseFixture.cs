using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Fixture.Docker
{
    /// <summary>
    /// A fixture that takes care of starting and stopping a database docker container and provides a
    /// <see cref="ConnectionString"/> in order to connect to the database exposed by the docker container.
    /// </summary>
    /// <typeparam name="TConfiguration">A type that must conform to <see cref="IDockerContainerConfiguration"/>, <see cref="IDatabaseConfiguration"/>, and have a default constructor.</typeparam>
    public class DockerDatabaseFixture<TConfiguration> : DockerFixture<TConfiguration> where TConfiguration : IDatabaseConfiguration, IDockerContainerConfiguration, new()
    {
        private string _connectionString;

        /// <summary>
        /// Initialize a new instance of the <see cref="DockerDatabaseFixture{TConfiguration}"/> class.
        /// </summary>
        /// <param name="sink">The <see cref="IMessageSink"/> provided my xUnit.</param>
        /// <remarks>This constructor is called automatically by xUnit.</remarks>
        public DockerDatabaseFixture(IMessageSink sink) : base(sink)
        {
        }

        /// <summary>
        /// The connection string to use in order to connect to the database exposed by the docker container.
        /// </summary>
        /// <remarks>The <c>ConnectionString</c> property is only available after <see cref="InitializeAsync"/> has completed.</remarks>
        public string ConnectionString => _connectionString ?? throw new InvalidOperationException($"Can not call {nameof(ConnectionString)} before {nameof(IAsyncLifetime.InitializeAsync)} has completed");

        /// <summary>
        /// Starts the docker container, ensures that the database is available and executes the
        /// configured <see cref="IDatabaseConfiguration.SqlStatements"/>.
        /// </summary>
        /// <returns>A task representing the container start operation.</returns>
        public override async Task InitializeAsync()
        {
            if (Configuration.ImageName == null)
            {
                _connectionString = Configuration.ConnectionString(host: null, port: 0);
            }
            else
            {
                await base.InitializeAsync();
                var ports = ContainerInfo.Ports;
                if (ports.Count == 0)
                    throw new ApplicationException($"The docker image {Configuration.ImageName} does not expose any port.");
                var containerPort = Configuration.Port;
                (ushort containerPort, ushort hostPort) portMapping;
                if (containerPort.HasValue)
                {
                    portMapping = ports.SingleOrDefault(p => p.containerPort == containerPort.Value);
                    if (portMapping == default)
                    {
                        throw new ApplicationException($"The docker image '{Configuration.ImageName}' does not expose port {containerPort.Value}. " +
                                                       $"Please check the documentation of this docker image and fix the '{Configuration.GetType().FullName}.{nameof(Configuration.Port)}' value.");
                    }
                }
                else
                {
                    if (ports.Count > 1)
                    {
                        throw new ApplicationException($"The docker image '{Configuration.ImageName}' exposes multiple ports ({string.Join(", ", ports.Select(e => e.containerPort))}). " +
                                                       $"Please specify which one is the database port that must be used with the '{Configuration.GetType().FullName}.{nameof(Configuration.Port)}' value.");
                    }
                    portMapping = ports[0];
                }
                bool.TryParse(Environment.GetEnvironmentVariable("XUNIT_FIXTURE_DOCKER_USE_CONTAINER_PORT"), out var useContainerPort);
                var port = useContainerPort ? portMapping.containerPort : portMapping.hostPort;
                _connectionString = Configuration.ConnectionString(ContainerInfo.Host, port);
            }

            IDatabaseChecker databaseChecker = new DatabaseChecker(Configuration);
            WriteDiagnostic($"Waiting for database to be available on {_connectionString}");
            var stopWatch = Stopwatch.StartNew();
            await databaseChecker.WaitForDatabaseAsync(_connectionString, connection => RunSqlStatementsAsync(connection, Configuration.SqlStatements));
            WriteDiagnostic($"It took {stopWatch.Elapsed.TotalSeconds:F1} seconds for the database to become available.");
        }

        private async Task RunSqlStatementsAsync(IDbConnection dbConnection, IEnumerable<string> sqlStatements)
        {
            foreach (var statement in sqlStatements)
            {
                WriteDiagnostic($"Executing SQL statement{Environment.NewLine}{statement}");
                var command = dbConnection.CreateCommand();
                command.CommandText = statement;
                command.CommandType = CommandType.Text;
                if (command is DbCommand dbCommand)
                {
                    await dbCommand.ExecuteNonQueryAsync();
                }
                else
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}