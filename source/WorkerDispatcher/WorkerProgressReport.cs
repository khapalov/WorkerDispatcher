using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WorkerDispatcher
{
    internal class WorkerProgressReport : Progress<WorkerProgressData>
    {
        private readonly IDispatcherTokenSender _sender;
        private readonly WorkerProgressData[] _datas;
        private readonly Action<WorkerCompletedData> _fn;
        private readonly IActionInvoker<WorkerCompletedData> _actionInvoker;
        private readonly object _sync = new object();

        private volatile int _countWorker;

        public WorkerProgressReport(IDispatcherTokenSender sender, WorkerProgressData[] datas, IActionInvoker<WorkerCompletedData> actionInvoker)
        {
            _sender = sender;
            _countWorker = datas.Length;
            _datas = datas;
            _actionInvoker = actionInvoker;
            _fn = (WorkerCompletedData data) => _sender.Post(_actionInvoker, new WorkerCompletedData { Results = _datas });
        }

        public WorkerProgressReport(IDispatcherTokenSender sender, WorkerProgressData[] datas, Action<WorkerCompletedData> fn)
        {
            _sender = sender;
            _countWorker = datas.Length;
            _datas = datas;
            _fn = fn;
        }

        protected override void OnReport(WorkerProgressData value)
        {
            base.OnReport(value);

            _datas[value.Index] = value;

            var complete = _countWorker == 0;

            lock (_sync)
            {
                _countWorker--;
                complete = _countWorker == 0;
            }

            if (complete)
            {
                _fn(new WorkerCompletedData
                {
                    Results = _datas
                });

            }
        }
    }
}
