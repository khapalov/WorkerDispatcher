using System.Threading;
using WorkerDispatcher.Extensions;

namespace WorkerDispatcher
{
    public class ActionDispatcherFactory : IActionDispatcherFactory
    {
        private readonly IWorkerHandler _handler;

        public ActionDispatcherFactory(IWorkerHandler handler)
        {
            _handler = handler;
        }

        public ActionDispatcherFactory()
            : this(new DefaultWorkerHandler())
        { }

        public IDispatcherToken Start(ActionDispatcherSettings config)
        {
            var processCount = new CounterBlocked();

            var queueWorker = new QueueWorker();

            var cancellationTokenSource = new CancellationTokenSource();

            var dispatcherToken = new DispatcherToken(processCount, queueWorker, cancellationTokenSource);

            var schedule = config.BuildSchedule(queueWorker, processCount, _handler);

            schedule.Start(cancellationTokenSource.Token);

            return dispatcherToken;
        }
    }
}
