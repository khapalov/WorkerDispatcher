using System.Threading.Tasks;

namespace WorkerDispatcher
{
    public interface IQueueWorkerReceiver
    {
        Task<IActionInvoker> ReceiveAsync();

		void WaitCompleted(int millisecondsTimeout);
    }
}
