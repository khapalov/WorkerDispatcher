using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher.ScheduleStrategies
{
    internal abstract class ScheduleStrategiesBase : IScheduleStrategies
    {
        private readonly ICounterBlocked _processCount;
        private readonly TimeSpan _timeLimit;
        private readonly IWorkerHandler _handler;

        public ScheduleStrategiesBase(ICounterBlocked processCount, TimeSpan timeLimit, IWorkerHandler workerHandler)
        {
            _processCount = processCount;
            _timeLimit = timeLimit;
            _handler = workerHandler;
        }

        public abstract void Start(CancellationToken cancellationToken);

        protected async Task ExcecuteInvoker(IActionInvoker actionInvoker)
        {
            var tokenSource = new CancellationTokenSource();

            try
            {
                _processCount.Increment();

                Trace.WriteLine(String.Format("start process count = {0}", _processCount.Count));

                tokenSource.CancelAfter(_timeLimit);

                await ProcessMessage(actionInvoker, tokenSource.Token);
            }
            catch (Exception ex)
            {
                _handler.HandleFault(ex);
            }
            finally
            {
                _processCount.Decremenet();
                tokenSource.Dispose();
            }

            Trace.WriteLine(String.Format("stop process, quantity left = {0}", _processCount.Count));
        }

        protected virtual async Task ProcessMessage(IActionInvoker actionInvoker, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();            

            await actionInvoker.Invoke(cancellationToken).ContinueWith(async actionResultTask =>
            {
                stopwatch.Stop();

                if (actionResultTask.IsCanceled || actionResultTask.IsFaulted)
                {
                    _handler.HandleError(actionResultTask.Exception, stopwatch.ElapsedMilliseconds, actionResultTask.IsCanceled);
                }
                else
                {
                    var result = await actionResultTask;
                    _handler.HandleResult(result, stopwatch.ElapsedMilliseconds);
                }
            });
        }
    }
}
