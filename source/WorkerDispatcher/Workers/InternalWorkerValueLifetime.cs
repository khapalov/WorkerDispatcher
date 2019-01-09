using System;

namespace WorkerDispatcher.Workers
{
	internal class InternalWorkerValueLifetime<TData> : InternalWorkerValue<TData>, IActionInvokerLifetime
	{
		public TimeSpan Lifetime { get; }

		public InternalWorkerValueLifetime(IActionInvoker<TData> actionInvoker, TData data, TimeSpan timeSpan) :
			base(actionInvoker, data)
		{
			Lifetime = timeSpan <= TimeSpan.Zero ? TimeSpan.FromMinutes(1) : timeSpan;
		}
	}

	internal interface IActionInvokerLifetime
	{
		TimeSpan Lifetime { get; }
	}
}
