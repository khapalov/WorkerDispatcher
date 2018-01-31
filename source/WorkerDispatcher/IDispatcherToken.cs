using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
    public interface IDispatcherToken
    {
        void Post(Func<CancellationToken, Task> fn);
        void Post(IActionInvoker actionInvoker);
        Task Stop(int delaySeconds = 30);
    }
}