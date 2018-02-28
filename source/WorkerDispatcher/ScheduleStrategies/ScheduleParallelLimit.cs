using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.ScheduleStrategies
{
    internal class ScheduleParallelLimit : ScheduleStrategiesBase
    {
        private readonly SemaphoreSlim _threadLimit;

        public ScheduleParallelLimit(IQueueWorker queue, ICounterBlocked processCount, TimeSpan timeLimit, int limit, IWorkerHandler workerHandler)
            : base(queue, processCount, timeLimit, workerHandler)
        {
            limit = limit <= 0 ? 5 : limit;

            _threadLimit = new SemaphoreSlim(limit, limit);
        }

        protected override async Task StartCore(IWorkerRunner workerRunner, IQueueWorker queueWorker, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested || !queueWorker.IsEmpty)
            {
                await _threadLimit.WaitAsync()
                    .ContinueWith(async t =>
                        await queueWorker.ReceiveAsync()
                            .ContinueWith(async invoker => workerRunner.ExcecuteInvoker(await invoker)
                                .ContinueWith(t2 => _threadLimit.Release()))
                        );
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_threadLimit != null)
                {
                    _threadLimit.Dispose();
                }
            }
            base.Dispose(disposing);
        }        
    }
}
