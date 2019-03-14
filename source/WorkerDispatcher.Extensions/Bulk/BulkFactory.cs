using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.Extensions.Bulk
{
    internal class BulkFactory
    {
        private readonly IWorkerHandler _workerHandler;

        internal BulkFactory(IWorkerHandler workerHandler)
        {
            _workerHandler = workerHandler;
        }

        public BulkToken<TData> Start<TData>(IDispatcherTokenSender dispatcherTokenSender, BulkSetting bulkSettnig, BulkWorkerFactoryDelegate<TData> workerFactory)
        {            
            var bulkToken = new BulkToken<TData>(dispatcherTokenSender, bulkSettnig);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (!bulkToken.CancellationToken.IsCancellationRequested)
                    {
                        var datas = bulkToken.WaitData();

                        var worker = workerFactory();

                        dispatcherTokenSender.Post(worker, new BulkData<TData>(datas));
                    }
                }
                catch (Exception ex)
                {
                    _workerHandler.HandleFault(ex);
                }               

            }, TaskCreationOptions.LongRunning);


            return bulkToken;
        }
    }
}
