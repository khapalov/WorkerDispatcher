using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    public interface ICounterBlockedReader
    {
        int Count { get; }
    }
}
