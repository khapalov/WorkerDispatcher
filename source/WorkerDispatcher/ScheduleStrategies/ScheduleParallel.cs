using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.ScheduleStrategies
{
    internal class ScheduleParallel : ScheduleStrategiesBase
    {
        public ScheduleParallel(IQueueWorker queue, ICounterBlocked processCount, TimeSpan timeLimit, IWorkerHandler workerHandler)
            : base(queue, processCount, timeLimit, workerHandler)
        {
        }

        protected override async Task StartCore(IWorkerRunner workerRunner, IQueueWorker queueWorker, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested || !queueWorker.IsEmpty)
            {
                await queueWorker.ReceiveAsync()
                    .ContinueWith(async invoker => await workerRunner.ExcecuteInvoker(await invoker), cancellationToken);
            }
        }       
    }
}
