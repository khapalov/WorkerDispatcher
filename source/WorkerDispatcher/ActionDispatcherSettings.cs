using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    public class ActionDispatcherSettings
    {        
        public int PrefetchCount { get; set; }
        public TimeSpan Timeout { get; set; }
        public ScheduleType Schedule { get; set; }
    }
}
