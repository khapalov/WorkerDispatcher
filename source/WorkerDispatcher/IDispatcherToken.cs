using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
	public interface IDispatcherToken : IDispatcherTokenSender, IDisposable
	{
        IDispatcherPlugin Plugin { get; }

		Task Stop(int timeoutSeconds = 60);
		void WaitCompleted(int timeoutSeconds = 60);
	}
}