using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
    public interface IDispatcherTokenSender
    {
		int ProcessCount { get; }

		void Post(Func<CancellationToken, Task> fn);
		void Post(IActionInvoker actionInvoker);
		void Post<TData>(IActionInvoker<TData> actionInvoker, TData data);
		void Post<TData>(IActionInvoker<TData> actionInvoker, TData data, TimeSpan lifetime);
	}
}
