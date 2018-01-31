using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
    public interface IWorkerProcess<TMessage>
    {
        Task Process(TMessage message, CancellationToken cancelToken);
    }
}
