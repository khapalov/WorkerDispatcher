using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.ScheduleStrategies
{
    internal abstract class ScheduleStrategiesBase : IScheduleStrategies, IDisposable
    {
        private readonly ICounterBlocked _processCount;
        private readonly TimeSpan _timeLimit;
        private readonly IWorkerHandler _handler;
        private readonly IQueueWorker _queueWorker;
        private readonly IWorkerRunner _workerRunner;

        public ScheduleStrategiesBase(IQueueWorker queueWorker, ICounterBlocked processCount, TimeSpan timeLimit, IWorkerHandler workerHandler)
        {
            _processCount = processCount;
            _timeLimit = timeLimit;
            _handler = workerHandler;
            _queueWorker = queueWorker;
            _workerRunner = CreateWorkerRunner(_processCount, workerHandler, _timeLimit);
        }

        protected virtual IWorkerRunner CreateWorkerRunner(ICounterBlocked counterBlocked, IWorkerHandler workerHandler, TimeSpan timeLimit)
        {
            return new DefaultWorkerRunner(_processCount, workerHandler, timeLimit, _queueWorker);
        }

        public void Start(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await StartCore(_workerRunner, _queueWorker, cancellationToken);
                }
                catch(TaskCanceledException)
                {                    
                    Debug.WriteLine("cancellation process");
                }
                catch (Exception ex)
                {
                    _handler.HandleFault(ex);
                }
            }, TaskCreationOptions.LongRunning);
        }

        protected abstract Task StartCore(IWorkerRunner workerRunner, IQueueWorker queueWorker, CancellationToken cancellationToken);             

#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_queueWorker != null)
                    {
                        _queueWorker.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ScheduleStrategiesBase() {
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
