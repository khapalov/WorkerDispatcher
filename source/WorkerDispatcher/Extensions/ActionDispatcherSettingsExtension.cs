using System;
using WorkerDispatcher.ScheduleStrategies;

namespace WorkerDispatcher.Extensions
{
    internal static class ActionDispatcherSettingsExtension
    {
        public static IScheduleStrategies BuildSchedule(this ActionDispatcherSettings settings,
            IQueueWorker queue,
            ICounterBlocked processCount,
            IWorkerHandler workerHandler)
        {
            var timeout = settings.Timeout == TimeSpan.MinValue ? TimeSpan.FromMinutes(1) : settings.Timeout;

            switch (settings.Schedule)
            {
                case ScheduleType.Parallel:
                    return new ScheduleParallel(queue, processCount, timeout, workerHandler);
                case ScheduleType.Sequenced:
                    return new ScheduleConsistent(queue, processCount, timeout, workerHandler);
                case ScheduleType.ParallelLimit:
                    return new ScheduleParallelLimit(queue, processCount, timeout, settings.PrefetchCount, workerHandler);
            }

            throw new ArgumentException("Schedule not found for " + settings.Schedule);
        }
    }
}
