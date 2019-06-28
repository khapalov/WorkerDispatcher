using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.Workers
{
    public class InternalWorkerDataSync<TData> : IActionInvoker<TData>
    {
        private readonly IActionInvoker<TData> _actionInvoker;
        private readonly ICounterBlocked _counterBlocked;

        public InternalWorkerDataSync(IActionInvoker<TData> actionInvoker, ICounterBlocked counterBlocked)
        {
            counterBlocked.Increment();

            _actionInvoker = actionInvoker;
            _counterBlocked = counterBlocked;            
        }

        public async Task<object> Invoke(TData data, CancellationToken token)
        {
            try
            {
                var result = await _actionInvoker.Invoke(data, token);

                return result;
            }
            finally
            {
                _counterBlocked.Decremenet();
            }
        }

        public override string ToString()
        {
            return _actionInvoker.ToString();
        }
    }
}
