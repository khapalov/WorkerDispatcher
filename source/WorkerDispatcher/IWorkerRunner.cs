using System.Threading.Tasks;

namespace WorkerDispatcher
{
    internal interface IWorkerRunner
    {
        Task ExcecuteInvoker(IActionInvoker actionInvoker);
    }
}