using System;
using System.Collections.Concurrent;
using System.Threading;

namespace WorkerDispatcher.Batch.QueueEvent
{
    internal class QueueEventUnique : IQueueEvent
    {        
        private readonly ConcurrentDictionary<Type, DateTime> _lastUpdateds = new ConcurrentDictionary<Type, DateTime>();

        private readonly BatchConfigProvider _config;
        private readonly IQueueEvent _queueEvent;

        public QueueEventUnique(BatchConfigProvider config, IQueueEvent queueEvent)
        {
            _config = config;
            _queueEvent = queueEvent;
        }

        public bool AddEvent(Type data, bool flush = false)
        {            
            var sendEvent = false;

            if (flush)
            {
                sendEvent = _queueEvent.AddEvent(data);
            }
            else
            {
                var cfg = _config.Get(data);

                if (cfg.TriggerCount <= 0)
                {
                    sendEvent = _queueEvent.AddEvent(data);
                }
                else
                {
                    var cur = DateTime.Now;

                    var upd = _lastUpdateds.GetOrAdd(data, p => cur);

                    var delta = cur - upd;

                    var isNew = delta == TimeSpan.Zero;                    

                    if (isNew || delta > cfg.AwaitTimePeriod)
                    {
                        if (isNew)
                        {
                            sendEvent = _queueEvent.AddEvent(data);
                        }
                        else
                        {
                            delta = DateTime.Now - upd;

                            if (delta <= cfg.AwaitTimePeriod)
                            {
                                sendEvent = _queueEvent.AddEvent(data);
                            }

                            _lastUpdateds.TryRemove(data, out _);
                        }
                    }
                }
            }

            return sendEvent;
        }

        public object WaitEvent(CancellationToken cancellationToken)
        {
            return _queueEvent.WaitEvent(cancellationToken);
        }

        public void Dispose()
        {
            _queueEvent.Dispose();
        }
    }
}
