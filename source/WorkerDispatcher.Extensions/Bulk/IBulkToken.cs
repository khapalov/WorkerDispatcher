using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher.Extensions.Bulk
{
    public interface IBulkToken<TData> : IBulkSender<TData>, IDisposable
    {
        CancellationToken CancellationToken { get; }

        TData[] Flush();
    }
}
