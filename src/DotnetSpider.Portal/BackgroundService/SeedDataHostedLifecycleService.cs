using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Lifetime = Microsoft.Extensions.Hosting.IHostApplicationLifetime;

namespace DotnetSpider.Portal.BackgroundService
{
    public class SeedDataHostedLifecycleService
        : IHostedLifecycleService
    {

        private readonly Lifetime _applicationLifetime;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly PortalOptions _options;

        internal Task startupTask;

        public SeedDataHostedLifecycleService(
            Lifetime applicationLifetime,
            IServiceProvider serviceProvider,
            PortalOptions options,
            ILogger<SeedDataHostedLifecycleService> logger)
        {
            this._applicationLifetime = applicationLifetime;
            this._serviceProvider = serviceProvider;
            this._options = options;
            this._logger = logger;
        }

        public virtual Task StartingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting SeedDataHostService");

            startupTask = AwaitStartupCompletionAndStartSchedulerAsync(cancellationToken);

            // If the task completed synchronously, await it in order to bubble potential cancellation/failure to the caller
            // Otherwise, return, allowing application startup to complete
            if (startupTask.IsCompleted)
            {
                await startupTask.ConfigureAwait(false);
            }
        }

        public virtual Task StartedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task AwaitStartupCompletionAndStartSchedulerAsync(CancellationToken startupCancellationToken)
        {
            using var combinedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(startupCancellationToken, this._applicationLifetime.ApplicationStarted);

            await Task.Delay(Timeout.InfiniteTimeSpan, combinedCancellationSource.Token) // Wait "indefinitely", until startup completes or is aborted
                .ContinueWith(_ => { }, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Default) // Without an OperationCanceledException on cancellation
                .ConfigureAwait(false);

            if (!startupCancellationToken.IsCancellationRequested)
            {
                await StartSeedingAsync(this._applicationLifetime.ApplicationStopping).ConfigureAwait(false); // Startup has finished, but ApplicationStopping may still interrupt starting of the scheduler
            }
        }

        /// <summary>
        /// Starts the <see cref="IScheduler"/>, either immediately or after the delay configured in the <see cref="options"/>.
        /// </summary>
        private async Task StartSeedingAsync(CancellationToken cancellationToken)
        {
            // Avoid potential race conditions between ourselves and StopAsync, in case it has already made its attempt to stop the scheduler
            if (this._applicationLifetime.ApplicationStopping.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await SeedData.InitializeAsync(this._options, this._serviceProvider).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        public virtual Task StoppingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Wait until any ongoing startup logic has finished or the graceful shutdown period is over
                await Task.WhenAny(startupTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
            finally
            {
            }
        }

        public virtual Task StoppedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
