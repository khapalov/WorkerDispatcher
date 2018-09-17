using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    public interface ICounterBlocked : ICounterBlockedReader, IDisposable
    {
        void Decremenet();
        void Increment();
		void Wait(int millisecondsTimeout);
	}
}
