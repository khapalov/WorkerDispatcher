using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchFactory : IBatchFactory
    {
        private readonly BatchQueueProvider _batchQueueProvider;
        private readonly IDispatcherTokenSender _sender;
        private readonly IReadOnlyDictionary<Type, BatchConfig> _config;

        public BatchFactory(BatchQueueProvider batchQueueProvider, 
            IDispatcherTokenSender sender, 
            IReadOnlyDictionary<Type, BatchConfig> config)
        {
            _batchQueueProvider = batchQueueProvider;
            _sender = sender;
            _config = config;
        }

        public IBatchToken Start()
        {
            BatchToken batchToken;

            _batchQueueProvider.StartTimers();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                batchToken = new BatchToken(cancellationTokenSource);

                var cancellationToken = cancellationTokenSource.Token;

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (!batchToken.CancellationToken.IsCancellationRequested)
                        {
                            _batchQueueProvider.WaitEvent(cancellationToken);

                            //_sender.Post(worker, new BulkData<TData>(datas));

                            //while (!bulkToken.CancellationToken.IsCancellationRequested)
                            //{
                            //    var datas = bulkToken.WaitData();

                            //    var worker = workerFactory();

                            //    dispatcherTokenSender.Post(worker, new BulkData<TData>(datas));
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        //_workerHandler.HandleFault(ex);
                    }
                });
            }

            return batchToken;
        }
    }
}
