using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    public interface IDispatcherPlugin
    {
        IDispatcherTokenSender Sender { get; }

        void LogFault(Exception exception);
    }
}
