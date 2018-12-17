//#define TRACE_STOP
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
    public interface IWorkerChain
    {
        IWorkerChain Post(IActionInvoker actionInvoker);

        IWorkerChain Post<TData>(IActionInvoker<TData> actionInvoker, TData data, TimeSpan lifetime);

        IWorkerChain Post<TData>(IActionInvoker<TData> actionInvoker, TData data);

        IWorkerChain Post(Func<CancellationToken, Task> fn);

        void Run(IActionInvoker<WorkerCompletedData> invoker);

        void Run(Action<WorkerCompletedData> fn);

        WorkerCompletedData RunSync();

        Task<WorkerCompletedData> RunAsync();
    }
}