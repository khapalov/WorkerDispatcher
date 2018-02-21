using System.Threading;

namespace WorkerDispatcher.ScheduleStrategies
{
    internal interface IScheduleStrategies
    {
        void Start(CancellationToken cancellationToken);
    }
}