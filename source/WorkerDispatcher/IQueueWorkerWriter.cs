namespace WorkerDispatcher
{
    public interface IQueueWorkerWriter
    {
        void Post(IActionInvoker actionInvoker);

        void PostBulk(IActionInvoker[] actionInvokers);

        void Complete();
    }
}
