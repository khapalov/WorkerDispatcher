//#define TRACE_STOP
using System;

namespace WorkerDispatcher
{
    public interface IWorkerChain
    {
        IWorkerChain Post(IActionInvoker actionInvoker);

        IWorkerChain Post<TData>(IActionInvoker<TData> actionInvoker, TData data, TimeSpan lifetime);

        IWorkerChain Post<TData>(IActionInvoker<TData> actionInvoker, TData data);

        void Run(IActionInvoker<WorkerCompletedData> invoker);

        void Run(Action<WorkerCompletedData> fn);

        WorkerCompletedData RunSync();
    }
}