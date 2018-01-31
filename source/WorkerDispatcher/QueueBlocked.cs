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

        public void Post(TData data)
        {
            _queue.Add(data);
            _autoResetEvent.Set();
        }

        public async Task<TData> ReceiveAsync()
        {
            return await Task.Run(() => ReceiveData()).ConfigureAwait(false);
        }

        public void Complete()
        {
            _queue.CompleteAdding();
            _cancellationTokenSource.Cancel();
        }

        private TData ReceiveData()
        {
            var data = default(TData);

            var cancellationToken = _cancellationTokenSource.Token;

            if (!_queue.TryTake(out data))
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
                cancellationToken.ThrowIfCancellationRequested();
            }

            return data;
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
                    _autoResetEvent.Dispose();
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
