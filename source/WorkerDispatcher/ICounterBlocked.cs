using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    public interface ICounterBlocked : ICounterBlockedReader
    {
        void Decremenet();
        void Increment();
    }
}
