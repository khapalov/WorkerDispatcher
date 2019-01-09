using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
	internal class QueueWorker : IQueueWorker
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
			get { return _queue.IsEmpty; }
		}

		public void Post(IActionInvoker actionInvoker)
		{
			_queue.Post(actionInvoker);
		}

        public void PostBulk(IActionInvoker[] actionInvokers)
        {
            if(actionInvokers == null)
            {
                throw new ArgumentNullException($"nameof{actionInvokers}");
            }

            using (var enterPost = _queue.EnterPost())
            {
                foreach (var item in actionInvokers)
                {
                    Post(item);
                }
            }
        }

		public async Task<IActionInvoker> ReceiveAsync()
		{
            return await _queue.ReceiveAsync();
        }

        void IWorkerNotify.SetWorkerEnd()
        {
            if (_queue.IsCompleted && _queue.IsEmpty)
            {
                SetCompleted();
            }
        }

		public void Complete()
		{
			_queue.Complete();

			if (_queue.IsEmpty)
			{
				SetCompleted();
			}			
		}

		private void SetCompleted()
		{
			if (_state.SafeWaitHandle != null && !_state.SafeWaitHandle.IsClosed)
			{
				_state.Set();
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
