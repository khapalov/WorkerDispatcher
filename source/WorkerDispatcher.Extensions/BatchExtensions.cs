using System;
using System.Collections.Generic;

namespace WorkerDispatcher.Batch
{
    public static class BatchExtensions
    {
        public static IBatchFactory Batch(this IDispatcherPlugin dispatcherPlugin, Action<IBatchQueueBuilder> action)
        {
            var config = new Dictionary<Type, BatchConfig>();

            var builder = new BatchQueueBuilder(config);

            action(builder);

            var queueEvent = new QueueEvent<Type>();

            var batchConfigProvider = new BatchConfigProvider(config);

            var batchProvider = new TimerQueueProvider(batchConfigProvider, queueEvent);

            var queueManager = new LocalQueueBuilder(batchConfigProvider).Build();

            return new BatchFactory(batchProvider, dispatcherPlugin, batchConfigProvider, queueEvent, queueManager);
        }
    }
}
