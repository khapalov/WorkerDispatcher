using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.ScheduleStrategies
{
    internal class ScheduleConsistent : ScheduleStrategiesBase
    {
        public ScheduleConsistent(IQueueWorker queue, ICounterBlocked processCount, TimeSpan timeLimit, IWorkerHandler workerHandler)
           : base(queue, processCount, timeLimit, workerHandler)
        {
        }

        protected override async Task StartCore(IWorkerRunner workerRunner, IQueueWorker queueWorker, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested || !queueWorker.IsEmpty)
            {
                var invoker = await queueWorker.ReceiveAsync();

                await workerRunner.ExcecuteInvoker(invoker);
            }
        }
    }
}
