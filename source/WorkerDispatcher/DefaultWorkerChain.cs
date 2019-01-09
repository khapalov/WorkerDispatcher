using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkerDispatcher.Report;
using WorkerDispatcher.Workers;

namespace WorkerDispatcher
{
    internal class DefaultWorkerChain : IWorkerChain
    {
        private readonly IQueueWorker _queueWorker;
        private readonly ICounterBlocked _counterBlocked;
        private readonly ConcurrentQueue<IActionInvoker> _workerChainDefaults;

        public DefaultWorkerChain(IQueueWorker queueWorker, ICounterBlocked counterBloaked)
        {
            _queueWorker = queueWorker;
            _counterBlocked = counterBloaked;
            _workerChainDefaults = new ConcurrentQueue<IActionInvoker>();            
        }

        public void Run(IActionInvoker<WorkerCompletedData> compeltedInvoker)
        {
            IActionInvoker[] arr = ExtractArrray();

            var progressDatas = new WorkerProgressData[arr.Length];            

            var invokerSync = new InternalWorkerDataSync<WorkerCompletedData>(compeltedInvoker, _counterBlocked);

            var progress = new WorkerProgressReportCompleted(_queueWorker, progressDatas, invokerSync);
            
            var actionInvokers = arr.Select((p, i) => new InternalWorkerProgress(p, progress, i)).ToArray();
            
            _queueWorker.PostBulk(actionInvokers);
        }

        public void Run(Action<WorkerCompletedData> fn)
        {
            IActionInvoker[] arr = ExtractArrray();

            var progressDatas = new WorkerProgressData[arr.Length];

            var progress = new WorkerProgressReportCallback(progressDatas, fn);

            var actionInvokers = arr.Select((p, i) => new InternalWorkerProgress(p, progress, i)).ToArray();

            _queueWorker.PostBulk(actionInvokers);
        }

        public WorkerCompletedData RunSync()
        {
            IActionInvoker[] arr = ExtractArrray();

            var progressDatas = new WorkerProgressData[arr.Length];

            var result = default(WorkerCompletedData);

            using (var sync = new ManualResetEventSlim(false))
            {
                var progress = new WorkerProgressReportCallback(progressDatas, data =>
                {
                    result = data;
                    sync.Set();
                });

                var actionInvokers = arr.Select((p, i) => new InternalWorkerProgress(p, progress, i)).ToArray();

                _queueWorker.PostBulk(actionInvokers);

                sync.Wait();
            }

            return result;
        }

        public Task<WorkerCompletedData> RunAsync()
        {
            var taskCompletionSource = new TaskCompletionSource<WorkerCompletedData>();

            try
            {
                var result = RunSync();

                taskCompletionSource.SetResult(result);
            }
            catch (OperationCanceledException)
            {
                taskCompletionSource.TrySetCanceled();
            }
            catch (Exception ex)
            {
                taskCompletionSource.TrySetException(ex);
            }

            return taskCompletionSource.Task;
        }

        private IActionInvoker[] ExtractArrray()
        {
            var arr = _workerChainDefaults.ToArray();

            if (arr.Length == 0)
            {
                throw new ArgumentException("chain collection is empty");
            }

            return arr;
        }

        public IWorkerChain Chain()
        {
            return new DefaultWorkerChain(_queueWorker, _counterBlocked);
        }

        public IWorkerChain Post(IActionInvoker actionInvoker)
        {
            _workerChainDefaults.Enqueue(actionInvoker);

            return this;
        }

        public IWorkerChain Post<TData>(IActionInvoker<TData> actionInvoker, TData data, TimeSpan lifetime)
        {
            var internalWorkerValue = new InternalWorkerValueLifetime<TData>(actionInvoker, data, lifetime);

            _workerChainDefaults.Enqueue(internalWorkerValue);

            return this;
        }

        public IWorkerChain Post<TData>(IActionInvoker<TData> actionInvoker, TData data)
        {
            var internalWorkerWalue = new InternalWorkerValue<TData>(actionInvoker, data);

            _workerChainDefaults.Enqueue(internalWorkerWalue);

            return this;
        }

        public IWorkerChain Post(Func<CancellationToken, Task> fn)
        {
            var internalWorkerWalue = new InternalWorkerFunc(fn);

            _workerChainDefaults.Enqueue(internalWorkerWalue);

            return this;
        }
    }
}
