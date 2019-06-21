using System;
using System.Collections.Concurrent;
using System.Threading;

namespace WorkerDispatcher.Batch
{
    internal class QueueEvent<TData> : IDisposable
    {
        private readonly ConcurrentQueue<object> _queue = new ConcurrentQueue<object>();
        private readonly ConcurrentDictionary<object, DateTime> _lastUpdateds = new ConcurrentDictionary<object, DateTime>();

        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private readonly BatchConfigProvider _config;

        public QueueEvent(BatchConfigProvider config)
        {
            _config = config;
        }
        public void AddEvent(object data, bool flush = false)
        {
            var type = (Type)data;

            if (flush)
            {
                _queue.Enqueue(data);
                _autoResetEvent.Set();
            }
            else
            {
                var cur = DateTime.Now;

                var upd = _lastUpdateds.GetOrAdd(type, p => cur);
                
                var delta = cur - upd;

                var isNew = delta == TimeSpan.Zero;

                var cfg = _config.Get(type);

                if (isNew || delta > cfg.AwaitTimePeriod)
                {
                    if (isNew)
                    {
                        _queue.Enqueue(data);
                        _autoResetEvent.Set();
                    }
                    else
                    {                        
                        delta = DateTime.Now - upd;

                        if (delta <= cfg.AwaitTimePeriod)
                        {
                            _queue.Enqueue(data);
                            _autoResetEvent.Set();
                        }

                        _lastUpdateds.TryRemove(type, out _);
                    }
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
            if (_queue.TryDequeue(out object data))
            {
                return (TData)data;
            }

            return default(TData);
        }
    }
}
