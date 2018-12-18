using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WorkerDispatcher.Workers;

namespace WorkerDispatcher
{
    internal class DefaultWorkerChain : IWorkerChain
    {
        private readonly IDispatcherTokenSender _sender;
        private readonly ConcurrentQueue<IActionInvoker> _workerChainDefaults;

        public DefaultWorkerChain(IDispatcherTokenSender sender)
        {
            _sender = sender;
            _workerChainDefaults = new ConcurrentQueue<IActionInvoker>();
        }

        public void Run(IActionInvoker<WorkerCompletedData> compeltedInvoker)
        {
            IActionInvoker[] arr = ExtractArrray();

            var progressDatas = new WorkerProgressData[arr.Length];

            var progress = new WorkerProgressReportCompleted(_sender, progressDatas, compeltedInvoker);

            for (var i = 0; i < arr.Length; i++)
            {
                _sender.Post(new InternalWorkerProgress(arr[i], progress, i));
            }
        }

        public void Run(Action<WorkerCompletedData> fn)
        {
            IActionInvoker[] arr = ExtractArrray();

            var progressDatas = new WorkerProgressData[arr.Length];

            var progress = new WorkerProgressReportCallback(progressDatas, fn);

            for (var i = 0; i < arr.Length; i++)
            {
                _sender.Post(new InternalWorkerProgress(arr[i], progress, i));
            }
        }

        public WorkerCompletedData RunSync()
        {
            IActionInvoker[] arr = ExtractArrray();

            var progressDatas = new WorkerProgressData[arr.Length];

            using (var progress = new WorkerProgressReportSync(progressDatas))
            {
                for (var i = 0; i < arr.Length; i++)
                {
                    _sender.Post(new InternalWorkerProgress(arr[i], progress, i));
                }

                return progress.WaitReady();
            }
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
            return new DefaultWorkerChain(_sender);
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
