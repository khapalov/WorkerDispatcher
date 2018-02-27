using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.ScheduleStrategies
{
    internal class ScheduleParallelLimit : ScheduleStrategiesBase, IDisposable
    {
        private readonly IQueueWorker _queue;
        private readonly SemaphoreSlim _threadLimit;

        public ScheduleParallelLimit(IQueueWorker queue, ICounterBlocked processCount, TimeSpan timeLimit, int limit, IWorkerHandler workerHandler)
            : base(processCount, timeLimit, workerHandler)
        {
            _queue = queue;

            limit = limit <= 0 ? 5 : limit;

            _threadLimit = new SemaphoreSlim(limit, limit);
        }

        public override void Start(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested || !_queue.IsEmpty)
                {
                    await _threadLimit.WaitAsync();

                    await _queue.ReceiveAsync()
                        .ContinueWith(async invoker => ExcecuteInvoker(await invoker).ContinueWith(t => _threadLimit.Release()));

                }

            }, TaskCreationOptions.LongRunning);
        }

        ~ScheduleParallelLimit()
        {
            Dispose(false);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _threadLimit.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ScheduleParallelLimit() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
