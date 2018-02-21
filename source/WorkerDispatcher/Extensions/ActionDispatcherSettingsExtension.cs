using System;
using System.Collections.Generic;
using System.Text;
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
            switch (settings.Schedule)
            {
                case ScheduleType.Parallel:
                    return new ScheduleParallel(queue, processCount, settings.Timeout, workerHandler);
                case ScheduleType.Sequenced:
                    return new ScheduleConsistent(queue, processCount, settings.Timeout, workerHandler);
            }

            throw new ArgumentException("Schedule not found for " + settings.Schedule);
        }
    }
}
