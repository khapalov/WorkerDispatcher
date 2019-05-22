using System;
using System.Collections.Concurrent;
using System.Threading;

namespace WorkerDispatcher.Batch
{
    internal class QueueEvent<TData> : IDisposable
    {
        private readonly ConcurrentQueue<TData> _queue = new ConcurrentQueue<TData>();
        private readonly ConcurrentDictionary<TData, DateTime> _latUpdateds = new ConcurrentDictionary<TData, DateTime>();

        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        public void AddEvent(TData data, bool flush = false)
        {
            if (flush)
            {
                _queue.Enqueue(data);
                _autoResetEvent.Set();
            }
            else
            {
                var isNew = !_latUpdateds.ContainsKey(data);

                var upd = _latUpdateds.GetOrAdd(data, p => DateTime.Now);
                var cur = DateTime.Now;

                var delta = cur- upd;

                if (isNew || delta.TotalMilliseconds > 1000)
                {
                    //Console.WriteLine($"Delta {delta.TotalMilliseconds}");
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
                _latUpdateds.TryRemove(data, out _);
            }

            return data;
        }
    }
}
