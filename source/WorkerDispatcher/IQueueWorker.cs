using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    public interface IQueueWorker : IQueueWorkerWriter, IQueueWorkerReceiver, IDisposable
    {
        int Count { get; }

        bool IsEmpty { get; }
    }
}
