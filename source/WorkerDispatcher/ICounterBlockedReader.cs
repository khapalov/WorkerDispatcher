using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    public interface ICounterBlockedReader
    {
        bool StopSignal { get; }
        int Count { get; }
    }
}
