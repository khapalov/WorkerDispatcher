using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher.Batch.QueueEvent
{
    internal interface IQueueEvent : IDisposable
    {
        bool AddEvent(object data, bool flush = false);

        object WaitEvent(CancellationToken cancellationToken);
    }
}
