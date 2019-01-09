using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    internal interface IQueueWorker : IQueueWorkerWriter, IQueueWorkerReceiver, IDisposable, IWorkerNotify
    {
        int Count { get; }

        bool IsEmpty { get; }
    }

    internal interface IWorkerNotify
    {
        void SetWorkerEnd();
    }
}
