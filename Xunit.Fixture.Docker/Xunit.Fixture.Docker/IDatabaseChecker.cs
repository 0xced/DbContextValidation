using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Fixture.Docker
{
    internal interface IDatabaseChecker
    {
        Task WaitForDatabaseAsync(string connectionString, Func<IDbConnection, Task> onOpened = null, CancellationToken cancellationToken = default);
    }
}