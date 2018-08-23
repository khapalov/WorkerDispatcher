using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
    public interface IDispatcherToken
    {
        void Post(Func<CancellationToken, Task> fn);
        void Post(IActionInvoker actionInvoker);
		void Post<TData>(IActionInvoker<TData> actionInvoker, TData data);
		Task Stop(int delaySeconds = 30);
    }
}