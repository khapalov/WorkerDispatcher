using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
	internal class QueueBlocked<TData> : IDisposable
	{
		private readonly BlockingCollection<TData> _queue;
		private readonly AutoResetEvent _autoResetEvent;
		private readonly CancellationTokenSource _cancellationTokenSource;

		public QueueBlocked()
		{
			_queue = new BlockingCollection<TData>();
			_autoResetEvent = new AutoResetEvent(false);
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public int Count
		{
			get { return _queue.Count; }
		}

		public bool IsCompleted
		{
			get
			{
				return _queue.IsCompleted;
			}
		}

		public void Post(TData data)
		{
			_queue.Add(data);
			_autoResetEvent.Set();
		}

		public void Complete()
		{
			_queue.CompleteAdding();
			_cancellationTokenSource.Cancel();
		}

		public Task<TData> ReceiveAsync()
		{
			var task = new TaskCompletionSource<TData>();

			try
			{
				var cancellationToken = _cancellationTokenSource.Token;

				if (!_queue.TryTake(out TData data))
				{
					using (var reg = cancellationToken.Register(() => { if (!_autoResetEvent.SafeWaitHandle.IsClosed) _autoResetEvent.Set(); }))
					{
						do
						{
							if (!cancellationToken.IsCancellationRequested)
							{
								_autoResetEvent.WaitOne();
							}

							if (_queue.TryTake(out data)) break;


						} while (!cancellationToken.IsCancellationRequested);
					}
				}

				if (cancellationToken.IsCancellationRequested && data == null)
				{
					task.SetCanceled();
				}
				else
				{
					task.SetResult(data);
				}
			}
			catch (Exception ex)
			{
				task.TrySetException(ex);
			}

			return task.Task;
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_queue.CompleteAdding();
					_cancellationTokenSource.Cancel();
                    _autoResetEvent.Set();
                    _autoResetEvent.Dispose();
					_cancellationTokenSource.Dispose();
                    _queue.Dispose();
                }

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
