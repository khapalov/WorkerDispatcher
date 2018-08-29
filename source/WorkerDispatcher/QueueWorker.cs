using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
	public class QueueWorker : IQueueWorker
	{
		private readonly QueueBlocked<IActionInvoker> _queue;
		private readonly ManualResetEvent _state = new ManualResetEvent(false);

		public QueueWorker()
		{
			_queue = new QueueBlocked<IActionInvoker>();

		}

		public int Count
		{
			get { return _queue.Count; }
		}

		public bool IsEmpty
		{
			get { return _queue.Count == 0; }
		}

		public void Post(IActionInvoker actionInvoker)
		{
			_queue.Post(actionInvoker);
		}

		public async Task<IActionInvoker> ReceiveAsync()
		{
			var actionInvoker = await _queue.ReceiveAsync().ContinueWith(p =>
			{
				if (_queue.IsCompleted)
				{
					OnCompleted();
				}

				return p;

			}, TaskContinuationOptions.OnlyOnRanToCompletion);

			return await actionInvoker;
		}

		protected virtual void OnCompleted()
		{
			if (_state.SafeWaitHandle != null && !_state.SafeWaitHandle.IsClosed)
			{
				_state.Set();
			}
		}

		public void Complete()
		{
			_queue.Complete();

			if (_queue.Count == 0)
			{
				OnCompleted();
			}			
		}

		public void WaitCompleted(int millisecondsTimeout)
		{
			_state.WaitOne(millisecondsTimeout);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					((IDisposable)_queue).Dispose();
					_state.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		void IDisposable.Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
