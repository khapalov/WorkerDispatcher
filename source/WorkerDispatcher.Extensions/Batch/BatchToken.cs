using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchToken : IBatchToken
    {
        private readonly ConcurrentDictionary<Type, ConcurrentQueue<object>> _queue;
        private readonly CancellationTokenSource _tokenSource;

        public CancellationToken CancellationToken => _tokenSource.Token;

        internal BatchToken(ConcurrentDictionary<Type, ConcurrentQueue<object>> queue, CancellationTokenSource cancellationTokenSource)
        {
            _queue = queue;
            _tokenSource = cancellationTokenSource;
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
            _tokenSource.Cancel();
        }

        public void Dispose()
        {
            
        }
    }
}
