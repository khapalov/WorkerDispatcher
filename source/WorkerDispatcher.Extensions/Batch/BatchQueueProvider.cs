using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchQueueProvider
    {
        private readonly QueueEvent<Type> _queueEvent = new QueueEvent<Type>();
        private readonly BatchConfig _batchConfig;

        public BatchQueueProvider(BatchConfig batchConfig)
        {
            _batchConfig = batchConfig;
        }

        public void WaitEvent(CancellationToken cencellationToken)
        {
            var data = _queueEvent.WaitEvent(cencellationToken);            
        }
    }
}
