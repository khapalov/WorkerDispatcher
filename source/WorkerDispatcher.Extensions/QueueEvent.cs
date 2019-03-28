using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher.Extensions
{
    internal class QueueEvent<TData> : IDisposable
    {
        private readonly ConcurrentQueue<TData> _queue = new ConcurrentQueue<TData>();

        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        public void AddEvent(TData data)
        {
            _queue.Enqueue(data);
            _autoResetEvent.Set();
        }

        public TData WaitEvent(CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(() => _autoResetEvent.Set()))
            {
                if (_queue.TryDequeue(out TData data))
                {
                    return data;
                }

                _autoResetEvent.WaitOne();
                return _queue.TryDequeue(out data) ? data : default(TData);
            }          
        }

        public void Dispose()
        {
            _autoResetEvent.Set();
            _autoResetEvent.Dispose();
        }
    }
}
