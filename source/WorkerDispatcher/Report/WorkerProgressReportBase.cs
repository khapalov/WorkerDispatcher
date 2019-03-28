using System;

namespace WorkerDispatcher.Report
{
    internal abstract class WorkerProgressReportBase : Progress<WorkerProgressData>
    {
        private readonly object _sync = new object();
        private readonly WorkerProgressData[] _datas;

        private volatile int _countWorker;

        public WorkerProgressReportBase(WorkerProgressData[] datas)
        {
            _countWorker = datas.Length;
            _datas = datas;
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
                OnComplete(new WorkerCompletedData
                {
                    Results = _datas
                });                
            }
        }

        protected abstract void OnComplete(WorkerCompletedData datas);       
    }
}
