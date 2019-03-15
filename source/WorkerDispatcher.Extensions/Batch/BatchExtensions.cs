using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Batch
{
    public static class BatchExtensions
    {
        public static IBatchFactory Batch(this IDispatcherToken dispatcherToken, Action<IBatchQueueBuilder> action)
        {
            var config = new Dictionary<Type, BatchConfig>();

            var builder = new BatchQueueBuilder(config);

            action(builder);            

            var batchProvider = new BatchQueueProvider(config);

            return new BatchFactory(batchProvider, dispatcherToken, config);
        }
    }
}
