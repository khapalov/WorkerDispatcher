using System;

namespace WorkerDispatcher.Batch
{
    public interface IScheduleTimer: IDisposable
    {
        void Start();

        void Stop();
    }
}