using System;

namespace WorkerDispatcher
{
    internal class CounterBlockedStrong : ICounterBlocked
    {
        private readonly ICounterBlocked _counterBlocked;

        public CounterBlockedStrong(ICounterBlocked counterBlocked)
        {
            _counterBlocked = counterBlocked;
        }

        public int Count => _counterBlocked.Count;

        public bool StopSignal => _counterBlocked.StopSignal;

        public void Increment()
        {
            if (StopSignal)
            {
                throw new InvalidOperationException("Impossible to increase the counter");
            }

            _counterBlocked.Increment();

            if (StopSignal)
            {
                _counterBlocked.Decremenet();
                throw new InvalidOperationException("Impossible to increase the counter");
            }
        }

        public void Decremenet()
        {
            _counterBlocked.Decremenet();
        }

        public void Dispose()
        {
            _counterBlocked.Dispose();
        }

        public void Wait(int millisecondsTimeout)
        {
            _counterBlocked.Wait(millisecondsTimeout);
        }
    }
}
