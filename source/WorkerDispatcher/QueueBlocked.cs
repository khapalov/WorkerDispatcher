using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
	internal class QueueBlocked<TData> : IDisposable, IEnterQueuePost
    {
		private readonly ConcurrentQueue<TData> _queue;
		private readonly AutoResetEvent _autoResetEvent;
		private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ReaderWriterLockSlim _rwLock;

        private volatile bool _isCompleted = false;

		public QueueBlocked()
		{
			_queue = new ConcurrentQueue<TData>();
			_autoResetEvent = new AutoResetEvent(false);
			_cancellationTokenSource = new CancellationTokenSource();
            _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        public int Count => _queue.Count;

        public bool IsCompleted => _isCompleted && _queue.IsEmpty;

        public bool IsEmpty => _queue.IsEmpty;

        public void Post(TData data)
        {
            _rwLock.EnterReadLock();
            try
            {
                if (_isCompleted)
                {
                    throw new InvalidOperationException("QueueBlocked has been complete");
                }

                _queue.Enqueue(data);
                _autoResetEvent.Set();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public EnterTokenPost EnterPost()
        {
            _rwLock.EnterReadLock();

            if (_isCompleted)
            {
                _rwLock.ExitReadLock();
                throw new InvalidOperationException("QueueBlocked has been complete");
            }

            return new EnterTokenPost(_rwLock);
        }

        public void Complete()
        {
            if (_isCompleted) return;

            _rwLock.EnterWriteLock();
            try
            {
                _isCompleted = true;
                _cancellationTokenSource.Cancel();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

		public Task<TData> ReceiveAsync()
		{
			var task = new TaskCompletionSource<TData>();

			try
			{
				var cancellationToken = _cancellationTokenSource.Token;

				if (!_queue.TryDequeue(out TData data))
				{
					using (var reg = cancellationToken.Register(() => { if (!_autoResetEvent.SafeWaitHandle.IsClosed) _autoResetEvent.Set(); }))
					{
						do
						{
							if (!cancellationToken.IsCancellationRequested)
							{
								_autoResetEvent.WaitOne();
							}

							if (_queue.TryDequeue(out data)) break;


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
                    _isCompleted = true;
					_cancellationTokenSource.Cancel();
                    _autoResetEvent.Set();
                    _autoResetEvent.Dispose();
					_cancellationTokenSource.Dispose();
                    _rwLock.Dispose();
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

    internal interface IEnterQueuePost
    {
        EnterTokenPost EnterPost();
    }

    internal class EnterTokenPost : IDisposable
    {
        private readonly ReaderWriterLockSlim _readerWriterLockSlim;

        public EnterTokenPost(ReaderWriterLockSlim readerWriterLockSlim)
        {
            _readerWriterLockSlim = readerWriterLockSlim;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls        

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _readerWriterLockSlim.ExitReadLock();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
