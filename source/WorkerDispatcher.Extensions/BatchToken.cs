using System;
using System.Collections.Concurrent;
using System.Threading;
using WorkerDispatcher.Batch.QueueEvent;

namespace WorkerDispatcher.Batch
{
    internal class BatchToken : IBatchToken
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly LocalQueueProvider _queue;
        private readonly TimerQueueProvider _timerQueueProvider;
        private readonly IQueueEvent _queueEvent;
        private readonly ManualResetEventSlim _triggerAllCompleted;
        private readonly BatchConfigProvider _batchConfig;

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        internal BatchToken(LocalQueueProvider queue, 
            TimerQueueProvider timeQueueProvider,
            IQueueEvent queueEvent, 
            ManualResetEventSlim manualResetEventSlim,
            BatchConfigProvider batchConfig)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _queue = queue;
            _timerQueueProvider = timeQueueProvider;
            _queueEvent = queueEvent;
            _triggerAllCompleted = manualResetEventSlim;
            _batchConfig = batchConfig;
        }

        public int Send<TData>(TData data)
        {
            var triggerCount = _batchConfig.Get<TData>().TriggerCount;

            var queueCount = _queue.Enqueue(data);

            if (triggerCount > 0)
            {
                var res = triggerCount - queueCount;

                if (res <= 0)
                {
                    _queueEvent.AddEvent(data.GetType());                    
                }
            }

            return queueCount;
        }

        public void Stop()
        {
            Stop(TimeSpan.Zero);
        }

        public void Stop(TimeSpan awaitTime)
        {
            _cancellationTokenSource.Cancel();
            if (awaitTime == TimeSpan.Zero)
            {
                _triggerAllCompleted.Wait();
            }
            else
            {
                _triggerAllCompleted.Wait(awaitTime);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _timerQueueProvider.Dispose();
            _cancellationTokenSource.Dispose();
        }

        public void Flush<TData>()
        {
            if (_queue.HasQueued<TData>())
            {
                _queueEvent.AddEvent(typeof(TData), true);
            }
        }
    }
}
