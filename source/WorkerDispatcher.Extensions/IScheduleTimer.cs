using System;

namespace WorkerDispatcher.Extensions
{
    public interface IScheduleTimer: IDisposable
    {
        void Start();

        void Stop();
    }
}