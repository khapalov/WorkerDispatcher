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
        private readonly IQueueWorkerWriter _queueWorker;
        private readonly ICounterBlockedReader _processCount;
        private readonly CancellationTokenSource _cancellationTokenSource;

        internal DispatcherToken(ICounterBlockedReader counterProcess, IQueueWorkerWriter queueWorker, CancellationTokenSource cancellationTokenSource)
        {
            _processCount = counterProcess;

            _queueWorker = queueWorker;

            _cancellationTokenSource = cancellationTokenSource;
        }

        public void Post(IActionInvoker actionInvoker)
        {
			if (actionInvoker == null)
			{
				return;
			}

            _queueWorker.Post(actionInvoker);
        }

        public void Post(Func<CancellationToken, Task> fn)
        {
			if (fn == null)
			{
				return;
			}

			_queueWorker.Post(new InternalWorker(fn));
        }

		public void Post<TData>(IActionInvoker<TData> actionInvoker, TData data)
		{
			if (actionInvoker == null)
			{
				return;
			}

			_queueWorker.Post(new InternalWorkerValue<TData>(actionInvoker, data));
		}

		public void Post<TData>(IActionInvoker<TData> actionInvoker, TData data, TimeSpan lifetime)
		{
			if (actionInvoker == null)
			{
				return;
			}

			_queueWorker.Post(new InternalWorkerValueLifetime<TData>(actionInvoker, data, lifetime));
		}

		public async Task Stop(int delaySeconds = 60)
        {
            _queueWorker.Complete();

            _cancellationTokenSource.Cancel();

            using (var tokenSource = new CancellationTokenSource())
            {
                tokenSource.CancelAfter(delaySeconds * 1000);

                do
                {
#if DEBUG
                    Trace.WriteLine(String.Format("Wating count process = {0}", _processCount.Count));
#endif
                    if (_processCount.Count == 0) break;

                    await Task.Delay(1, tokenSource.Token);

                } while (true);
            }
                
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
