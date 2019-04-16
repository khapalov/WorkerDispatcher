using System;
using System.Collections.Generic;

namespace WorkerDispatcher.Extensions.Batch
{
    public static class BatchExtensions
    {
        public static IBatchFactory Batch(this IDispatcherPlugin dispatcherPlugin, Action<IBatchQueueBuilder> action)
        {
            var config = new Dictionary<Type, BatchConfig>();

            var builder = new BatchQueueBuilder(config);

            action(builder);

            var queueEvent = new QueueEvent<Type>();

            var batchProvider = new BatchQueueProvider(config, queueEvent);

            var queueManager = new LocalQueueBuilder(config).Build();

            return new BatchFactory(batchProvider, dispatcherPlugin, config, queueEvent, queueManager);
        }
    }
}
