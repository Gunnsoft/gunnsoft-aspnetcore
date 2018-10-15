using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gunnsoft.AspNetCore
{
    // TODO Move to Gunnsoft.AspNetCore
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceProvider _serviceProvider;

        public QueuedHostedService
        (
            IBackgroundTaskQueue queue,
            IServiceProvider serviceProvider,
            ILogger<QueuedHostedService> logger
        )
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync
        (
            CancellationToken stoppingToken
        )
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);

                try
                {
                    await workItem(_serviceProvider, stoppingToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError
                    (
                        exception,
                        $"Failed to execute {nameof(workItem)}"
                    );
                }
            }
        }
    }
}
