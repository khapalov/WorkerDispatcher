using System;
using System.Collections.Generic;

namespace WorkerDispatcher.Batch
{
    internal class TimerQueueProvider : IDisposable
    {
        private readonly QueueEvent<Type> _queueEvent;
        private readonly LocalQueueProvider _localQueueProvider;
        private readonly BatchConfigProvider _config;
        private readonly List<IScheduleTimer> _timers = new List<IScheduleTimer>();

        public TimerQueueProvider(BatchConfigProvider config, QueueEvent<Type> queueEvent, LocalQueueProvider localQueueProvider)
        {
            _config = config;
            _queueEvent = queueEvent;
            _localQueueProvider = localQueueProvider;
        }

        public void StartTimers()
        {
            StopTimers();

            foreach(var cfg in _config.GetAll())
            {
                var time = new ScheduleTimer(cfg.Value.AwaitTimePeriod, _queueEvent, cfg.Key, _localQueueProvider, true);
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
            _timers.Clear();
        }

        public void Dispose()
        {
            StopTimers();
        }
    }
}
