using System;

namespace WorkerDispatcher
{
    public interface IDispatcherPlugin
    {
        IDispatcherTokenSender Sender { get; }

        void LogFault(Exception exception);
    }
}
