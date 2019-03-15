using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchQueueProvider
    {
        private readonly QueueEvent<Type> _queueEvent = new QueueEvent<Type>();
        private readonly IReadOnlyDictionary<Type, BatchConfig> _config;
        private readonly List<IScheduleTimer> _timers = new List<IScheduleTimer>();

        public BatchQueueProvider(IReadOnlyDictionary<Type, BatchConfig> config)
        {
            _config = config;
        }

        public void WaitEvent(CancellationToken cencellationToken)
        {
            var data = _queueEvent.WaitEvent(cencellationToken);            
        }

        public void StartTimers()
        {            
            foreach(var cfg in _config)
            {
                var time = new ScheduleTimer(cfg.Value.AwaitTimePeriod,cfg.Value.AwaitTimePeriod, true);
                _timers.Add(time);
            }
        }

        public void StopTimers()
        {
            foreach (var t in _timers)
            {
                t.Stop();
                t.Dispose();
            }
        }
    }
}
