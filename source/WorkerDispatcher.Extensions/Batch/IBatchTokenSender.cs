using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Extensions.Batch
{
    public interface IBatchTokenSender
    {
        void Send<TData>(TData data);

        void Flush<TData>();
    }
}
