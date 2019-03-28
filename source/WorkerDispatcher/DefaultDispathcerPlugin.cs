//#define TRACE_STOP
using System;

namespace WorkerDispatcher
{
    internal class DefaultDispathcerPlugin : IDispatcherPlugin
    {
        private readonly IWorkerHandler _workerHandler;

        public IDispatcherTokenSender Sender { get; private set; }

        public DefaultDispathcerPlugin(IDispatcherTokenSender sender, IWorkerHandler workerHandler)
        {
            Sender = sender;
            _workerHandler = workerHandler;
        }

        public void LogFault(Exception exception)
        {
            _workerHandler.HandleFault(exception);
        }
    }
}