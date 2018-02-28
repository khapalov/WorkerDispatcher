using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace WorkerDispatcher
{
    internal class DefaultWorkerRunner : IWorkerRunner
    {
        private readonly ICounterBlocked _counterBlocked;
        private readonly IWorkerHandler _workerHandler;
        private readonly TimeSpan _timeLimit;

        public DefaultWorkerRunner(ICounterBlocked counterBlocked, IWorkerHandler workerHandler, TimeSpan timeLimit)
        {
            _counterBlocked = counterBlocked;
            _workerHandler = workerHandler;
            _timeLimit = timeLimit;
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
                tokenSource.CancelAfter(_timeLimit);

                await ProcessMessage(actionInvoker, tokenSource.Token);
            }
            catch (Exception ex)
            {
                _workerHandler.HandleFault(ex);
            }
            finally
            {
                _counterBlocked.Decremenet();
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
