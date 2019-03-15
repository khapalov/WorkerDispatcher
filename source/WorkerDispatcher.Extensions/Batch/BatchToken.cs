using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher.Extensions.Batch
{
    internal class BatchToken : IBatchToken
    {
        private readonly CancellationTokenSource _tokenSource;

        public CancellationToken CancellationToken => _tokenSource.Token;

        public BatchToken(CancellationTokenSource cancellationTokenSource)
        {
            _tokenSource = cancellationTokenSource;
        }

        public void Send<TData>(TData data)
        { }

        public void Stop()
        {
            _tokenSource.Cancel();
        }

        public void Dispose()
        {
            
        }
    }
}
