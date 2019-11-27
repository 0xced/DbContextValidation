using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Fixture.Docker
{
    internal class DatabaseChecker : IDatabaseChecker
    {
        private readonly IDatabaseConfiguration _databaseConfiguration;

        public DatabaseChecker(IDatabaseConfiguration databaseConfiguration)
        {
            _databaseConfiguration = databaseConfiguration ?? throw new ArgumentNullException(nameof(databaseConfiguration));
        }

        public async Task WaitForDatabaseAsync(string connectionString, Func<IDbConnection, Task> onOpened = null, CancellationToken cancellationToken = default)
        {
            var stopWatch = Stopwatch.StartNew();
            while (true)
            {
                using (var connection = _databaseConfiguration.ProviderFactory.CreateConnection() ?? throw new InvalidOperationException($"Failed to create a connection with {_databaseConfiguration.ProviderFactory}."))
                {
                    var success = false;
                    connection.ConnectionString = connectionString;
                    try
                    {
                        await connection.OpenAsync(cancellationToken);
                        success = true;
                    }
                    catch (Exception exception)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(value: 1), cancellationToken);
                        if (stopWatch.Elapsed > _databaseConfiguration.Timeout)
                        {
                            throw new TimeoutException($"Database was not available on \"{connectionString}\" after waiting for {_databaseConfiguration.Timeout.TotalSeconds:F1} seconds.", exception);
                        }
                    }
                    if (success)
                    {
                        if (onOpened != null)
                        {
                            await onOpened(connection);
                        }
                        break;
                    }
                }
            }
        }
    }
}