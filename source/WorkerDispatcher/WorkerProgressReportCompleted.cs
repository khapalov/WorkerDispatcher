using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerDispatcher
{
    internal class WorkerProgressReportCompleted : WorkerProgressReportBase
    {
        private readonly IActionInvoker<WorkerCompletedData> _actionInvoker;
        private readonly IDispatcherTokenSender _sender;

        public WorkerProgressReportCompleted(IDispatcherTokenSender sender, WorkerProgressData[] datas, IActionInvoker<WorkerCompletedData> actionInvoker) 
            : base(datas)
        {
            _sender = sender;
        }

        protected override void OnComplete(WorkerCompletedData datas)
        {
            _sender.Post(_actionInvoker, datas);
        }
    }    
}
