using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchFactory : IBatchFactory
    {
        private readonly BatchQueueProvider _batchQueueProvider;

        public BatchFactory(BatchQueueProvider batchQueueProvider)
        {
            _batchQueueProvider = batchQueueProvider;
        }

        public IBatchToken Start()
        {            
            var batchToken = new BatchToken();
            
            Task.Factory.StartNew(() =>
            {

            });


            return batchToken;
        }
    }
}
