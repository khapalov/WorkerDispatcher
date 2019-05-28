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
            
            var batchConfigProvider = new BatchConfigProvider(config);

            var queueProvider = new LocalQueueBuilder(batchConfigProvider).Build();

            var queueEvent = new QueueEvent<Type>();

            var batchProvider = new TimerQueueProvider(batchConfigProvider, queueEvent, queueProvider);

            return new BatchFactory(batchProvider, dispatcherPlugin, batchConfigProvider, queueEvent, queueProvider);
        }
    }
}
