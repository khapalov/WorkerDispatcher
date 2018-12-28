using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.Workers
{
    internal class InternalWorkerFunc : IActionInvoker
    {
        private readonly Func<CancellationToken, Task> _fn;

        public InternalWorkerFunc(Func<CancellationToken, Task> fn)
        {
            _fn = fn;
        }

        public async Task<object> Invoke(CancellationToken token)
        {
            await  _fn(token);

            return null;
        }

        public override string ToString()
        {
            return _fn.ToString();
        }
    }
}
