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
        private readonly IQueueWorker _queue;

        public ScheduleParallel(IQueueWorker queue, ICounterBlocked processCount, TimeSpan timeLimit, IWorkerHandler workerHandler)
            : base(processCount, timeLimit, workerHandler)
        {
            _queue = queue;
        }

        public override void Start(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested || !_queue.IsEmpty)
                {
                    await _queue.ReceiveAsync()
                        .ContinueWith(async invoker => ExcecuteInvoker(await invoker), TaskContinuationOptions.OnlyOnRanToCompletion);
                }

            }, TaskCreationOptions.LongRunning);
        }       
    }
}
