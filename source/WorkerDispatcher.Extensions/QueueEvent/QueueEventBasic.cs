using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher.Batch.QueueEvent
{
    internal class QueueEventBasic : IQueueEvent
    {
        private readonly ConcurrentQueue<object> _queue = new ConcurrentQueue<object>();
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        public bool AddEvent(object data, bool flush = false)
        {
            var type = (Type)data;

            _queue.Enqueue(data);
            _autoResetEvent.Set();

            return true;
        }

        public object WaitEvent(CancellationToken cancellationToken)
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

        private object TryDequeue()
        {
            if (_queue.TryDequeue(out object data))
            {
                return data;
            }

            return default(object);
        }
    }
}
