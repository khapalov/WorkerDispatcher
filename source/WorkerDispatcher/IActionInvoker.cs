using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
    public interface IActionInvoker
    {
        Task<object> Invoke(CancellationToken token);
    }

	public interface IActionInvoker<TData>
	{
		Task<object> Invoke(TData data, CancellationToken token);
	}
}
