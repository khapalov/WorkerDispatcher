using System;

namespace WorkerDispatcher
{
    public class ActionDispatcherSettings
    {
        public ActionDispatcherSettings()
        {
            PrefetchCount = 4;
            Timeout = TimeSpan.FromMinutes(1);
            Schedule = ScheduleType.Parallel;
        }

        public int PrefetchCount { get; set; }
        public TimeSpan Timeout { get; set; }
        public ScheduleType Schedule { get; set; }
    }
}
