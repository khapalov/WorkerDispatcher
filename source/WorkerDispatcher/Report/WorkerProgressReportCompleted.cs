using WorkerDispatcher.Workers;

namespace WorkerDispatcher.Report
{
    internal class WorkerProgressReportCompleted : WorkerProgressReportBase
    {
        private readonly IActionInvoker<WorkerCompletedData> _actionInvoker;
        private readonly IQueueWorker _queueWorker;

        public WorkerProgressReportCompleted(IQueueWorker sender, 
            WorkerProgressData[] datas, 
            IActionInvoker<WorkerCompletedData> actionInvoker) 
            : base(datas)
        {
            _queueWorker = sender;
            _actionInvoker = actionInvoker;
        }

        protected override void OnComplete(WorkerCompletedData datas)
        {
            var worker = new InternalWorkerValue<WorkerCompletedData>(_actionInvoker, datas);

            _queueWorker.Post(worker);
        }
    }    
}
