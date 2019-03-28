using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchToken : IBatchToken
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentDictionary<Type, ConcurrentQueue<object>> _queue;
        private readonly BatchQueueProvider _queueProvider;
        private readonly QueueEvent<Type> _queueEvent;

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        internal BatchToken(ConcurrentDictionary<Type, ConcurrentQueue<object>> queue, BatchQueueProvider queueProvider, QueueEvent<Type> queueEvent)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _queue = queue;
            _queueProvider = queueProvider;
            _queueEvent = queueEvent;
        }

        public void Send<TData>(TData data)
        {
            if (_queue.TryGetValue(typeof(TData), out ConcurrentQueue<object> q))
            {                
                q.Enqueue(data);
            }
            else
            {
                throw new ArgumentException($"No registered type {typeof(TData)}");
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _queueProvider.Dispose();
        }

        public void Flush<TData>()
        {
            _queueEvent.AddEvent(typeof(TData));
        }
    }
}
