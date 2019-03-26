using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Batch
{
    public interface IBatchActionInvoker<TData> : IActionInvoker<TData[]>
    {
    }
}
