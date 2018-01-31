using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

            Task.Factory.StartNew(async () =>
            {
                var cancellationToken = cancellationTokenSource.Token;

                while (!cancellationToken.IsCancellationRequested || !queueWorker.IsEmpty)
                {
                    var configValue = config;

                    await queueWorker.ReceiveAsync()
                        .ContinueWith(async invoker => ProcessMessageInner(await invoker, configValue, processCount), TaskContinuationOptions.OnlyOnRanToCompletion);
                }

            }, TaskCreationOptions.LongRunning);

            return dispatcherToken;
        }

        private async Task ProcessMessageInner(IActionInvoker actionInvoker, ActionDispatcherSettings config, CounterBlocked processCount)
        {
            var tokenSource = new CancellationTokenSource();

            try
            {
                processCount.Increment();
#if DEBUG
                Trace.WriteLine(String.Format("start process count = {0}", processCount.Count));
#endif
                tokenSource.CancelAfter(config.Timeout);

                await ProcessMessage(actionInvoker, tokenSource.Token);
            }
            catch (Exception ex)
            {
                _handler.HandleFault(ex);
            }
            finally
            {
                processCount.Decremenet();
                tokenSource.Dispose();
            }
#if DEBUG
            Trace.WriteLine(String.Format("stop process count = {0}", processCount.Count));
#endif
        }

        private async Task ProcessMessage(IActionInvoker actionInvoker, CancellationToken cancellationToken)
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
