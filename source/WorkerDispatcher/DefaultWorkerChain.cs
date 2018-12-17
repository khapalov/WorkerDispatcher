using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerDispatcher
{
    internal class DefaultWorkerChain : IWorkerChain
    {
        private readonly IDispatcherTokenSender _sender;
        private readonly BlockingCollection<IActionInvoker> _workerChainDefaults;

        public DefaultWorkerChain(IDispatcherTokenSender sender)
        {
            _sender = sender;
            _workerChainDefaults = new BlockingCollection<IActionInvoker>();
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

            var progress = new WorkerProgressReportSync(progressDatas);

            for (var i = 0; i < arr.Length; i++)
            {
                _sender.Post(new InternalWorkerProgress(arr[i], progress, i));
            }

            using (progress)
            {
                return progress.WaitReady();
            }
        }

        private IActionInvoker[] ExtractArrray()
        {            
            _workerChainDefaults.CompleteAdding();
            
            var arr = _workerChainDefaults.ToArray();

            _workerChainDefaults.Dispose();

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
            _workerChainDefaults.Add(actionInvoker);

            return this;
        }

        public IWorkerChain Post<TData>(IActionInvoker<TData> actionInvoker, TData data, TimeSpan lifetime)
        {
            var internalWorkerValue = new InternalWorkerValueLifetime<TData>(actionInvoker, data, lifetime);

            _workerChainDefaults.Add(internalWorkerValue);

            return this;
        }

        public IWorkerChain Post<TData>(IActionInvoker<TData> actionInvoker, TData data)
        {
            var internalWorkerWalue = new InternalWorkerValue<TData>(actionInvoker, data);

            _workerChainDefaults.Add(internalWorkerWalue);

            return this;
        }
    }
}
