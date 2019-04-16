using System;
using System.Collections.Concurrent;
using System.Threading;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchToken : IBatchToken
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly LocalQueueManager _queue;
        private readonly BatchQueueProvider _queueProvider;
        private readonly QueueEvent<Type> _queueEvent;
        private readonly ManualResetEventSlim _manualResetEventSlim;

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        internal BatchToken(LocalQueueManager queue, BatchQueueProvider queueProvider, QueueEvent<Type> queueEvent, ManualResetEventSlim manualResetEventSlim)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _queue = queue;
            _queueProvider = queueProvider;
            _queueEvent = queueEvent;
            _manualResetEventSlim = manualResetEventSlim;
        }

        public void Send<TData>(TData data)
        {
            _queue.Enqueue(data);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _manualResetEventSlim.Wait();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _queueProvider.Dispose();
            _cancellationTokenSource.Dispose();
        }

        public void Flush<TData>()
        {
            if (_queue.HasQueued<TData>())
            {
                _queueEvent.AddEvent(typeof(TData));
            }
        }
    }
}
