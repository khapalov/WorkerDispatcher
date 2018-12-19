using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher.Report
{
    internal class WorkerProgressReportCallback : WorkerProgressReportBase
    {
        private readonly Action<WorkerCompletedData> _action;

        public WorkerProgressReportCallback(WorkerProgressData[] datas, Action<WorkerCompletedData> action)
            : base(datas)
        {
            _action = action;
        }

        protected override void OnComplete(WorkerCompletedData datas)
        {
            _action(datas);
        }
    }
}
