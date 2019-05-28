using System;
using System.Collections.Concurrent;
using System.Threading;

namespace WorkerDispatcher.Batch
{
    internal class QueueEvent<TData> : IDisposable
    {
        private readonly ConcurrentQueue<TData> _queue = new ConcurrentQueue<TData>();

        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private readonly ConcurrentHashSet<TData> _concurrentHashSet = new ConcurrentHashSet<TData>();

        public QueueEvent()
        { }

        public void AddEvent(TData data, bool flush = false)
        {
            if (flush)
            {
                _queue.Enqueue(data);
                _autoResetEvent.Set();
            }
            else
            {
                if (_concurrentHashSet.Add(data))
                {
                    _queue.Enqueue(data);
                    _autoResetEvent.Set();
                }
            }
        }

        public TData WaitEvent(CancellationToken cancellationToken)
        {
            var data = TryDequeue();

            if (data != null)
                return data;

            using (cancellationToken.Register(() => _autoResetEvent.Set()))
            {
                data = TryDequeue();

                if (data != null)
                    return data;

                _autoResetEvent.WaitOne();

                return TryDequeue();
            }
        }

        public void Dispose()
        {
            _autoResetEvent.Set();
            _autoResetEvent.Dispose();
        }

        private TData TryDequeue()
        {
            if (_queue.TryDequeue(out TData data))
            {
                _concurrentHashSet.Remove(data);
                return data;
            }

            return default(TData);
        }
    }
}
