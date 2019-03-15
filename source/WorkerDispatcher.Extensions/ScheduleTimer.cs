﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace WorkerDispatcher.Extensions
{
    internal class ScheduleTimer : IScheduleTimer
    {
        private readonly Timer _timer;
        private readonly QueueEvent<Type> _queue;
        private readonly Type _type;

        public ScheduleTimer(TimeSpan period, QueueEvent<Type> queue, Type type, bool start = false)
        {
            _timer = new Timer(period.TotalMilliseconds)
            {
                Enabled = start,
                AutoReset = true
            };
            
            _timer.Elapsed += TimerElapsed;
            _queue = queue;
            _type = type;
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
            _queue.AddEvent(_type);            
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