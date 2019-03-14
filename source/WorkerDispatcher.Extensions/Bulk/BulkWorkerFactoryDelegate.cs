using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Bulk
{
    public delegate IActionInvoker<BulkData<TData>> BulkWorkerFactoryDelegate<TData>();
}
