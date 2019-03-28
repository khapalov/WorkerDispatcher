using System;
using System.Collections.Generic;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchQueueProvider : IDisposable
    {
        private readonly QueueEvent<Type> _queueEvent;
        private readonly IReadOnlyDictionary<Type, BatchConfig> _config;
        private readonly List<IScheduleTimer> _timers = new List<IScheduleTimer>();

        public BatchQueueProvider(IReadOnlyDictionary<Type, BatchConfig> config, QueueEvent<Type> queueEvent)
        {
            _config = config;
            _queueEvent = queueEvent;
        }

        public void StartTimers()
        {            
            foreach(var cfg in _config)
            {
                var time = new ScheduleTimer(cfg.Value.AwaitTimePeriod, _queueEvent, cfg.Key, true);
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

        public void Dispose()
        {
            StopTimers();
        }
    }
}
