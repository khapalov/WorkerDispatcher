using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    public interface IQueueWorkerWriter
    {
        void Post(IActionInvoker actionInvoker);

        void PostBulk(IActionInvoker[] actionInvokers);

        void Complete();
    }
}
