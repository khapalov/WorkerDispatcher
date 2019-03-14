using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Batch
{
    public static class BatchExtensions
    {
        public static IBatchFactory Batch(this IDispatcherToken dispatcherToken, Action<IBatchQueueBuilder> action)
        {
            var builder = new BatchQueueBuilder();

            action(builder);
            
            var batchProvider = new BatchQueueProvider(new BatchConfig());

            return new BatchFactory(batchProvider);
        }
    }
}
