using System;
using System.Collections.Generic;
using System.Linq;
using WorkerDispatcher.Batch.QueueEvent;

namespace WorkerDispatcher.Batch
{
    public static class BatchExtensions
    {
        public static IBatchFactory Batch(this IDispatcherPlugin dispatcherPlugin, Action<IBatchQueueBuilder> action)
        {
            var config = new Dictionary<Type, BatchConfig>();

            var builder = new BatchQueueBuilder(config);

            action(builder);

            var batchConfigProvider = new BatchConfigProvider(config);

            var queueProvider = new LocalQueueBuilder(batchConfigProvider).Build();

            IQueueEvent queueEvent = new QueueEventBasic();

            if (batchConfigProvider.Find(p => p.Value.TriggerCount > 0).Any())
            {
                queueEvent = new QueueEventUnique(batchConfigProvider, queueEvent);
            }

            var batchProvider = new TimerQueueProvider(batchConfigProvider, queueEvent, queueProvider);

            return new BatchFactory(batchProvider, dispatcherPlugin, batchConfigProvider, queueEvent, queueProvider);
        }
    }
}
