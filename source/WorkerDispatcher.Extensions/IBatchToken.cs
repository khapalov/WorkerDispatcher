using System;
using System.Threading;

namespace WorkerDispatcher.Batch
{
    public interface IBatchToken : IBatchTokenSender, IDisposable
    {
        CancellationToken CancellationToken { get; }

        void Stop();

        void Stop(TimeSpan awaitTime);
    }
}