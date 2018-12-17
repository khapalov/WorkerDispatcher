using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
	internal class InternalWorkerValue<TData> : IActionInvoker
	{
		private readonly TData _data;
		private readonly IActionInvoker<TData> _actionInvoker;

		public InternalWorkerValue(IActionInvoker<TData> actionInvoker, TData data)
		{
			_data = data;
			_actionInvoker = actionInvoker;
		}

		public virtual async Task<object> Invoke(CancellationToken token)
		{
			return await _actionInvoker.Invoke(_data, token);
		}
	}
}
