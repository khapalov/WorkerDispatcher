using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher
{
    public class CounterBlocked : ICounterBlocked
    {
        private int _count = 0;

        public int Count
        {
            get { return _count; }
        }

        public void Increment()
        {
            Interlocked.Increment(ref _count);
        }

        public void Decremenet()
        {
            Interlocked.Decrement(ref _count);
        }

    }
}
