using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher.Extensions.Bulk
{
    public class BulkToken<TData> : IBulkToken<TData>
    {
        private readonly ConcurrentQueue<TData> _datas = new ConcurrentQueue<TData>();

        private readonly AutoResetEvent _eventSlim = new AutoResetEvent(true);
        private readonly BulkSetting _bulkSetting;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly int _maxCount;

        public BulkToken(IDispatcherTokenSender dispatcherTokenSender, BulkSetting bulkSetting)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _bulkSetting = bulkSetting;
            _maxCount = _bulkSetting.MaxCount <= 0 ? 1 : _bulkSetting.MaxCount;
            CancellationToken = _cancellationTokenSource.Token;
            CancellationToken.Register(() => _eventSlim.Set());
        }

        public CancellationToken CancellationToken { get; }

        public TData[] WaitData()
        {
            var queueCount = 0;

            do
            {                
                _eventSlim.WaitOne(5000);

                queueCount = _datas.Count;

            } while (queueCount > 0 || CancellationToken.IsCancellationRequested);

            if (queueCount == 0)
            {
                return Array.Empty<TData>();
            }

            var cnt = queueCount > _maxCount ? _maxCount : queueCount;

            var ready = (TData[])Array.CreateInstance(typeof(TData), cnt);

            for (int i = 0; i < cnt; i++)
            {
                if (!_datas.TryDequeue(out TData data)) break;

                ready[i] = data;
            }

            return ready;
        }

        public TData[] Flush()
        {
            
            return _datas.ToArray();
        }

        public void Send(TData data)
        {
            _datas.Enqueue(data);

            if (_datas.Count >= _maxCount)
            {
                _eventSlim.Set();
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
                    
                    _eventSlim.Set();
                    _eventSlim.Dispose();

                    _cancellationTokenSource.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BulkToken() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
