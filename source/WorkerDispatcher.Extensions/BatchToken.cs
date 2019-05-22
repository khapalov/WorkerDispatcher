using System;
using System.Collections.Concurrent;
using System.Threading;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchToken : IBatchToken
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly LocalQueueManager _queue;
        private readonly TimerQueueProvider _timerQueueProvider;
        private readonly QueueEvent<Type> _queueEvent;
        private readonly ManualResetEventSlim _manualResetEventSlim;

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        internal BatchToken(LocalQueueManager queue, TimerQueueProvider timeQueueProvider, QueueEvent<Type> queueEvent, ManualResetEventSlim manualResetEventSlim)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _queue = queue;
            _timerQueueProvider = timeQueueProvider;
            _queueEvent = queueEvent;
            _manualResetEventSlim = manualResetEventSlim;
        }

        public void Send<TData>(TData data)
        {
            _queue.Enqueue(data);
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
                _manualResetEventSlim.Wait();
            }
            else
            {
                _manualResetEventSlim.Wait(awaitTime);
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
                _queueEvent.AddEvent(typeof(TData));
            }
        }
    }
}
