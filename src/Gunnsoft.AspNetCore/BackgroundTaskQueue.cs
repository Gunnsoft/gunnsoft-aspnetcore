using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Gunnsoft.AspNetCore
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly SemaphoreSlim _signal;
        private readonly ConcurrentQueue<Func<IServiceProvider, CancellationToken, Task>> _workItems;

        public BackgroundTaskQueue()
        {
            _signal = new SemaphoreSlim(0);
            _workItems = new ConcurrentQueue<Func<IServiceProvider, CancellationToken, Task>>();
        }

        public void QueueBackgroundWorkItem(Func<IServiceProvider, CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);

            _workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }
}
