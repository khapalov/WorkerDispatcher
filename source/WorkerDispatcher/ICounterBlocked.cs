using System;

namespace WorkerDispatcher
{
    public interface ICounterBlocked : ICounterBlockedReader, IDisposable
    {
        void Decremenet();
        void Increment();
		void Wait(int millisecondsTimeout);
	}
}
