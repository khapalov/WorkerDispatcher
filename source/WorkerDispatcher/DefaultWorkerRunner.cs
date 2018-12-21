using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WorkerDispatcher.Workers;

namespace WorkerDispatcher
{
    internal class DefaultWorkerRunner : IWorkerRunner
    {
        private readonly ICounterBlocked _counterBlocked;
        private readonly IWorkerHandler _workerHandler;
        private readonly TimeSpan _timeLimit;
        private readonly IWorkerNotify _queueWorker;

        public DefaultWorkerRunner(ICounterBlocked counterBlocked, IWorkerHandler workerHandler, TimeSpan timeLimit, IWorkerNotify queueWorker)
        {
            _counterBlocked = counterBlocked;
            _workerHandler = workerHandler;
            _timeLimit = timeLimit;
            _queueWorker = queueWorker;
        }

        public async Task ExcecuteInvoker(IActionInvoker actionInvoker)
        {
            var tokenSource = new CancellationTokenSource();

            try
            {
                _counterBlocked.Increment();
#if DEBUG
                Trace.WriteLine(String.Format("start process count = {0}", _counterBlocked.Count));
#endif

				if (actionInvoker is IActionInvokerLifetime invokerTimeLimit)
				{
					tokenSource.CancelAfter(invokerTimeLimit.Lifetime);
				}
				else
				{
					tokenSource.CancelAfter(_timeLimit);
				}
				
				await ProcessMessage(actionInvoker, tokenSource.Token);
            }
            catch (Exception ex)
            {
                _workerHandler.HandleFault(ex);
            }
            finally
            {
                _counterBlocked.Decremenet();
                _queueWorker.SetWorkerEnd();
                tokenSource.Dispose();
            }

#if DEBUG
            Trace.WriteLine(String.Format("stop process, quantity left = {0}", _counterBlocked.Count));
#endif
        }

        protected virtual async Task ProcessMessage(IActionInvoker actionInvoker, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            await actionInvoker.Invoke(cancellationToken).ContinueWith(async actionResultTask =>
            {
                stopwatch.Stop();

                if (actionResultTask.IsCanceled || actionResultTask.IsFaulted)
                {
                    _workerHandler.HandleError(actionResultTask.Exception, stopwatch.ElapsedMilliseconds, actionResultTask.IsCanceled);
                }
                else
                {
                    var result = await actionResultTask;
                    _workerHandler.HandleResult(result, stopwatch.ElapsedMilliseconds);
                }
            });
        }
    }
}
