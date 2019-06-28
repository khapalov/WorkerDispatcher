using System;
using System.Timers;
using WorkerDispatcher.Batch.QueueEvent;

namespace WorkerDispatcher.Batch
{
    internal class ScheduleTimer : IScheduleTimer
    {
        private readonly Timer _timer;
        private readonly IQueueEvent _queue;
        private readonly Type _type;
        private readonly LocalQueueProvider _localQueueProvider;

        public ScheduleTimer(TimeSpan period, IQueueEvent queueEvent, Type type,LocalQueueProvider localQueueProvider,  bool start = false)
        {
            _timer = new Timer(period.TotalMilliseconds)
            {
                Enabled = start,
                AutoReset = true
            };

            _timer.Elapsed += TimerElapsed;
            _queue = queueEvent;
            _type = type;
            _localQueueProvider = localQueueProvider;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_localQueueProvider.HasQueued(_type))
            {
                _queue.AddEvent(_type);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
