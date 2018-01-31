using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    public interface IWorkerHandler
    {
        void HandleError(Exception ex, decimal duration, bool isCancel);
        void HandleFault(Exception ex);
        void HandleResult(object result, decimal duration);
    }
}
