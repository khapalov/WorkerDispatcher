using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorkerDispatcher;
using WorkerDispatcher.Extensions.Batch;

namespace BulkApp
{
    internal class BatchDataWorker<TData> : IActionInvoker<BatchData<TData>>
    {
        public Task<object> Invoke(BatchData<TData> data, CancellationToken token)
        {
            foreach (var d in data.Datas)
            {
                Console.WriteLine(d);
            }

            return Task.FromResult(new object());
        }
    }

    internal class BatchDataWorkerInt : IActionInvoker<BatchData<int>>
    {
        public Task<object> Invoke(BatchData<int> data, CancellationToken token)
        {
            foreach(var d in data.Datas)
            {
                Console.WriteLine(d);
            }
            return Task.FromResult(new object());
        }
    }

    internal class BatchDataWorkerString : IActionInvoker<BatchData<string>>
    {
        public Task<object> Invoke(BatchData<string> data, CancellationToken token)
        {
            foreach (var d in data.Datas)
            {
                Console.WriteLine(d);
            }

            return Task.FromResult(new object());
        }
    }
}
