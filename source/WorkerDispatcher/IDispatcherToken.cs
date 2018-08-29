using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
	public interface IDispatcherToken : IDispatcherTokenSender
	{
		Task Stop(int timeoutSeconds = 30);
		void WaitComplete(int timeoutSeconds = 30);
	}
}