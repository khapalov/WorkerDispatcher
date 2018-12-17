using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher
{
    internal class WorkerProgressReportSync : WorkerProgressReportBase, IDisposable
    {
        private readonly ManualResetEventSlim _manualResetEvent;

        private WorkerCompletedData _result;

        public WorkerProgressReportSync(
            IDispatcherTokenSender sender,
            WorkerProgressData[] datas) : base(datas)
        {
            _manualResetEvent = new ManualResetEventSlim(false);
        }

        protected override void OnComplete(WorkerCompletedData datas)
        {
            _result = datas;

            _manualResetEvent.Set();
        }

        public WorkerCompletedData WaitReady(CancellationToken cancellationToken)
        {
            _manualResetEvent.Wait(cancellationToken);

            return _result;
        }

        public WorkerCompletedData WaitReady()
        {
            return WaitReady(CancellationToken.None);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _manualResetEvent.Set();
                    _manualResetEvent.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }      

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
