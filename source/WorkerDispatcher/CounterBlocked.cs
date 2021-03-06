﻿using System.Threading;

namespace WorkerDispatcher
{
    internal class CounterBlocked : ICounterBlocked
    {
        private readonly ManualResetEvent _state = new ManualResetEvent(false);
        private volatile int _count = 0;

        public bool StopSignal { get; private set; } = false;

        public int Count => _count;

        public void Increment()
        {
            Interlocked.Increment(ref _count);
        }

        public void Decremenet()
        {
            Interlocked.Decrement(ref _count);
			if (StopSignal && _count == 0)
			{
				SetCompleted();
			}
		}

		public void Wait(int millisecondsTimeout)
		{
			StopSignal = true;

			if (_count == 0)
			{
				SetCompleted();
			}

			_state.WaitOne(millisecondsTimeout);
		}

		private void SetCompleted()
		{
			if (_state.SafeWaitHandle != null && !_state.SafeWaitHandle.IsClosed)
			{
				_state.Set();
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_state.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
