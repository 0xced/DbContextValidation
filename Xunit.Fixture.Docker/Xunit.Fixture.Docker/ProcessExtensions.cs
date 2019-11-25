using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Fixture.Docker
{
    /// <summary>
    /// Async extensions to the <see cref="Process"/> class.
    /// </summary>
    public static class ProcessExtensions
    {
        // Adapted from https://github.com/Microsoft/vs-threading/blob/a25ce38cd972ab15fabcc055dc0aa7fb8bf0c344/src/Microsoft.VisualStudio.Threading/AwaitExtensions.cs#L62-L100
        /// <summary>
        /// Returns a task that completes when the process exits and provides the exit code of that process.
        /// </summary>
        /// <param name="process">The process to wait for exit.</param>
        /// <param name="cancellationToken">
        /// A token whose cancellation will cause the returned Task to complete
        /// before the process exits in a faulted state with an <see cref="OperationCanceledException"/>.
        /// This token has no effect on the <paramref name="process"/> itself.
        /// </param>
        /// <returns>A task whose result is the <see cref="Process.ExitCode"/> of the <paramref name="process"/>.</returns>
        public static async Task<int> WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));

            var tcs = new TaskCompletionSource<int>();

            void ExitHandler(object s, EventArgs e)
            {
                tcs.TrySetResult(process.ExitCode);
            }

            try
            {
                process.EnableRaisingEvents = true;
                process.Exited += ExitHandler;
                if (process.HasExited)
                {
                    // Allow for the race condition that the process has already exited.
                    tcs.TrySetResult(process.ExitCode);
                }

                using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
                {
                    return await tcs.Task.ConfigureAwait(continueOnCapturedContext: false);
                }
            }
            finally
            {
                process.Exited -= ExitHandler;
            }
        }
    }
}