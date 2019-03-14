using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions
{
    public class ScheduleTimerProvider
    {
        public HashSet<IScheduleTimer> _scheduleTimers = new HashSet<IScheduleTimer>();

        public void Add(IScheduleTimer scheduleTimer)
        {
            _scheduleTimers.Add(scheduleTimer);
        }

        public void Start()
        {
            foreach (var item in _scheduleTimers)
            {
                item.Start();
            }           
        }

        public void Stop()
        {
            foreach (var item in _scheduleTimers)
            {
                item.Stop();
                item.Dispose();
            }
        }
    }
}
