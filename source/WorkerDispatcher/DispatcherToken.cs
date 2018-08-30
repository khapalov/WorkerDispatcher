using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace WorkerDispatcher
{
    public class DispatcherToken : IDisposable, IDispatcherToken
    {
        private readonly IQueueWorker _queueWorker;
        private readonly ICounterBlocked _processCount;
        private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly ActionDispatcherSettings _actionDispatcherSettings;

		internal DispatcherToken(ICounterBlocked counterProcess, 
			IQueueWorker queueWorker,
			ActionDispatcherSettings actionDispatcherSettings,
			CancellationTokenSource cancellationTokenSource)
        {
            _processCount = counterProcess;

            _queueWorker = queueWorker;

            _cancellationTokenSource = cancellationTokenSource;

			_actionDispatcherSettings = actionDispatcherSettings;
		}

		public int ProcessCount
		{
			get
			{
				return _processCount.Count;
			}
		}

		public int ProcessLimit
		{
			get
			{
				return _actionDispatcherSettings.PrefetchCount;
			}
		}

		public int QueueProcessCount
		{
			get
			{
				return _queueWorker.Count;
			}
		}


		public void Post(IActionInvoker actionInvoker)
        {
			if (actionInvoker == null)
			{
				throw new ArgumentNullException(nameof(actionInvoker));
			}

            _queueWorker.Post(actionInvoker);
        }

        public void Post(Func<CancellationToken, Task> fn)
        {
			if (fn == null)
			{
				throw new ArgumentNullException(nameof(fn));
			}

			_queueWorker.Post(new InternalWorker(fn));
        }

		public void Post<TData>(IActionInvoker<TData> actionInvoker, TData data)
		{
			if (actionInvoker == null)
			{
				throw new ArgumentNullException(nameof(actionInvoker));
			}

			_queueWorker.Post(new InternalWorkerValue<TData>(actionInvoker, data));
		}

		public void Post<TData>(IActionInvoker<TData> actionInvoker, TData data, TimeSpan lifetime)
		{
			if (actionInvoker == null)
			{
				throw new ArgumentNullException(nameof(actionInvoker));
			}

			_queueWorker.Post(new InternalWorkerValueLifetime<TData>(actionInvoker, data, lifetime));
		}

		public async Task Stop(int timeoutSeconds = 60)
        {
			await Task.Yield();

			WaitComplete(timeoutSeconds);
        }

		public void WaitComplete(int timeoutSeconds = 60)
		{
			_queueWorker.Complete();

			_cancellationTokenSource.Cancel();

			var timeout = timeoutSeconds * 1000;

			_queueWorker.WaitCompleted(timeout);

			//Debug.WriteLine("queue completed");

			_processCount.Wait(timeout);

			//Debug.WriteLine("process completed");
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cancellationTokenSource.Cancel();

                    ((IDisposable)_queueWorker).Dispose();

                    _cancellationTokenSource.Dispose();

					_processCount.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
		#endregion
	}
}
