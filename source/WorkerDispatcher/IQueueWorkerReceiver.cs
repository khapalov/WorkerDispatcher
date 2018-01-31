using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
    public interface IQueueWorkerReceiver
    {
        Task<IActionInvoker> ReceiveAsync();
    }
}
