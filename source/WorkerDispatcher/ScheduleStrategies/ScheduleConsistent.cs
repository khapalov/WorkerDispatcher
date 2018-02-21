using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.ScheduleStrategies
{
    internal class ScheduleConsistent : ScheduleStrategiesBase
    {
        private readonly IQueueWorker _queue;

        public ScheduleConsistent(IQueueWorker queue, ICounterBlocked processCount, TimeSpan timeLimit, IWorkerHandler workerHandler)
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
                    var invoker = await _queue.ReceiveAsync();

                    await ExcecuteInvoker(invoker);                        
                }

            }, TaskCreationOptions.LongRunning);
        }
    }
}
