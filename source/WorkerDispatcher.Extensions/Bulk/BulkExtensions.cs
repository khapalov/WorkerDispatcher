using System;
using System.Threading;

namespace WorkerDispatcher.Extensions.Bulk
{
    public static class BulkExtensions
    {
        public static IBulkToken<TData> CreateBulk<TData>(this IDispatcherToken dispatcherToken, BulkSetting bulkSettnig, IWorkerHandler workerHandler, BulkWorkerFactoryDelegate<TData> workerFactory)
        {
            var factory = new BulkFactory(workerHandler);

            var bulk = factory.Start(dispatcherToken, bulkSettnig, workerFactory);

            return bulk;
        }


    }
}
