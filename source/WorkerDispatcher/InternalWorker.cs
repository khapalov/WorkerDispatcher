using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
    internal class InternalWorker : IActionInvoker
    {
        private readonly Func<CancellationToken, Task> _fn;

        public InternalWorker(Func<CancellationToken, Task> fn)
        {
            _fn = fn;
        }

        public async Task<object> Invoke(CancellationToken token)
        {
            await  _fn(token);

            return null;
        }
    }
}
