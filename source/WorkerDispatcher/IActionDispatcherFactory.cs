using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    public interface IActionDispatcherFactory
    {
        IDispatcherToken Start(ActionDispatcherSettings config);
    }
}
